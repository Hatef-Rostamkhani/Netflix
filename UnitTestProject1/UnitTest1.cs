using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTestProject1
{
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


    }
    public class Video
    {
        public string name { get; set; }
        public string Url { get; set; }

    }

}
