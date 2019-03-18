using Newtonsoft.Json;
using Olive.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    //public class JokeJson
    //{
    //   public string body { get; set; }
    //        category
    //    id
    //        title
    //    score
    //        rating
    //}
    public enum JokeSourceEnum
    {
        RedditJokesJson = 1,
        StupidstuffJson = 2,
        WockaJson = 3,

    }
    public class ImportJokes
    {
        public static void ImportJokeJson()
        {
            List<Joke> list = new List<Joke>();
            list.AddRange(ImportFile("stupidstuff.json", JokeSourceEnum.StupidstuffJson));
            list.AddRange(ImportFile("reddit_jokes.json", JokeSourceEnum.RedditJokesJson));
            list.AddRange(ImportFile("wocka.json", JokeSourceEnum.WockaJson));
            var enity = new EnglishWordsEntities();
            enity.BulkInsert<Joke>(list);
        }

        public static void ExportCSV()
        {
            var entity = new EnglishWordsEntities();
            Console.WriteLine("Fething data....");
            var data = entity.Jokes.ToList();
            Console.WriteLine("Convert To CSV....");
            var csv = data.ToCsv();
            Console.WriteLine("Savging to file....");
            //var csv = JsonConvert.SerializeObject(data.Take(2000).ToList());
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\joke.csv", csv, Encoding.UTF8);
        }

        public static List<Joke> ImportFile(string fileName, JokeSourceEnum source)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var jokeFile = File.ReadAllText(@"D:\temp\joke-dataset-master\" + fileName);
            var jokes = JsonConvert.DeserializeObject<List<Joke>>(jokeFile, jsonSerializerSettings);
            foreach (var joke in jokes)
                joke.Source = (int)source;

            return jokes;
        }
    }
}
