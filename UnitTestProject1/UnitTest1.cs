using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace UnitTestProject1
{
    public static class TT
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
    [TestClass]
    public class UnitTest1
    {
  

        [TestMethod]
        public void ConvertCSVtoJson()
        {
            var lins = File.ReadAllLines(@"F:\Projects\Geeksltd\Netflix\NetflixSSL\convert.csv");
            List<Video> list = new List<Video>();
            foreach (var l in lins)
            {
                var v = new Video();
                if (l.StartsWith('"'))
                {
                    var last = l.LastIndexOf('"');
                    v.name = l.Substring(0, last - 1).Trim(',', ' ', '"');
                    v.Url = l.Substring(last + 1).Trim(',', ' ', '"');
                }
                else if (l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                {
                    var last = l.LastIndexOf(',');
                    v.name = l.Substring(0, last - 1).Trim(',', ' ', '"');
                    v.Url = l.Substring(last + 1).Trim(',', ' ', '"');
                }
                else
                {
                    var data = l.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (data.Length == 0)
                        continue;
                    v.name = data[0].Trim(',', ' ', '"');
                    v.Url = data[1].Trim(',', ' ', '"');
                }
                list.Add(v);
            }
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Data.json", JsonConvert.SerializeObject(list), Encoding.UTF8);
        }


        [TestMethod]
        public void ParsHtmlAndFindEnglishAudio()
        {
            var files = Directory.GetFiles(@"F:\netflix_html\other", "*.*");
            foreach (var f in files)
            {

                var doc = new HtmlDocument();
                doc.Load(f);
                var mdnode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:url']");
                var videoId = "";
                if (mdnode != null)
                {
                    var desc = mdnode.Attributes["content"];
                    string fulldescription = desc.Value;
                    //Trace.Write("VideoID: " + fulldescription.Split("/").LastOrDefault());
                    videoId = fulldescription.Split("/").LastOrDefault();
                }

                var audio11 = doc.DocumentNode.SelectNodes("//p//strong");
                if (audio11 != null)
                {
                    var audio2 = audio11.Where(x => x.InnerText.ToLower() == "Audio:".ToLower()).FirstOrDefault();
                    if (audio2 != null)
                    {
                        var p = audio2.ParentNode;
                        if (p.InnerText.ToLower().Contains("english"))
                            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\HaveEnglishAudio.txt", "\r\n" + videoId);
                        //Trace.WriteLine(videoId);
                    }
                }
                else
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\NotfoundAudio.txt", "\r\n" + videoId);
                }
            }


            Assert.IsTrue(true);
        }


        [TestMethod]
        public void RemoveNotEnglish()
        {
            var allText = File.ReadAllText(@"F:\Projects\Geeksltd\Netflix\NetflixSSL\convertcsv.json");

            var AllEnglishSercies = File.ReadAllLines(@"F:\Projects\Geeksltd\Netflix\UnitTestProject1\bin\Debug\netcoreapp2.2\HaveEnglishAudio.txt").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();

            var allSercies = JsonConvert.DeserializeObject<List<Video2>>(allText);

            var lastResult = allSercies.Where(x => AllEnglishSercies.Contains(x.RootVideo)).ToList();
            lastResult = lastResult.DistinctBy(x => x.VideoId).DistinctBy(x => x.FullUrl).ToList();

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\FinalResult.txt", JsonConvert.SerializeObject(lastResult), Encoding.UTF8);

        }

        [TestMethod]
        public void SplitWords()
        {
            var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");
            List<string> words = new List<string>();
            foreach (var c in LineToLine)
            {
                var res = NormalString(c).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (res.Any())
                    words.AddRange(res);
            }

            var statistics = words
    .GroupBy(word => word)
    .ToDictionary(kvp => kvp.Key,
        kvp => kvp.Count()).Where(x => x.Key.Length > 1).OrderByDescending(x => x.Value);

            StringBuilder result = new StringBuilder();


            foreach (var key in statistics)
            {
                result.AppendLine($"{key.Key}"); //: {key.Value}");
            }

            File.WriteAllText(@"D:\temp\txt\forProcess.txt", result.ToString(), Encoding.UTF8);

        }

        public string NormalString(string allText)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in allText)
            {
                if (char.IsLetter(c) || c == '\'')
                    sb.Append(c.ToString().ToLower());
                else sb.Append(" ");
            }
            return sb.ToString();
        }



    }

    public class Video
    {
        public string name { get; set; }
        public string Url { get; set; }

    }

    public class Video2
    {
        public string Name { get; set; }
        public int RootVideo { get; set; }
        public string Season { get; set; }
        public int VideoId { get; set; }
        public string FullUrl { get; set; }
    }


}
