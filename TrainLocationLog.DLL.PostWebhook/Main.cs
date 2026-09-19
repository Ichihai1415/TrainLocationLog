using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TrainLocationLog.DLL.PostWebhook
{
    public class PostWebhook
    {
        public void Run(string num, string line)
        {
            //Post("sandbox", "test: " + num + " : " + line);
            var data = line.Split(',');
            var company = data[1];
            if (company == "HF")
                HF_OtherDetect(num, line);
        }
        //csv= "dateTime,company,type,from,to,delay,other\n"

        List<string> nums = [];

        private void HF_OtherDetect(string num, string line)
        {
            if (nums.Contains(num))
                return;
            nums.Add(num);
            var flag = false;
            if (num.StartsWith('雪') || num.StartsWith('単') || num.StartsWith('試') || num.StartsWith('配'))
                flag = true;
            //var data = line.Split(',');
            else
            {
                var num_num = int.Parse(Regex.Replace(num, @"\D", ""));
                if (num_num >= 6000)
                    flag = true;
            }
            if (flag)
                Post("HF", "detect: " + num + " : " + line);

        }

        HttpClient client = new();
        Dictionary<string, string> urls = [];
        private void Post(string urlKey, string message)
        {
            string url;
            if (urls.TryGetValue(urlKey, out string? value))
            {
                url = value;
            }
            else
            {
                var path = "plugin\\TrainLocationLog.DLL.PostWebhook.URL_" + urlKey + ".txt";
                if (File.Exists(path))
                {
                    url = File.ReadAllText(path);
                    urls[urlKey] = url;
                }
                else
                {
                    if (urlKey == "sandbox")
                        return;
                    Console.WriteLine($"URLファイルが見つかりません: {path}");
                    File.WriteAllText(path, "");
                    return;
                }
                url = File.ReadAllText(path);
                if (!url.StartsWith("http"))
                {
                    Console.WriteLine($"URLが設定されていません: {path}");
                    return;
                }
                urls[urlKey] = url;
            }

            var obj = new
            {
                content = message
            };

            var json = JsonSerializer.Serialize(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = client.PostAsync(url, content).Result;
            Console.WriteLine("POST: " + res.StatusCode);

            //Console.WriteLine($"送信中...");
            //Task.Run(async () =>
            //{
            //    using var client = new HttpClient();
            //    var res = await client.PostAsync(url, content);

            //    Console.WriteLine("POST: " + res.StatusCode);

            //});
        }
    }
}