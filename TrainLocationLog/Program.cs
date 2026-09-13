using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TrainLocationLog;

var client = new HttpClient();

var hf_hrm_carType = client.GetAsync("https://trainlocation.hapi-line.co.jp/config/car_type.json").Result;
var hf_carType = JsonSerializer.Deserialize<HF_car_type>(hf_hrm_carType.Content.ReadAsStringAsync().Result)!;
var hf_carType_dict = hf_carType.Type.ToDictionary(x => x.Code, x => x.Name);
Console.WriteLine("HF_carType loaded.");

var hf_hrm_station = client.GetAsync("https://trainlocation.hapi-line.co.jp/config/station.json").Result;
var hf_station = JsonSerializer.Deserialize<HF_station>(hf_hrm_station.Content.ReadAsStringAsync().Result)!;
var hf_station_dict = hf_station.Ikisaki.ToDictionary(x => x.Code, x => x.Name);
Console.WriteLine("HF_station loaded.");

var ir_hrm_definitions = client.GetAsync("https://www.ishikawa-railway.jp/api/v1/definitions").Result;
var ir_definitions = JsonNode.Parse(ir_hrm_definitions.Content.ReadAsStringAsync().Result)!;
var ir_definitions_dict = ir_definitions.AsArray().ToDictionary(x => x!["@type"]!.ToString(), x => x!.AsObject().ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()));
var ir_carType_dict = ir_definitions_dict["odpt:TrainType"];
Console.WriteLine("IR_carType loaded.");
var ir_station_dict = ir_definitions_dict["odpt:Station"];
Console.WriteLine("IR_station loaded.");
var ir_direction_dict = ir_definitions_dict["odpt:RailDirection"];
Console.WriteLine("IR_direction loaded.");

var ak_hrm_station = client.GetAsync("https://trafficinfo.ainokaze.co.jp/api/json/station.json").Result;
var ak_station = JsonSerializer.Deserialize<AK_station>(ak_hrm_station.Content.ReadAsStringAsync().Result)!;
var ak_station_dict = ak_station.Result.Data.Where(x => x.JsonName != null).ToDictionary(x => x.JsonName, x => x.StationNameJa);
Console.WriteLine("AK_station loaded.");

System.Timers.Timer timer;
Console.WriteLine("init finish.\n");

ScheduleNext();
Thread.Sleep(Timeout.Infinite);

void ScheduleNext()
{
    var now = DateTime.Now;
    var next = new DateTime(
        now.Year, now.Month, now.Day,
        now.Hour, now.Minute, 15);

    if (now.Second >= 14)//14.999とか対策で14
    {
        next = next.AddMinutes(1);
    }

    double wait = (next - now).TotalMilliseconds;
    if (wait < 0) wait = 0;

    timer = new System.Timers.Timer(wait)
    {
        AutoReset = false
    };
    timer.Elapsed += (_, __) =>
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in Run(): {ex}");
            throw;
        }
        finally
        {
            ScheduleNext();
        }
    };
    timer.Start();
}

void Run()
{
    Console.Write("Now  : ");
    Console.WriteLine(DateTime.Now);
    HF();
    IRAK();
    Console.WriteLine();
}

void HF()
{
    /*
     id :列番%2
    E102D:1
    E114D:1
    E114U:0
    D106:1
    U103:0
    E=駅停車　D:下り？U:上り
     */
    var res_hrm = client.GetAsync("https://trainlocation.hapi-line.co.jp/data/traffic_info.json?" + DateTime.Now.Ticks).Result;
    var res = res_hrm.Content.ReadAsStringAsync().Result;
    var json = JsonNode.Parse(res);

    var dt = json["UP"][0]["dt"].ToString();

    Console.Write("HF dt: ");
    Console.WriteLine(dt);

    foreach (var key in new string[] { "TS", "EK" })
        foreach (var el in json[key]?.AsArray() ?? [])
        {
            var id = el["id"].ToString();
            var isE = id.Contains('E');
            var isU = id.Contains('U');
            var isD = id.Contains('D');
            var id_num = Regex.Replace(id, @"\D", "");
            foreach (var tr in el["tr"].AsArray())
            {
                var no = tr["no"].ToString();
                var bs = tr["bs"].ToString();
                var sy = tr["sy"].ToString();
                var ik = tr["ik"].ToString();
                var dl = tr["dl"].ToString();
                var hk = tr["hk"].ToString();
                hf_carType_dict.TryGetValue(sy, out var syName);
                hf_station_dict.TryGetValue(ik, out var ikName);
                hf_station_dict.TryGetValue(id_num, out var idName);
                syName ??= "null";
                ikName ??= "null";
                idName ??= "null";
                var line = $"{dt},HF,{syName},{(isE ? idName : "(前駅)")},{(isE ? "null" : idName)},{dl},key={key}/id={id}/bs={bs}/hk={hk}\n";
                Console.WriteLine($"  {no} {syName}  {(isU ? "上り" : isD ? "下り" : "")} {(ikName == "null" ? "" : (ikName + "行 "))} {(!isE ? "(前駅)～" : "")}{idName}{(isE ? "付近" : "")}  delay={dl}  bs={bs} hk={hk}");
                AddCsv("HF", no, line);
            }
        }

}

