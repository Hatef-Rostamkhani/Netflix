using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                urlList.Add($"{videoData.Name.Normal()},{videoData.MasterRootVideo.Normal()},{videoData.Season.Normal()},{url.VideoId},{url.FullUrl}");
            if (urlList.Count > 0)
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



        [Route("saveimage")]
        [HttpGet]
        public ActionResult SaveImage(string word, string dataText, bool skip)
        {
            if (!string.IsNullOrEmpty(word))
            {
                if (skip)
                {
                    System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Skiped.txt",
                        word + "\r\n");
                    return Content(JsonConvert.SerializeObject(NextWord(word)), "application/json");
                }
                else
                {
                    var image = JsonConvert.DeserializeObject<GoogleImage>(HttpUtility.UrlDecode(dataText));
                    Task.Factory.StartNew(() => DownloadImage(word, image.ImageUrl));
                }

            }
            return Content(JsonConvert.SerializeObject(NextWord(word)), "application/json");

        }


        public static void DownloadImage(string word, string url)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(url, AppDomain.CurrentDomain.BaseDirectory + "\\Images\\" + word + ".jpg");
            }
            catch (Exception)
            {
                System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\FailToDownload.txt",
                    word + "," + url + "\r\n");
            }

        }
        private WordImage NextWord(string oldWord)
        {
            if (!string.IsNullOrEmpty(oldWord))
                Startup.dictionary.Remove(oldWord);
            var word = Startup.dictionary.FirstOrDefault();
            return new WordImage()
            {
                word = word.Key,
                translate = word.Value,
            };
        }

        public class WordImage
        {
            public string word { get; set; }
            public string translate { get; set; }
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

    public partial class GoogleImage
    {
        [JsonIgnore]
        [JsonProperty("cl")]
        public long Cl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }


        [JsonProperty("isu")]
        public string Website { get; set; }

        [JsonIgnore]
        [JsonProperty("itg")]
        public long Itg { get; set; }

        [JsonProperty("ity")]
        public string Format { get; set; }

        [JsonProperty("oh")]
        public long Height { get; set; }

        [JsonProperty("ou")]
        public string ImageUrl { get; set; }

        [JsonProperty("ow")]
        public long Width { get; set; }

        [JsonProperty("pt")]
        public string Title { get; set; }

        [JsonIgnore]
        [JsonProperty("rh")]
        public string Rh { get; set; }

        [JsonIgnore]
        [JsonProperty("rid")]
        public string Rid { get; set; }

        [JsonIgnore]
        [JsonProperty("rt")]
        public long Rt { get; set; }

        [JsonProperty("ru")]
        public string PageUrl { get; set; }

        [JsonProperty("s")]
        public string Alt { get; set; }
        [JsonIgnore]
        [JsonProperty("st")]
        public string St { get; set; }
        [JsonIgnore]
        [JsonProperty("th")]
        public long Th { get; set; }
        [JsonIgnore]
        [JsonProperty("tu")]
        public Uri Tu { get; set; }
        [JsonIgnore]
        [JsonProperty("tw")]
        public long Tw { get; set; }
    }

    public partial class VideoListFromPaymon1
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("episodes")]
        public List<string> Episodes { get; set; }
    }
}
