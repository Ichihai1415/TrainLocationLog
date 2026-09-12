using System.Text;
using System.Text.Json.Nodes;

var client = new HttpClient();


System.Timers.Timer timer;
ScheduleNext();
Thread.Sleep(Timeout.Infinite);


void ScheduleNext()
{
    var now = DateTime.Now;
    var next = new DateTime(
        now.Year, now.Month, now.Day,
        now.Hour, now.Minute, 15);

    if (now.Second >= 15)
    {
        next = next.AddMinutes(1);
    }

    double wait = (next - now).TotalMilliseconds;

    timer = new System.Timers.Timer(wait)
    {
        AutoReset = false
    };
    timer.Elapsed += (_, __) =>
    {
        Run();
        ScheduleNext();
    };
    timer.Start();
}

void Run()
{

    var res = client.GetAsync("https://trainlocation.hapi-line.co.jp/data/traffic_info.json").Result;

    var json = JsonObject.Parse(res.Content.ReadAsStringAsync().Result);

    var dt = json["UP"][0]["dt"].ToString();
    Console.WriteLine(dt);
    foreach (var ts in json["TS"].AsArray())
    {
        var id = ts["id"].ToString();
        foreach (var tr in ts["tr"].AsArray())
        {
            var no = tr["no"].ToString();
            var bs = tr["bs"].ToString();
            var sy = tr["sy"].ToString();
            var ik = tr["ik"].ToString();
            var dl = tr["dl"].ToString();
            var hk = tr["hk"].ToString();
            var line = $"{dt},TS,{bs},{sy},{ik},{dl},{hk}\n";
            AddCsv(no, line);
        }

    }
    foreach (var ek in json["EK"].AsArray())
    {
        var id = ek["id"].ToString();
        foreach (var tr in ek["tr"].AsArray())
        {
            var no = tr["no"].ToString();
            var bs = tr["bs"].ToString();
            var sy = tr["sy"].ToString();
            var ik = tr["ik"].ToString();
            var dl = tr["dl"].ToString();
            var hk = tr["hk"].ToString();
            var line = $"{dt},EK,{bs},{sy},{ik},{dl},{hk}\n";
            AddCsv(no, line);
        }
    }
}

void AddCsv(string no, string line)
{
    var path = "log\\hapi\\" + no + ".csv";
    Directory.CreateDirectory("log\\hapi");
    if (File.Exists(path))
    {
        var csv = File.ReadAllText(path);
        File.WriteAllText(path, csv + line);

    }
    else
    {
        File.WriteAllText(path, "dt,TS/EK,id,no,bs,sy,ik,dl,hk");
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
