using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StringSplitOptions = System.StringSplitOptions;

namespace ConsoleAppWordProcess
{
    public class TranslatorService
    {
        public static List<string> ServerList = new List<string>()
        {
            "localhost",
            "68.183.96.203",
            "157.230.54.116",
            "157.230.49.211",
            "157.230.54.219",
            "157.230.54.87",
            "157.230.60.220",
            "157.230.57.244",
            "157.230.12.253",
            "157.230.10.77",
            "134.209.217.97",
        };
        public static async Task<string> Translate(string from, string to, string body, int server)
        {
            HttpResponseMessage res;
            using (var hc = new HttpClient())
            {

                res = await hc.PostAsync($"http://{ServerList[server]}:8734/TranslateService", new MultipartFormDataContent
                {
                    {new StringContent(body), "MessageToTranslate"},
                    {new StringContent(from), "FromLanguage"},
                    {new StringContent(to), "ToLanguage"}
                });
            }

            if (!res.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Blocked {ServerList[server]}\t{body}\t{to}\t{DateTime.Now.ToString("HH:mm:ss")}");

                throw new Exception(await res.Content.ReadAsStringAsync());
            }

            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<string> TranslateGetAll(string from, string to, string body, int server)
        {
            HttpResponseMessage res;
            using (var hc = new HttpClient())
            {

                res = await hc.PostAsync($"http://{ServerList[server]}:8734/TranslateGetAll", new MultipartFormDataContent
                {
                    {new StringContent(body), "MessageToTranslate"},
                    {new StringContent(from), "FromLanguage"},
                    {new StringContent(to), "ToLanguage"}
                });
            }

            if (!res.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Blocked {ServerList[server]}\t{body}\t{to}\t{DateTime.Now.ToString("HH:mm:ss")}");

                throw new Exception(await res.Content.ReadAsStringAsync());
            }

            return await res.Content.ReadAsStringAsync();



        }
    }
    public class TrasnlateService
    {
        public static void StartTask()
        {
            while (true)
            {
                TranslateResourceData();
            }
        }

        private static void TrasnaltePerServer(List<GetWordForTranslate_Result> dataList, int serverId)
        {


            foreach (var data in dataList)
            {
                var dateTime = DateTime.Now;
                using (var service2 = new ResourceService())
                {
                    if (!string.IsNullOrEmpty(data.Word) &&
                        "en" != data.LanguageCode.ToLower())
                    {
                        try
                        {
                            var resultT = TranslatorService.TranslateGetAll(
                                "en",
                                data.LanguageCode,
                                data.Word,
                                serverId);
                            resultT.Wait();

                            data.Translated = resultT.Result;
                            var objectT = JsonConvert.DeserializeObject<CallBankService>(data.Translated);
                            if (!string.IsNullOrEmpty(data.Translated))
                            {
                                service2.SaveResourceTranslated(data, objectT);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine(
                                    $"{serverId}\t{TranslatorService.ServerList[serverId]}\t{data.Word}\t{data.LanguageCode}\t{objectT.Text}\t{DateTime.Now:HH:mm:ss}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                var waitMilisecond = 1100;
                var sleep = (DateTime.Now - dateTime).TotalMilliseconds;
                //Console.WriteLine($"Time To call all {sleep}");
                if (sleep < waitMilisecond)
                {
                    var wait = waitMilisecond - (int)sleep;
                    Task.Delay(wait).Wait();
                    //Console.WriteLine($"Time need wait {wait}");
                }
                var rondomNumber = new Random().Next(1, 300);
                //Console.Write("\tRondomNumber: " + rondomNumber + "\t");
                Task.Delay(rondomNumber).Wait();
            }
            //if (serverId == 0)
            //{
            //    System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            //    Environment.Exit(-1);
            //}

        }

        private static void TranslateResourceData()
        {
            using (var service = new ResourceService())
            {
                var dataList = service.GetResourceNeedTranslate();
                if (dataList != null)
                {

                    var currenTask = new List<Task>();

                    var threadCount = 10;//TranslatorService.ServerList.Count;
                    var count = dataList.Count / threadCount;


                    for (var serverID = 0; serverID < threadCount; serverID++)
                    {
                        var id = serverID;
                        currenTask.Add(Task.Factory.StartNew(() =>
                        {
                            if (id < threadCount)
                            {

                                var forLast = dataList.Count - (id * count);
                                var range = dataList.GetRange(count * id,
                                    id + 1 == threadCount ? forLast : count);
                                Console.WriteLine("Start Thread " + id + $" List: {range.Count}");
                                TrasnaltePerServer(range, id);
                            }

                        }));
                    }
                    Task.WaitAll(currenTask.ToArray());




                }
            }


        }

        public static void ImportWords()
        {
            var wordText = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Import_Word.txt");
            var words = wordText.Split(new char[] { ',', '\"', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var lists = words.Select(x => new AllWordFromPaymon
            {
                Word = x.Trim(),
            }).Where(x => !string.IsNullOrEmpty(x.Word)).ToList();
            Console.WriteLine("Intering..");
            var entity = new EnglishWordsEntities();
            entity.BulkInsert(lists);
            Console.WriteLine("Inserted");
        }
    }
    public partial class CallBankService
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("all")]
        public List<string> All { get; set; }

        [JsonProperty("from")]
        public From From { get; set; }

        [JsonProperty("raw")]
        public string Raw { get; set; }
    }

    public partial class From
    {
        [JsonProperty("language")]
        public Language Language { get; set; }

        [JsonProperty("text")]
        public Text Text { get; set; }
    }

    public partial class Language
    {
        [JsonProperty("didYouMean")]
        public bool DidYouMean { get; set; }

        [JsonProperty("iso")]
        public string Iso { get; set; }
    }

    public partial class Text
    {
        [JsonProperty("autoCorrected")]
        public bool AutoCorrected { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("didYouMean")]
        public bool DidYouMean { get; set; }
    }
}
