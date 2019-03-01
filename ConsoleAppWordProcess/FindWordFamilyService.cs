using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    public class FindWordFamilyService
    {
        private static Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();

        static FindWordFamilyService()
        {
            List.Add("be", new List<string>() { "am", "are", "is", "was", "were", "being", "been" });
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

        public static void StartCalculate()
        {

            var processWords = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\needprocess.txt").ToList();


            //Console.WriteLine("Preparing words");
            //List<string> processWords = new List<string>();
            //foreach (var c in LineToLine)
            //{
            //    var res = NormalString(c).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (res.Any())
            //        processWords.AddRange(res);
            //}


            //var wordList = File.ReadAllText(@"D:\temp\txt\Word forms.csv").Split(new char[] { ',', '"', '\r', '\n', ' ' },
            //    StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();


            //processWords.RemoveAll(x => wordList.Contains(x));


            var distinct = processWords.Distinct().ToList();

            // var groupedWord = processWords.GroupBy(x => x).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());





            ConcurrentDictionary<string, List<string>> familyGroup = new ConcurrentDictionary<string, List<string>>();
            var entity = new EnglishWordsEntities();
            var root = entity.Roots.ToList();

            //File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\needprocess.txt", processWords, Encoding.UTF8);

            foreach (var word in distinct)
            {

            }

            foreach (var key in familyGroup)
            {
                Console.WriteLine($"{key} : {key.Value.Aggregate((x, y) => x + "," + y)}");
            }



            //List<Task> currenTask = new List<Task>();

            //var threadCount = 10;
            //var count = lines.Count / threadCount;


            //for (var indexTh = -1; indexTh < threadCount;)
            //{

            //    indexTh++;
            //    if (indexTh < threadCount)
            //    {
            //        Console.WriteLine("Start Thread " + indexTh);

            //        var forLast = (lines.Count - (indexTh * count));
            //        var range = lines.GetRange(count * indexTh,
            //            indexTh + 1 == threadCount ? forLast : count);
            //        currenTask.Add(Task.Factory.StartNew(() =>
            //        {
            //            foreach (var line in range)
            //            {
            //                var words = line.Split(new[] { ',', ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries);
            //                var countOfWord = groupedWord.Where(x => words.Contains(x.Key)).Select(x => x.Value)
            //                    .Sum();
            //                Console.WriteLine($"{line} : {countOfWord}");
            //                sb.AppendLine($"{line}, {countOfWord}");
            //            }

            //        }));
            //        //ProcessLines(range[i], i + (count * indexTh));
            //    }


            //}

            //Task.WaitAll(currenTask.ToArray());
            // File.WriteAllText(@"D:\temp\txt\WordForms.txt", sb.ToString(), Encoding.UTF8);



        }

    }
}
