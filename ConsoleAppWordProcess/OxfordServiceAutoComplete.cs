using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using StringSplitOptions = System.StringSplitOptions;

namespace ConsoleAppWordProcess
{

    public class OxfordServiceAutoComplete
    {
        private static string _path;
        static OxfordServiceAutoComplete()
        {
            _path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\OxfordServiceAutoComplete").FullName;
        }
        public static void StartTask()
        {
            //GetDataAutoComplete();
            ExtartFilesDataAutoComplete();
        }


        public static void ExtartFilesData()
        {
            //var fileJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json");

            var titleList = new ResourceService().GetAll();
            var files = Directory.GetFiles(_path);
            var taskList = new List<Task>();
            foreach(var file in files)
            {
                taskList.Add(Task.Factory.StartNew(() => {
                    var fi = new FileInfo(file);
                    var id = int.Parse(fi.Name.Split('.').FirstOrDefault());
                    var video = titleList.FirstOrDefault(x => x.ID == id);



                    ////   if (video != null)
                    //{
                    var phoneticList = new List<Phonetic>();
                    var doc = new HtmlDocument();
                    doc.Load(file, Encoding.UTF8);
                    Phonetic lastProcssed = null;
                    var pronuncitionsSection =
                        doc.DocumentNode.SelectSingleNode("//div[contains(@class,'pron-gs ei-g')]");
                    if(pronuncitionsSection != null)
                    {
                        var nodes = pronuncitionsSection.SelectNodes("span[@class='pron-g']");

                        foreach(var htmlNode in nodes)
                        {
                            var ph = new Phonetic();
                            var prefixNodes = htmlNode.SelectNodes("span[@class='prefix']");
                            if(prefixNodes != null && prefixNodes.Any())
                            {
                                var first = prefixNodes.First();
                                ph.Accent = first.InnerText;
                                if(prefixNodes.Count > 1)
                                {
                                    var mode = prefixNodes[1];
                                    ph.Usage = mode.InnerText;
                                }
                            } else if(lastProcssed != null)
                            {
                                ph.Usage = lastProcssed.Usage;
                                ph.Accent = lastProcssed.Accent;
                            }

                            var phonNode = htmlNode.SelectSingleNode("span[@class='phon']");
                            if(phonNode != null)
                            {
                                var allSpan = phonNode.SelectNodes("span")?.Where(x =>
                                    x.HasClass("bre")
                                    || x.HasClass("wrap")
                                    || x.HasClass("separator")
                                    || x.HasClass("name")).ToList();
                                if(allSpan != null)
                                    foreach(var span in allSpan)
                                        phonNode.RemoveChild(span);

                                ph.Phonetic1 = phonNode.InnerText;
                                ph.WordId = id;
                                lastProcssed = ph;
                                phoneticList.Add(ph);
                                Console.WriteLine(
                                    $"Word: {video.Word}\tAccent: {ph.Accent}\tUsage: {ph.Usage}\tPhonetic1: {ph.Phonetic1}");
                            }
                        }

                        using(var r = new ResourceService())
                            r.BatchInsertPhonetic(phoneticList);
                    }



                    // Console.WriteLine($"{video.VideoId}\t{video.Name}\t{video.ProductionYear}\t{video.AgeRating}");
                    // }
                    // }));

                }));

                if(taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }

            Task.WaitAll(taskList.ToArray());
            taskList.Clear();

            // Console.WriteLine(
            //     $"{titleList.Count(x => !string.IsNullOrEmpty(x.ProductionYear) && !string.IsNullOrEmpty(x.AgeRating))}");

            // File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json", JsonConvert.SerializeObject(titleList), Encoding.UTF8);
        }

        public static void ExtartFilesDataAutoComplete()
        {
            var titleList = new ResourceService().GetAllIsPrimary();
            var files = Directory.GetFiles(_path);
            var taskList = new List<Task>();
            foreach(var file in files)
            {
                taskList.Add(Task.Factory.StartNew(() => {
                    var fi = new FileInfo(file);
                    var id = int.Parse(fi.Name.Split('.').FirstOrDefault());
                    var word = titleList.FirstOrDefault(x => x.ID == id);


                    var content = File.ReadAllText(file);

                    var list = JsonConvert.DeserializeObject<AutoCompleteModel>(content);


                    if(list.results != null && list.results.Count > 0)
                    {
                        var words = new List<OxfordWord>();

                        Console.WriteLine(
                                    $"Word Id: {word.ID}\t Family: {list.results.Select(x => x.searchtext).Aggregate((x, y) => x + "," + y)}");

                        words.AddRange(list.results.Select(x => new OxfordWord {
                            WordId = id,
                            Type = 1,
                            Word = x.searchtext
                        }).ToList());
                        using(var service = new ResourceService())
                            service.BatchInsertOxfordWord(words);
                    }





                }));

                if(taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }

            Task.WaitAll(taskList.ToArray());
            taskList.Clear();

            // Console.WriteLine(
            //     $"{titleList.Count(x => !string.IsNullOrEmpty(x.ProductionYear) && !string.IsNullOrEmpty(x.AgeRating))}");

            // File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json", JsonConvert.SerializeObject(titleList), Encoding.UTF8);
        }

        private static void TrasnaltePerServer(List<AllWordFromPaymon> dataList, int serverId)
        {


            foreach(var data in dataList)
            {
                var dateTime = DateTime.Now;
                using(var service2 = new ResourceService())
                {
                    if(!string.IsNullOrEmpty(data.Word))
                    {
                        try
                        {

                            var hc = new HttpClient();
                            var task1 = hc.GetAsync(
                                $"https://www.oxfordlearnersdictionaries.com/autocomplete/english/?q={data.Word.ToLower().Trim()}%20&contentType=application%2Fjson%3B%20charset%3Dutf-8");
                            task1.Wait();
                            if(task1.Result.IsSuccessStatusCode)
                            {
                                var dataT = task1.Result.Content.ReadAsStringAsync();
                                dataT.Wait();
                                File.WriteAllText(_path + "\\" + data.ID + ".txt", dataT.Result, Encoding.UTF8);
                                service2.SetStatusOxfordDownloadAutoComplete(data.ID, (int) task1.Result.StatusCode);

                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"{serverId}\t{data.Word}\t{task1.Result.StatusCode}\t{DateTime.Now:HH:mm:ss}");
                            } else
                            {
                                service2.SetStatusOxfordDownloadAutoComplete(data.ID, (int) task1.Result.StatusCode);
                                Console.ForegroundColor = ConsoleColor.Red;
                                // {TranslatorService.ServerList[serverId]}
                                Console.WriteLine($"{serverId}\t{data.Word}\t{task1.Result.StatusCode}\t{DateTime.Now:HH:mm:ss}");
                            }
                        } catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                //var waitMilisecond = 1400;
                //var sleep = (DateTime.Now - dateTime).TotalMilliseconds;
                ////Console.WriteLine($"Time To call all {sleep}");
                //if (sleep < waitMilisecond)
                //{
                //    var wait = waitMilisecond - (int)sleep;
                //    Task.Delay(wait).Wait();
                //    //Console.WriteLine($"Time need wait {wait}");
                //}
                //var rondomNumber = new Random().Next(1, 500);
                ////Console.Write("\tRondomNumber: " + rondomNumber + "\t");
                //Task.Delay(rondomNumber);
            }
            //if (serverId == 0)
            //{
            //    System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
            //    Environment.Exit(-1);
            //}

        }

        private static void GetDataAutoComplete()
        {
            using(var service = new ResourceService())
            {
                var dataList = service.GetNeedToDownloadOxfordAutoComplete();
                if(dataList != null)
                {

                    var currenTask = new List<Task>();

                    var threadCount = 20;//TranslatorService.ServerList.Count;
                    var count = dataList.Count / threadCount;


                    for(var serverID = 0; serverID < threadCount; serverID++)
                    {
                        var id = serverID;
                        currenTask.Add(Task.Factory.StartNew(() => {
                            if(id < threadCount)
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
            var lists = words.Select(x => new AllWordFromPaymon {
                Word = x.Trim(),
            }).Where(x => !string.IsNullOrEmpty(x.Word)).ToList();
            Console.WriteLine("Intering..");
            var entity = new EnglishWordsEntities();
            entity.BulkInsert(lists);
            Console.WriteLine("Inserted");
        }
    }

    public class AutoCompleteWord
    {
        public string searchtext { get; set; }
    }

    public class AutoCompleteModel
    {
        public List<AutoCompleteWord> results { get; set; }
    }
}
