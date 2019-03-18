using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    public class WordFamilyService
    {
        private static Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();

        static WordFamilyService()
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

            var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");


            Console.WriteLine("Preparing words");
            List<string> processWords = new List<string>();
            foreach (var c in LineToLine)
            {
                var res = NormalString(c).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (res.Any())
                    processWords.AddRange(res);
            }


            var groupedWord = processWords.GroupBy(x => x).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());


            var lines = File.ReadAllLines(@"D:\temp\txt\Word forms.csv").ToList();
            StringBuilder sb = new StringBuilder();


            






            List<Task> currenTask = new List<Task>();

            var threadCount = 10;
            var count = lines.Count / threadCount;


            for (var indexTh = -1; indexTh < threadCount;)
            {

                indexTh++;
                if (indexTh < threadCount)
                {
                    Console.WriteLine("Start Thread " + indexTh);

                    var forLast = lines.Count - (indexTh * count);
                    var range = lines.GetRange(count * indexTh,
                        indexTh + 1 == threadCount ? forLast : count);
                    currenTask.Add(Task.Factory.StartNew(() =>
                    {
                        foreach (var line in range)
                        {
                            var words = line.Split(new[] { ',', ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries);
                            var countOfWord = groupedWord.Where(x => words.Contains(x.Key)).Select(x => x.Value)
                                .Sum();
                            Console.WriteLine($"{line} : {countOfWord}");
                            sb.AppendLine($"{line}, {countOfWord}");
                        }

                    }));
                    //ProcessLines(range[i], i + (count * indexTh));
                }
            }

            Task.WaitAll(currenTask.ToArray());
            File.WriteAllText(@"D:\temp\txt\WordForms.txt", sb.ToString(), Encoding.UTF8);



        }

    }
}
