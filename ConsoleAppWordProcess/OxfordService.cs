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

    public class OxfordService
    {
        private static string _path;
        static OxfordService()
        {
            _path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\OxfordDic").FullName;
        }
        public static void StartTask()
        {
            TranslateResourceData();
        }

        private static void TrasnaltePerServer(List<AllWordFromPaymon> dataList, int serverId)
        {


            foreach (var data in dataList)
            {
                var dateTime = DateTime.Now;
                using (var service2 = new ResourceService())
                {
                    if (!string.IsNullOrEmpty(data.Word))
                    {
                        try
                        {

                            var hc = new HttpClient();
                            var task1 = hc.GetAsync(
                                $"https://www.oxfordlearnersdictionaries.com/definition/english/{data.Word}");
                            task1.Wait();
                            if (task1.Result.IsSuccessStatusCode)
                            {
                                var dataT = task1.Result.Content.ReadAsStringAsync();
                                dataT.Wait();
                                File.WriteAllText(_path + "\\" + data.ID + ".txt", dataT.Result, Encoding.UTF8);
                                service2.SetStatusOxfordDownload(data.ID, (int)task1.Result.StatusCode);

                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"{serverId}\t{data.Word}\t{task1.Result.StatusCode}\t{DateTime.Now:HH:mm:ss}");
                            }
                            else
                            {
                                service2.SetStatusOxfordDownload(data.ID, (int)task1.Result.StatusCode);
                                Console.ForegroundColor = ConsoleColor.Red;
                                // {TranslatorService.ServerList[serverId]}
                                Console.WriteLine($"{serverId}\t{data.Word}\t{task1.Result.StatusCode}\t{DateTime.Now:HH:mm:ss}");
                            }
                        }
                        catch (Exception e)
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

        private static void TranslateResourceData()
        {
            using (var service = new ResourceService())
            {
                var dataList = service.GetNeedToDownloadOxford();
                if (dataList != null)
                {

                    var currenTask = new List<Task>();

                    var threadCount = 20;//TranslatorService.ServerList.Count;
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
}
