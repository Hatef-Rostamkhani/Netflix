using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            //Task.Factory.StartNew(() => CheckQueue());
            //Task.Factory.StartNew(() => FindImmediatelyNew());
            //Task.Factory.StartNew(() => WordFamilyService.StartCalculate());
            Task.Factory.StartNew(FindWordFamilyService.StartDownloadAsync);
            Task.Factory.StartNew(FindWordFamilyService.StartDownloadVoabularyTimer);

            Console.Read();
        }

        public static string NormalString(string allText)
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
        static void CheckQueue()
        {
            do
            {
                while (Finded.Any())
                {
                    Finded.TryDequeue(out var finish);
                    File.AppendAllText(@"D:\temp\txt\Finded.txt", finish + "\r\n");
                }
                while (Finished.Any())
                {
                    Finished.TryDequeue(out var finish);
                    File.AppendAllText(@"D:\temp\txt\Finished.txt", finish + "\r\n");
                }


                Task.Delay(1000);
            } while (true);
        }
        static System.Collections.Concurrent.ConcurrentQueue<string> Finded = new System.Collections.Concurrent.ConcurrentQueue<string>();
        static System.Collections.Concurrent.ConcurrentQueue<string> Finished = new System.Collections.Concurrent.ConcurrentQueue<string>();


        public static void ProcessWord(string currentWord, int indexArray)
        {
            List<string> newWord = new List<string>();

            int found = 0;
            foreach (var item in wordInLines.Where(x => x.Count > 1 && x.Any(c => c.ToLower() == currentWord.ToLower())))
            {
                var index = item.IndexOf(currentWord);
                if (index + 1 <= item.Count - 1)
                {
                    found++;
                    newWord.Add(currentWord + " " + item[index + 1]);
                }
            }

            var statistics = newWord.GroupBy(word => word).ToDictionary(kvp => kvp.Key,
                kvp => kvp.Count()).Where(x => x.Key.Length > 1).OrderByDescending(x => x.Value).Take(7).ToList();

            var result = new StringBuilder();

            foreach (var key in statistics)
                Finded.Enqueue($"{key.Key}");//: {key.Value}");

            Finished.Enqueue(currentWord);
            Console.WriteLine($"index word {indexArray} word {currentWord} found {found}");// Console.WriteLine($" ");
        }
        static List<List<string>> wordInLines = new List<List<string>>();
        //  public static int indexCollection = -1;
        public static void FindImmediately()
        {
            List<string> processed = new List<string>();
            if (File.Exists(@"D:\temp\txt\Finished.txt"))
            {
                var processedTemp = File.ReadAllLines(@"D:\temp\txt\Finished.txt");
                if (processedTemp.Any())
                    processed.AddRange(processedTemp);
            }



            var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");


            Console.WriteLine("Preparing words");
            List<string> processWords = new List<string>();
            foreach (var c in LineToLine)
            {
                var res = NormalString(c).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (res.Any())
                {
                    processWords.AddRange(res);
                    wordInLines.Add(res.ToList());
                }
            }
            var words = processWords.Distinct().Where(x => x.Length > 1).ToList();
            Console.WriteLine("Preparing words finished.");

            words.RemoveAll(x => processed.Contains(x));

            Console.WriteLine("All word : " + words.Count);
            List<Task> currenTask = new List<Task>();
            var indexCollection = 0;
            for (; indexCollection < words.Count;)
            {
                currenTask.Add(Task.Factory.StartNew(() =>
                {
                    var current = Interlocked.Increment(ref indexCollection);
                    if (indexCollection < words.Count)
                        ProcessWord(words[current], current);

                }));
                if (currenTask.Count >= 100)
                {
                    Task.WaitAll(currenTask.ToArray());
                    currenTask.Clear();
                }
            }






            //File.WriteAllText(@"D:\temp\txt\result3.txt", result.ToString(), Encoding.UTF8);

        }

        public class WordClass
        {
            public string FirstWord { get; set; }
            public string SecondWord { get; set; }
            public string Total => (FirstWord ?? "") + " " + (SecondWord ?? "");
        }

        static List<WordClass> processWords = new List<WordClass>();
        private static List<string> ignoreWords = new List<string>() { "is", "the", "a", "an", "and", "no", "that", "this" };
        public static void FindImmediatelyNew()
        {
            try
            {
                List<string> processed = new List<string>();
                if (File.Exists(@"D:\temp\txt\Finished.txt"))
                {
                    var processedTemp = File.ReadAllLines(@"D:\temp\txt\Finished.txt");
                    if (processedTemp.Any())
                        processed.AddRange(processedTemp);
                }



                var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");


                Console.WriteLine("Preparing words");

                foreach (var c in LineToLine)
                {
                    var res = NormalString(c).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (res.Any() && res.Length > 1)
                    {
                        //processWords.AddRange(res);
                        wordInLines.Add(res.ToList());
                    }
                }
                //var words = processWords.Distinct().Where(x => x.Length > 1).ToList();
                Console.WriteLine("Preparing words finished.");

                //words.RemoveAll(x => processed.Contains(x));




                Console.WriteLine("All Lines : " + wordInLines.Count);
                List<Task> currenTask = new List<Task>();

                var threadCount = 10;
                var count = wordInLines.Count / threadCount;


                for (var indexTh = -1; indexTh < threadCount;)
                {
                    currenTask.Add(Task.Factory.StartNew(() =>
                    {
                        indexTh++;
                        if (indexTh < threadCount)
                        {
                            Console.WriteLine("Start Thread " + indexTh);
                            //Interlocked.Increment(ref indexCollection);
                            var forLast = (wordInLines.Count - (indexTh * count));
                            var range = wordInLines.GetRange(count * indexTh,
                                indexTh + 1 == threadCount ? forLast : count);
                            for (int i = 0; i < range.Count; i++)
                                ProcessLines(range[i], i + (count * indexTh));
                        }

                    }));
                }
                Task.WaitAll(currenTask.ToArray());
                var result = new StringBuilder();
                var statistics = processWords.GroupBy(word => word.FirstWord).ToList();
                foreach (var s in statistics)
                {
                    Console.WriteLine("word process : " + s.Key + " " + s.Count());
                    var list = s.GroupBy(x => x.Total).ToDictionary(kvp => kvp.Key, kvp => kvp.Count()).Where(x => x.Key.Length > 1)
                        .OrderByDescending(x => x.Value).Take(7).ToList();
                    foreach (var l in list)
                        result.AppendLine($"{l.Key} : {l.Value}");
                }
                File.WriteAllText(@"D:\temp\txt\FinalResult.txt", result.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }

        private static void ProcessLines(List<string> current, int index)
        {
            //if (index % 100000 == 0 || index % 10000 == 0)
            //     Console.WriteLine("Lines : " + index);
            for (var j = 0; j < current.Count - 1; j++)
            {
                if (ignoreWords.Contains(current[j + 1]))
                    continue;
                lock (processWords)
                {
                    processWords.Add(new WordClass()
                    {
                        FirstWord = current[j],
                        SecondWord = current[j + 1],
                    });
                }
            }
        }
    }
}
