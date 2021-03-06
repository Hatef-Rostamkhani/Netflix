﻿using HtmlAgilityPack;
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

    public class OxfordService
    {
        private static string _path;
        static OxfordService()
        {
            _path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\OxfordDic_NewVersion").FullName;
        }
        public static void StartTask()
        {
            //TranslateResourceData();
            //ExtartFilesData();
            ExtartFilesDataIdioms();
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

        public static void ExtartFilesDataIdioms()
        {
            //var fileJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json");

            var titleList = new ResourceService().GetAllIsPrimary();
            var files = Directory.GetFiles(_path);
            var taskList = new List<Task>();
            foreach(var file in files)
            {
                taskList.Add(Task.Factory.StartNew(() => {
                    var fi = new FileInfo(file);
                    var id = int.Parse(fi.Name.Split('.').FirstOrDefault());
                    var currentWord = titleList.FirstOrDefault(x => x.ID == id);



                    if(currentWord != null)
                    {
                        var oxfordWords = new List<OxfordWord>();
                        var doc = new HtmlDocument();
                        doc.Load(file, Encoding.UTF8);
                        oxfordWords.AddRange(GetWords("Phrasal verbs", id, 2, doc));
                        oxfordWords.AddRange(GetWords("Idioms", id, 3, doc));
                        var other = GetWords("All matches", id, 4, doc);
                        var newOther = other.Where(x => !oxfordWords.Select(c => c.Word).Contains(x.Word)).ToList();
                        oxfordWords.AddRange(newOther);

                        if(oxfordWords.Count > 0)
                        {
                            Console.WriteLine($"{currentWord.Word}\t{oxfordWords.Count}");//\t{ oxfordWords.Select(x => x.Word).Aggregate((x, y) => x + "," + y)}
                                                                                          //");


                            using(var service = new ResourceService())
                                service.BatchInsertOxfordWord(oxfordWords.Where(x => x.Word != currentWord.Word).ToList());
                        }



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

        private static List<OxfordWord> GetWords(string text, int id, int type, HtmlDocument doc)
        {
            var list = new List<OxfordWord>();

            var idiomsSection =
                doc.DocumentNode.SelectSingleNode($"//dt[contains(text(),'{text}')]");
            if(idiomsSection != null)
            {
                var nodes = idiomsSection.NextSibling.NextSibling.SelectNodes("ul/li/a/span");

                foreach(var htmlNode in nodes)
                {
                    // var prefixNodes = htmlNode.SelectNodes("span[@class='prefix']");
                    var needtoRemove = htmlNode.ChildNodes.Where(x => x.Name.ToLower() == "pos-g").ToList();
                    foreach(var e in needtoRemove)
                        htmlNode.ChildNodes.Remove(e);

                    var ph = new OxfordWord {
                        Type = type,
                        WordId = id,
                        Word = htmlNode.InnerText.Trim()
                    };


                    //var phonNode = htmlNode.SelectSingleNode("span[@class='phon']");
                    //if(phonNode != null)
                    //{
                    //    var allSpan = phonNode.SelectNodes("span")?.Where(x =>
                    //        x.HasClass("bre")
                    //        || x.HasClass("wrap")
                    //        || x.HasClass("separator")
                    //        || x.HasClass("name")).ToList();
                    //    if(allSpan != null)
                    //        foreach(var span in allSpan)
                    //            phonNode.RemoveChild(span);

                    //    ph.Phonetic1 = phonNode.InnerText;
                    //    ph.WordId = id;
                    //    lastProcssed = ph;
                    list.Add(ph);
                    //    Console.WriteLine(
                    //      $"Word: {video.Word}\tAccent: {ph.Accent}\tUsage: {ph.Usage}\tPhonetic1: {ph.Phonetic1}");
                    //  }
                }
                //using(var r = new ResourceService())
                //    r.BatchInsertPhonetic(phoneticList);
            }
            return list;
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
                            var task1 = hc.GetAsync($"https://www.oxfordlearnersdictionaries.com/definition/english/{data.Word.ToLower().Trim()}");
                            task1.Wait();
                            if(task1.Result.IsSuccessStatusCode)
                            {
                                var dataT = task1.Result.Content.ReadAsStringAsync();
                                dataT.Wait();
                                File.WriteAllText(_path + "\\" + data.ID + ".txt", dataT.Result, Encoding.UTF8);
                                service2.SetStatusOxfordDownload(data.ID, (int) task1.Result.StatusCode);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"{serverId}\t{data.Word}\t{task1.Result.StatusCode}\t{DateTime.Now:HH:mm:ss}");
                            } else
                            {
                                service2.SetStatusOxfordDownload(data.ID, (int) task1.Result.StatusCode);
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

        private static void TranslateResourceData()
        {
            using(var service = new ResourceService())
            {
                var dataList = service.GetNeedToDownloadOxford();
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


}
