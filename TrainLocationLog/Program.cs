using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TrainLocationLog;

var client = new HttpClient();

var hf_jsonSt_carType = client.GetAsync("https://trainlocation.hapi-line.co.jp/config/car_type.json").Result;
var hf_carType = JsonSerializer.Deserialize<HF_car_type>(hf_jsonSt_carType.Content.ReadAsStringAsync().Result);
var hf_carType_dict = hf_carType.type.ToDictionary(x => x.code, x => x.name);

var hf_jsonSt_station = client.GetAsync("https://trainlocation.hapi-line.co.jp/config/station.json").Result;
var hf_station = JsonSerializer.Deserialize<HF_station>(hf_jsonSt_station.Content.ReadAsStringAsync().Result);
var hf_station_dict = hf_station.ikisaki.ToDictionary(x => x.code, x => x.name);

System.Timers.Timer timer;
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
    HF();
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
    var res = client.GetAsync("https://trainlocation.hapi-line.co.jp/data/traffic_info.json").Result;
    var json = JsonNode.Parse(res.Content.ReadAsStringAsync().Result);

    var dt = json["UP"][0]["dt"].ToString();
    Console.Write(DateTime.Now);
    Console.Write(" - ");
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
                syName ??= "[未定義]";
                ikName ??= "[未定義]";
                idName ??= "[未定義]";
                var line = $"{dt},{key},{id},{bs},{sy},{ik},{dl},{hk}\n";
                Console.WriteLine($"  [{key}] {no} id={id}/{(!isE ? "前駅～" : "")}{idName}{(isU ? "(上り)" : isD ? "(下り)" : "")} bs={bs} sy={sy}/{syName} ik={ik}/{ikName} dl={dl} hk={hk}");
                AddCsv(no, line);
            }
        }
    Console.WriteLine();

}


void AddCsv(string no, string line)
{
    var eDt = DateTime.Now - TimeSpan.FromHours(3);//25時までカウント
    var dir = $"log\\hapi\\{eDt:yyyyMM}\\{eDt:dd}";
    Directory.CreateDirectory(dir);
    var path = Path.Combine(dir, no + ".csv");
    if (File.Exists(path))
    {
        File.AppendAllText(path, line);
    }
    else
    {
        File.WriteAllText(path, "dt,TS/EK,id,no,bs,sy,ik,dl,hk\n" + line);
    }
    return;
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
