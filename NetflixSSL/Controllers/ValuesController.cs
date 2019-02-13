using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace NetflixSSL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public readonly string RootProject;

        public ValuesController()
        {
            var info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            RootProject = info.Parent?.Parent?.Parent?.FullName;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Route("CollectVideo")]
        [HttpGet]
        public ActionResult<bool> CollectVideo(string dataText)
        {
            var videoData = JsonConvert.DeserializeObject<VideoData2>(HttpUtility.UrlDecode(dataText));
            var urlList = new List<string>();
            AppendForCrawler(videoData.Urls);
            var format = "https://www.Netflix.com/watch/";
            System.IO.File.AppendAllText(RootProject + "\\VideoData.csv", $"\r\n{videoData.Name},{videoData.MasterRootVideo},{format + videoData.MasterRootVideo}", Encoding.UTF8);
            return true;
        }

        [Route("ProcessVideo")]
        [HttpGet]
        public ActionResult<bool> ProcessVideo(string dataText)
        {
            var videoData = JsonConvert.DeserializeObject<VideoData>(HttpUtility.UrlDecode(dataText));
            var urlList = new List<string>();
            foreach (var url in videoData.Urls)
                urlList.Add($"{videoData.Name},{videoData.MasterRootVideo},{videoData.Season},{url.VideoId},{url.FullUrl}");
            System.IO.File.AppendAllLines(RootProject + "\\Data.csv", urlList, Encoding.UTF8);
            return true;
        }

        private void AppendForCrawler(List<int> video)
        {
            var allvideo = System.IO.File.ReadAllLines(RootProject + "\\Videos.txt").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();
            var allvideoProcessed = System.IO.File.ReadAllLines(RootProject + "\\VideosProccessed.txt").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();


            var newVideo = video.Where(x => !allvideo.Contains(x) && !allvideoProcessed.Contains(x)).ToList();
            if (newVideo != null && newVideo.Count > 0)
                System.IO.File.AppendAllLines(RootProject + "\\Videos.txt", newVideo.ConvertAll(x => x.ToString()));
        }

        [Route("GetNextVideo")]
        [HttpGet]
        public ActionResult<int> GetNextVideo(int? id, bool notfound)
        {
            var allvideo = System.IO.File.ReadAllLines(RootProject + "\\Videos.txt").Where(x => !string.IsNullOrEmpty(x)).Select(int.Parse).ToList();
            if (id.HasValue)
            {
                allvideo.Remove(id.Value);
                System.IO.File.WriteAllLines(RootProject + "\\Videos.txt", allvideo.ConvertAll(x => x.ToString()));
            }
            if (!notfound && id.HasValue)
                System.IO.File.AppendAllText(RootProject + "\\VideosProccessed.txt", "\r\n" + id.Value.ToString());

            if (notfound && id.HasValue)
                System.IO.File.AppendAllText(RootProject + "\\VideosNotFound.txt", "\r\n" + id.Value.ToString());
            return allvideo.FirstOrDefault();
        }





        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Video
    {
        public string VideoId { get; set; }
        public string FullUrl { get; set; }
    }
    public class VideoData
    {
        public List<Video> Urls { get; set; }
        public string Season { get; set; }
        public string MasterRootVideo { get; set; }
        public string Name { get; set; }
    }

    public class VideoData2
    {
        public List<int> Urls { get; set; }
        public string MasterRootVideo { get; set; }
        public string Name { get; set; }
    }
}