void IRAK()
{
    foreach (var comp in new string[] { "IR", "AK" })
    {
        var res_hrm = client.GetAsync(comp == "IR" ? "https://www.ishikawa-railway.jp/api/v1/trains?" + DateTime.Now.Ticks : "https://trafficinfo.ainokaze.co.jp/api//json/train.json?" + DateTime.Now.Ticks).Result;
        var res = res_hrm.Content.ReadAsStringAsync().Result.Replace("@", "");
        var json = comp == "IR" ? JsonSerializer.Deserialize<OPDT[]>(res) : JsonSerializer.Deserialize<OPDT[]>(JsonSerializer.Deserialize<AK_data>(res)!.Result.Data);

        if (json.Length == 0)
            return;
        var dt = json[0].DcDate.ToString();
        Console.Write(comp);
        Console.Write(" dt: ");
        Console.WriteLine(dt);

        foreach (var tr in json)
        {
            dt = tr.DcDate.ToString();
            var num = RemoveTop0F(tr.OdptTrainNumber);
            var type = comp == "IR" ? ir_carType_dict[tr.OdptTrainType] : tr.OdptTrainType;
            var delay = tr.OdptDelay;
            var start = StationConverter(tr.OdptStartingStation, comp);
            var terminal = StationConverter(tr.OdptTerminalStation, comp);
            var from = StationConverter(tr.OdptFromStation, comp);
            var to = StationConverter(tr.OdptToStation, comp);
            var dir = comp == "IR" ? ir_direction_dict[tr.OdptRailDirection] : tr.OdptRailDirection.Replace("行き", "");

            var line = $"{dt},{comp},{type},{from},{to},{delay},{start}始発 {dir} {terminal}行\n";
            Console.WriteLine($"  {num} {type}  {start}始発 {dir} {(terminal == "null" ? "" : (terminal + "行"))}  {from}{(to == "null" ? "付近" : ("～" + to))}  delay={delay}");
            AddCsv(comp, num, line);
        }


    }
}

string StationConverter(string? stationCode, string comp)
{
    stationCode ??= "null";
    if (stationCode == "null")
        return "null";
    return comp == "IR" ? ir_station_dict[stationCode] : ak_station_dict[stationCode];
}

void AddCsv(string comp, string no, string line)
{
    var eDt = DateTime.Now - TimeSpan.FromHours(3);//25時までカウント
    var dir = $"log\\{comp}\\{eDt:yyyyMM}\\{eDt:dd}";
    Directory.CreateDirectory(dir);
    var path = Path.Combine(dir, no + ".csv");
    if (File.Exists(path))
    {
        File.AppendAllText(path, line);
    }
    else
    {
        File.WriteAllText(path, "dateTime,company,type,from,to,delay,other\n" + line);
    }
    return;
}

string RemoveTop0F(string str)
{
    while (str.Length > 0 && (str[0] == '0' || str[0] == 'F'))
        str = str.Substring(1);
    return str;
}

/*
 "TS": [
{
"id": "E111U",　列車位置？
"tr": [
{
"no": "1240M",　列番
"bs": "1",
"sy": "1",　種別
"ik": "101",   行先
"dl": "0",　　遅延？
"hk": "0"　　　
}
 */
