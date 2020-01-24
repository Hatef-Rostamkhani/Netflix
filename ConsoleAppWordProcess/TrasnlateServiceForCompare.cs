using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StringSplitOptions = System.StringSplitOptions;

namespace ConsoleAppWordProcess
{
    public class TranslatorServiceCompare
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
        public static object ob1 = 1;
        public static void ProcessAll()
        {
            Console.Clear();
            Console.WriteLine("Start ProcessAll");
            EnglishWordsEntities entity = new EnglishWordsEntities();
            var langauages = entity.WordTranslates.Where(x => x.Proccessed == null).Select(x => (int?)x.LanguageId).Distinct()
                .Take(1).FirstOrDefault();
            List<Task> taskList = new List<Task>();

            if (!langauages.HasValue)
                return;
            var dataList = GetData(langauages.Value);
            var count = dataList.Count / 10;
            for (var id = 0; id < 10; id++)
            {

                var forLast = dataList.Count - (id * count);
                var range = dataList.GetRange(count * id,
                    id + 1 == 10 ? forLast : count);
                Console.WriteLine("Start Thread " + id + $" List: {range.Count}");
                //  taskList.Add(Task.Factory.StartNew(() => ProcessTranslateFiles(langauages.Value, range)));
            }

            Task.WaitAll(taskList.ToArray());
            ProcessAll();
        }

        public static List<Result1> GetData(int languageId)
        {
            EnglishWordsEntities entity = new EnglishWordsEntities();
            entity.Database.CommandTimeout = int.MaxValue;

            List<Result1> allData = new List<Result1>();
            Console.WriteLine("Fetching before lock Data...");
            lock (ob1)
            {
                Console.WriteLine("Fetching Data...");
                allData = (from f in entity.AllWordFromPaymons
                           join t in entity.WordTranslates on f.ID equals t.WordID
                           where t.LanguageId == languageId &&
                                 t.Proccessed == null //f.IsPrimary == true && t.LanguageId == lan.ID
                           orderby f.Word

                           select new Result1
                           {
                               WordId = t.WordID,
                               LanId = t.LanguageId,
                               Word = f.Word,
                               AllData = t.AllData,
                               First = "",
                           }).Take(10000).ToList();
            }
            Console.WriteLine("Fetched Data");
            return allData;
        }
        public static WordProcessed ProcessTranslateFiles(string word, WordTranslateCompare allData)
        {

            bool Verified = false;
            string First = "";
            Console.WriteLine("Processing...");

            var dicAll = new List<string>();

            var objectT = JsonConvert.DeserializeObject<CallBankService>(allData.AllData);
            var body = JsonConvert.DeserializeObject<JArray>(objectT.Raw);

            if (body.Count > 0)
            {
                var first = body.First<JToken>();
                var item = first.FirstOrDefault();
                if (item != null)
                {
                    Verified = item[4].Value<int>() == 1;
                    First = item[0].Value<string>();
                }
            }


            if (body.Count > 5 && body[5].HasValues && !Verified)
            {
                var item = body[5].FirstOrDefault();
                if (item != null && item[3].HasValues && item[2].HasValues && item[3].Any())
                {
                    var googleWord = item[0].Value<string>().ToLower();
                    var myword = word.ToLower();
                    if (googleWord != myword)
                    {
                        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\DeffWords.txt",
                            "\r\n" + myword + "\t" + googleWord);
                    }
                    var goodRank = item[3][0].Values<int>().Select((r, i) => new
                    {
                        Index = i,
                        Rank = r,
                    }).OrderByDescending(x => x.Rank).FirstOrDefault();
                    if (goodRank != null)
                    {
                        if (item[2].Count() > goodRank.Index)
                            First = item[2][goodRank.Index][0].Value<string>();
                        else
                            First = item[2].FirstOrDefault()?[0].Value<string>();
                    }


                }
            }

            if (!string.IsNullOrEmpty(First) && First.Length > 0)
                dicAll.Add(First.Trim());

            if (body.Count > 1 && body[1].HasValues)
            {
                foreach (var v in body[1])
                {
                    if (v.HasValues && v.Count() > 1)
                    {
                        var dic = v[1].Select(x => x.Value<string>()).ToList();
                        dicAll.AddRange(dic);
                    }
                }
            }

            dicAll = dicAll.Distinct().ToList();
            var proceessd = dicAll.Aggregate((x, y) => x + ", " + y).Trim(' ', ',');
            return new WordProcessed
            {
                AllWords = proceessd,
                First = First
            };

            //  entity.WordTranslates.Where(x => x.WordID == result1.WordId && x.LanguageId == result1.LanId)
            //    .UpdateFromQuery(x => new WordTranslate { AllWords = proceessd, Translated = result1.First, Proccessed = true });

            //    Console.WriteLine($"Update {result1.WordId}\t{result1.LanId}\t{result1.Word}");


        }
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

    public class WordProcessed
    {
        public WordProcessed()
        {
        }

        public string AllWords { get; internal set; }
        public string First { get; internal set; }
    }

    public class TrasnlateServiceCompare
    {
        public static void StartTask()
        {
            while (true)
            {
                try
                {
                    TranslateResourceData();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
                    Environment.Exit(-1);
                }

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
                            var resultT = TranslatorServiceCompare.TranslateGetAll(
                                "en",
                                data.LanguageCode,
                                data.Word,
                                serverId);
                            resultT.Wait();

                            data.Translated = resultT.Result;
                            var objectT = JsonConvert.DeserializeObject<CallBankService>(data.Translated);
                            if (!string.IsNullOrEmpty(data.Translated) && !string.IsNullOrEmpty(objectT.Text))
                            {
                                service2.SaveResourceTranslatedCompare(data, objectT);
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

                var waitMilisecond = 1400;
                var sleep = (DateTime.Now - dateTime).TotalMilliseconds;
                //Console.WriteLine($"Time To call all {sleep}");
                if (sleep < waitMilisecond)
                {
                    var wait = waitMilisecond - (int)sleep;
                    Task.Delay(wait).Wait();
                    //Console.WriteLine($"Time need wait {wait}");
                    var rondomNumber = new Random().Next(1, 100);
                    Task.Delay(rondomNumber).Wait();
                }

            }
            if (serverId == 0)
            {
                System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(-1);
            }

        }

        private static void TranslateResourceData()
        {
            using (var service = new ResourceService())
            {
                var dataList = service.GetResourceNeedTranslate();
                if (dataList != null)
                {

                    var currenTask = new List<Task>();

                    var threadCount = 1;//TranslatorService.ServerList.Count;
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

        public partial class CallBankServiceCompare
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

}
