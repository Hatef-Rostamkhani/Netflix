using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    public class NetflixService2
    {

        public static void ExtractInfoFromNetflixInfoPages()
        {
            var fileJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json");
            var titleList = JsonConvert.DeserializeObject<List<Title>>(fileJson);
            var files = Directory.GetFiles(pathFilesInfo);
            var taskList = new List<Task>();
            foreach (var file in files)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    var fi = new FileInfo(file);
                    var id = int.Parse(fi.Name.Split('.').FirstOrDefault());
                    var video = titleList.FirstOrDefault(x => x.VideoId == id);



                    if (video != null)
                    {
                        if (!string.IsNullOrEmpty(video.ProductionYear) && !string.IsNullOrEmpty(video.AgeRating))
                            return;
                        var doc = new HtmlDocument();
                        doc.Load(file);

                        var ageRestriction = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'ratingsgfx')]");
                        if (ageRestriction != null)
                        {
                            if (string.IsNullOrEmpty(video.AgeRating) && ageRestriction.Attributes["title"] != null && !string.IsNullOrEmpty(ageRestriction.Attributes["title"].Value))
                                video.AgeRating = ageRestriction.Attributes["title"].Value.ToLower();
                        }
                        else
                        {
                            ageRestriction = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'ratingsblock')]");
                            if (string.IsNullOrEmpty(video.AgeRating) && ageRestriction.Attributes["title"] != null && !string.IsNullOrEmpty(ageRestriction.Attributes["title"].Value))
                                video.AgeRating = ageRestriction.Attributes["title"].Value.ToLower();
                        }

                        var productions = doc.DocumentNode.Descendants("strong");
                        var productionYearNode =
                            productions.FirstOrDefault(x => x.InnerText.ToLower().Trim() == "year:");
                        var nextNode = productionYearNode?.NextSibling;
                        if (nextNode != null && string.IsNullOrEmpty(video.ProductionYear))
                            video.ProductionYear = nextNode.InnerText.Trim();

                        Console.WriteLine($"{video.VideoId}\t{video.Name}\t{video.ProductionYear}\t{video.AgeRating}");
                    }
                }));
                if (taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
            Task.WaitAll(taskList.ToArray());
            taskList.Clear();

            Console.WriteLine(
                $"{titleList.Count(x => !string.IsNullOrEmpty(x.ProductionYear) && !string.IsNullOrEmpty(x.AgeRating))}");

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json", JsonConvert.SerializeObject(titleList), Encoding.UTF8);
        }
        public static void ExtractInfoFromPages()
        {
            var fileJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json");
            var titleList = JsonConvert.DeserializeObject<List<Title>>(fileJson);
            var files = Directory.GetFiles(pathFiles);
            var taskList = new List<Task>();
            foreach (var file in files)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    var fi = new FileInfo(file);
                    var id = int.Parse(fi.Name.Split('.').FirstOrDefault());
                    var video = titleList.FirstOrDefault(x => x.VideoId == id);


                    if (video != null)
                    {
                        var doc = new HtmlDocument();
                        doc.Load(file);

                        var productionNode =
                            doc.DocumentNode.SelectSingleNode("//span[@class='title-info-metadata-item item-year']");
                        if (productionNode != null && string.IsNullOrEmpty(video.ProductionYear) &&
                            !string.IsNullOrEmpty(productionNode.InnerText))
                            video.ProductionYear = productionNode.InnerText.Trim();

                        var maturityRatingNode =
                            doc.DocumentNode.SelectSingleNode(
                                "//span[@class='title-info-metadata-item item-maturity']");
                        if (productionNode != null && string.IsNullOrEmpty(video.AgeRating))
                        {
                            if (!string.IsNullOrEmpty(maturityRatingNode.InnerText.Trim()))
                                video.AgeRating = maturityRatingNode.InnerText.Trim();
                            else
                            {
                                var json = doc.DocumentNode.SelectSingleNode("//script[@type=\"application/ld+json\"]");
                                if (json != null)
                                {
                                    var videoData = JsonConvert.DeserializeObject<VideoData>(json.InnerText);
                                    video.AgeRating = videoData.ContentRating.Trim();
                                }
                            }
                        }

                        Console.WriteLine($"{video.VideoId}\t{video.Name}\t{video.ProductionYear}\t{video.AgeRating}");
                    }
                }));
                if (taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
            Task.WaitAll(taskList.ToArray());
            taskList.Clear();

            Console.WriteLine(
                $"{titleList.Count(x => !string.IsNullOrEmpty(x.ProductionYear) && !string.IsNullOrEmpty(x.AgeRating))}");

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json", JsonConvert.SerializeObject(titleList), Encoding.UTF8);
        }
        static string pathFiles = AppDomain.CurrentDomain.BaseDirectory + "\\NetFlixFiles";
        static string pathFilesInfo = AppDomain.CurrentDomain.BaseDirectory + "\\NetFlixFilesInfo";
        static void DownloadFile(Title title, int index, string path)
        {
            var id = title.Url.Split('/').LastOrDefault();
            try
            {

                var url = $"https://anz.newonnetflix.info/info/{id}";
                var html = new WebClient().DownloadString(url);//title.Url);
                File.WriteAllText(path + "\\" + id + ".txt", html, Encoding.UTF8);
                Console.WriteLine("Downloaded " + title.Url + "\t" + index);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\notdownload.txt", id + "\r\n");
            }
        }
        public static void DownloadPages()
        {
            var fileJson = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Titles.json");
            Directory.CreateDirectory(pathFiles);
            Directory.CreateDirectory(pathFilesInfo);

            var downloadedFiles = Directory.GetFiles(pathFiles).Select(x => x.Split('\\').LastOrDefault()?.Split('.').FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

            var downloadedFileInfo = Directory.GetFiles(pathFilesInfo).Select(x => x.Split('\\').LastOrDefault()?.Split('.').FirstOrDefault()).Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

            //File.AppendAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\DownloadedNetflix.txt", downloaded.Select(x => x.ToString()));

            var downloaded = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\DownloadedNetflix.txt").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();
            List<int> notdownloaded = new List<int>();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\notdownload.txt"))
                notdownloaded = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\notdownload.txt")
                    .Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

            downloaded.AddRange(downloadedFiles);
            downloaded.AddRange(notdownloaded);
            downloaded.AddRange(downloadedFileInfo);
            downloaded = downloaded.Distinct().ToList();


            var titleList = JsonConvert.DeserializeObject<List<Title>>(fileJson);

            titleList = titleList.Where(x => !downloaded.Contains(x.VideoId)).ToList();
            Console.WriteLine("Total URL " + titleList.Count);
            List<Task> taskList = new List<Task>();
            for (var index = 0; index < titleList.Count; index++)
            {
                var title = titleList[index];
                var index1 = index;
                taskList.Add(Task.Factory.StartNew(() => DownloadFile(title, index1, pathFilesInfo)));
                if (taskList.Count > 5)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
        }
    }
}
