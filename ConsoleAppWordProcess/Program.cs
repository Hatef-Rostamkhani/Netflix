using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    class FlagComparer : IEqualityComparer<string>
    {
        private string languageCode;

        public FlagComparer(string languageCode)
        {
            this.languageCode = languageCode;
        }

        // Products are equal if their names and product numbers are equal.
        public bool Equals(string x, string y)
        {

            //Check whether the compared objects reference the same data.
            //if (ReferenceEquals(x, y)) return true;

            ////Check whether any of the compared objects is null.
            //if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            //    return false;

            //Check whether the products' properties are equal.
            var ret = StringComparer.Create(new CultureInfo(languageCode), true).Compare(x, y);
            //var ret = string.Compare(x, y,
            //    StringComparison.CurrentCultureIgnoreCase);
            return ret == 0;
        }

        public int GetHashCode(string obj)
        {
            var ret = StringComparer.Create(new CultureInfo(languageCode), true).GetHashCode(obj);

            return ret;
        }
    }
    public class Program
    {
        public static CancellationToken token = new CancellationToken();
        public static Task currentTask = null;
        static void Main(string[] args)
        {
            // Task.Factory.StartNew(() => CheckQueue());
            // Task.Factory.StartNew(() => FindImmediatelyNew());
            // Task.Factory.StartNew(() => WordFamilyService.StartCalculate());
            // Task.Factory.StartNew(FindWordFamilyService.StartDownloadAsync);
            // Task.Factory.StartNew(FindWordFamilyService.StartDownloadVoabularyTimer);
            //ImportJokes.ExportCSV();
            //currentTask = Task.Factory.StartNew(TrasnlateService.StartTask, token);
            currentTask = Task.Factory.StartNew(OxfordService.StartTask, token);

            //  currentTask = Task.Factory.StartNew(TranslatorService.ProcessAll, token);
            //TrasnlateServiceCompare.StartTask();

            // MakeWordOpensive();

            //TranslatorService3.StartTask();
            //ConvertToSQL2();

            //WikiDictionaryService.GetData();


            //var list = new List<string>
            //{
            //    "উপক্ষারীয়",
            //    "উপক্ষারীয়"
            //};

            //var ret = string.Compare(list[0], list[1], CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace);

            //var newlist = list.Distinct(new FlagComparer("bn")).ToList();

            //Merge2Files();
            //Fixportuguese();

            // ConvertToSQL();
            //ConvertToJson3();
            //ConvertToJson2();
            //ConvertToJson();
            //CheckJson();
            //ProcessTranslateFiles();
            Console.Read();

        }

        private static void MakeWordOpensive()
        {
            var files = Directory.GetFiles(@"C:\Users\President\Downloads", "*.txt");
            var mainWords = new List<string>();
            foreach(var file in files)
            {
                var lines = File.ReadLines(file).Where(x => !string.IsNullOrEmpty(x)).ToList();
                var ret1 = lines.Select(x =>
                    x.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()).ToList();
                if(ret1.Any(string.IsNullOrEmpty))
                {

                }
                mainWords.AddRange(ret1);

            }

            mainWords = mainWords.Select(x => x.Replace(" ", "")).Distinct().ToList().OrderBy(x => x.ToLower()).ToList();
            var newList = mainWords.Where(x => x.Contains(" ")).ToList();
            var FinalResult = new List<string>();
            //throw new NotImplementedException();
            foreach(var word in mainWords)
            {
                Thread.Sleep(1600);
                var resultT = TranslatorServiceCompare.TranslateGetAll(
                    "en",
                    "fa",
                    word, 0);
                resultT.Wait();
                Console.WriteLine("Translate :" + word);
                var ret = resultT.Result;
                var objectT = JsonConvert.DeserializeObject<CallBankService>(ret);
                FinalResult.Add(word.Trim().ToLower());
                if(objectT.IsVerb)
                {
                    FinalResult.Add(word.Trim().ToLower() + "ing");
                    FinalResult.Add(word.Trim().ToLower() + "er");
                }
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ResultOpensive.txt",
                FinalResult.Aggregate((x, y) => x + "," + y).Trim(','), Encoding.UTF8);


        }

        private static void Merge2Files()
        {
            var data2 = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\pt.json", Encoding.UTF8);
            var dicOrg = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(data2);

            var data = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\newPortghal.txt", Encoding.UTF8);
            var newDic = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<string>>>(data);
            int counter = 0;
            int matched = 0;
            foreach(var d in newDic)
            {
                if(dicOrg.ContainsKey(d.Key.ToLower()))
                {
                    matched++;
                    dicOrg.AddOrUpdate(d.Key.ToLower(), d.Value, (k, old) => {

                        var newAdd = d.Value.Where(x => !old.Any(x.Contains)).ToList();
                        if(newAdd.Count > 0)
                        {
                            counter++;
                            old.AddRange(newAdd);
                        }

                        return old;
                    });
                }

            }
            Console.WriteLine("All new " + newDic.Count);
            Console.WriteLine("matched " + matched);
            Console.WriteLine("Added New " + counter);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\New_Pt.json", JsonConvert.SerializeObject(dicOrg), Encoding.UTF8);
        }

        private static void Fixportuguese()
        {
            var data = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\portuguese.txt", Encoding.UTF8);
            SortedDictionary<string, List<string>> dic = new SortedDictionary<string, List<string>>();
            List<string> newData = new List<string>();
            newData = data.Where(x => x.Length > 0).Select(x => x.Replace("(!)", " ")).ToList();
            foreach(var line in newData)
            {
                var l = line.Trim();
                var splited = l.Split(new[] { '–', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if(splited.Length < 2)
                {
                    //throw new Exception("Less than 2");
                } else
                {
                    List<string> translated = new List<string>();
                    var porteghal = splited[0].Trim();

                    if(porteghal.EndsWith("/a") || porteghal.Contains("/a "))
                    {
                        List<string> gender = new List<string>();
                        if(porteghal.EndsWith("/a"))
                        {
                            gender = porteghal.Split(new[] { "/a" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        } else
                        {
                            gender = porteghal.Split(new[] { "/a " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }

                        if(gender.Count > 0)
                        {
                            string addEnd = "";
                            if(gender.Count > 2)
                            {
                                throw new Exception("Less than 2");
                            } else if(gender.Count == 2)
                            {
                                addEnd = " " + gender.LastOrDefault();
                            }

                            var masculaine = gender.FirstOrDefault();
                            var feminine = masculaine.Remove(masculaine.Length - 1) + "a";
                            translated.Add(masculaine.Trim() + addEnd);
                            translated.Add(feminine.Trim() + addEnd);

                        } else
                        {
                            translated.Add(porteghal);
                        }
                    } else if(porteghal.Contains("("))
                    {
                        continue;
                    } else if(porteghal.Contains("/"))
                    {
                        translated = porteghal.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                            .ToList();
                    } else
                    {
                        translated.Add(porteghal);
                    }
                    var translateed = splited[1].Replace(" f.", "").Replace(" m.", "").Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    foreach(var d in translateed)
                    {
                        if(dic.ContainsKey(d))
                        {
                            var d1 = dic[d];
                            foreach(var t in translated)
                            {
                                if(!d1.Contains(t))
                                    d1.Add(t);
                            }
                        } else
                        {
                            dic.Add(d, translated);
                        }
                        //dic.AddOrUpdate(d, translated, (key, oldValue) =>
                        //  {
                        //      foreach (var t in translated)
                        //      {
                        //          if (!oldValue.Contains(t))
                        //              oldValue.Add(t);
                        //      }

                        //      return oldValue;
                        //  });
                    }
                }
            }
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\newPortghal.txt", JsonConvert.SerializeObject(dic), Encoding.UTF8);
            Console.WriteLine("Writed");

        }

        public static void ConvertToSQL2()
        {
            var path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFileMerged");


            var pathSQL = AppDomain.CurrentDomain.BaseDirectory + "\\" + "InsertTranslated2.sql";

            File.Delete(pathSQL);
            Console.WriteLine("Gettting");

            EnglishWords3Entities entity = new EnglishWords3Entities();

            var data = (from f in entity.AllWordFromPaymons
                        join t in entity.WordTranslates on f.ID equals t.WordID
                        join l in entity.Languages on t.LanguageId equals l.ID
                        orderby l.LanguageCode
                        select new {
                            f.Word,
                            t.AllWords,
                            l.LanguageCode
                        }).ToList();


            Console.WriteLine("Geted");




            List<string> codes = new List<string>();
            string lastLan = "";
            foreach(var f in data)
            {

                var d = $"\r\nINSERT INTO [dbo].[Translations] ([Word] ,[Language] ,[Meaning]) VALUES " +
                    $"('{f.Word.Replace("'", "''")}','{f.LanguageCode}',N'{f.AllWords.Replace("'", "''")}')";


                File.AppendAllText(pathSQL, d);

                if(lastLan != f.LanguageCode)
                    Console.WriteLine("Code " + f.LanguageCode + " finished");

                lastLan = f.LanguageCode;
            }
        }

        public static void ConvertToSQL()
        {
            var path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFileMerged");

            var imported = Directory.GetFiles(path.FullName, "*.*");

            var pathSQL = AppDomain.CurrentDomain.BaseDirectory + "\\" + "InsertTranslated.sql";

            File.Delete(pathSQL);
            File.AppendAllText(pathSQL, ConsoleAppWordProcess.Properties.Resources.WordTranslation);

            List<string> codes = new List<string>();
            foreach(var f in imported)
            {

                FileInfo fi = new FileInfo(f);
                var code = fi.Name.Split('.').FirstOrDefault();



                var data =
                    JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(f, Encoding.UTF8));

                var counter = 0;

                foreach(var d in data)
                    foreach(var tr in d.Value)
                    {
                        if(codes.Count == 1000)
                        {
                            File.AppendAllText(pathSQL, codes.Aggregate((x, y) => x + "," + y));
                            codes.Clear();
                        }

                        if(codes.Count == 0 || codes.Count == 1000)
                        {
                            counter++;
                            File.AppendAllText(pathSQL, $"\r\nprint 'inserting language {code} Step {counter}'" +
                                                        $"\r\n------------- Language Code {code}-------------" +
                                                        $"\r\nGO\r\n INSERT INTO [dbo].[WordTranslation] VALUES ");
                        }

                        codes.Add($"('{d.Key.Replace("'", "''")}','{code}',N'{tr.Replace("'", "''")}')");
                    }
                if(codes.Count > 0)
                    File.AppendAllText(pathSQL, codes.Aggregate((x, y) => x + "," + y));
                codes.Clear();

                Console.WriteLine("Code " + code + " finished");
            }
        }


        private static void ConvertToJson3()
        {
            var entity = new EnglishWordsEntities();

            var path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFileMerged2");

            // var filesNeed = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFile2 - Copy");

            //  var listed = filesNeed.Select(x => x.Split('\\').LastOrDefault().Replace(".json", "").ToLower()).ToList();

            var languages = entity.Languages.ToList(); // entity.GetCompletedLanguages();
            var imported = Directory.GetFiles(path.FullName, "*.*")
                .Select(x => x.Split('\\').LastOrDefault().Split('.').FirstOrDefault()).ToList();
            var needToExport = languages.Where(x => !imported.Contains(x.LanguageCode)).ToList();
            var allwords = entity.AllWordFromPaymons.Where(x => x.IsPrimary == true).OrderBy(x => x.ID).ToList();
            foreach(var lan in needToExport.Where(x => x.LanguageCode == "fa"))
            {
                var allData = entity.WordTranslates.Where(x => x.LanguageId == lan.ID).Select(x => new {
                    x.WordID,
                    x.AllWords
                }).Distinct().ToList();
                var wiki = entity.Wikis.Where(x => x.languageId == lan.ID).ToList();
                var words = new Dictionary<string, string>();
                foreach(var t in allwords)
                {
                    var list = new List<string>();
                    var allwords2 = allData.Where(x => x.WordID == t.ID).Select(x => x.AllWords).FirstOrDefault();
                    if(allwords2 != null)
                    {
                        list.AddRange(allwords2.Trim().Split(new[] { ',', '/', '\\', '\r', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    var wik = wiki.Where(x => x.wordId == t.ID).ToList();
                    if(wik.Count > 0)
                    {
                        foreach(var wiki1 in wik)
                        {
                            list.AddRange(wiki1.Translated.Trim().Split(new[] { ',', '/', '\\', '|', '\r', '\n' },
                                StringSplitOptions.RemoveEmptyEntries));
                        }
                    }

                    if(list.Count > 0)
                        words.Add(t.Word.ToLower(), list
                            .Select(x => x.Trim().ToLower())
                            .Where(x => x.Length > 0)
                            .Distinct(new FlagComparer(lan.LanguageCode))
                            .Aggregate((x, y) => x + " , " + y));//.Replace("\\", "/"));


                }
                // File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", "{" + sb.ToString().Trim(',', ' ') + "}", Encoding.UTF8);
                File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", JsonConvert.SerializeObject(words), Encoding.UTF8);
                Console.WriteLine("Code " + lan.LanguageCode);
            }
        }

        private static void ConvertToJson2()
        {
            var entity = new EnglishWordsEntities();

            var path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFileMerged2");

            // var filesNeed = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFile2 - Copy");

            //  var listed = filesNeed.Select(x => x.Split('\\').LastOrDefault().Replace(".json", "").ToLower()).ToList();

            var languages = entity.Languages.ToList(); // entity.GetCompletedLanguages();
            var imported = Directory.GetFiles(path.FullName, "*.*")
                .Select(x => x.Split('\\').LastOrDefault().Split('.').FirstOrDefault()).ToList();
            var needToExport = languages.Where(x => !imported.Contains(x.LanguageCode)).ToList();
            var allwords = entity.AllWordFromPaymons.Where(x => x.IsPrimary == true).OrderBy(x => x.Word).ToList();
            foreach(var lan in needToExport.Where(x => x.LanguageCode == "fa"))
            {
                var allData = entity.WordTranslates.Where(x => x.LanguageId == lan.ID).Select(x => new {
                    x.WordID,
                    x.AllWords
                }).Distinct().ToList();
                var wiki = entity.Wikis.Where(x => x.languageId == lan.ID).ToList();
                var words = new Dictionary<string, List<string>>();
                foreach(var t in allwords)
                {
                    var list = new List<string>();
                    var allwords2 = allData.Where(x => x.WordID == t.ID).Select(x => x.AllWords).FirstOrDefault();
                    if(allwords2 != null)
                    {
                        list.AddRange(allwords2.Trim().Split(new[] { ',', '/', '\\', '\r', '\n', '|' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    var wik = wiki.Where(x => x.wordId == t.ID).ToList();
                    if(wik.Count > 0)
                    {
                        foreach(var wiki1 in wik)
                        {
                            list.AddRange(wiki1.Translated.Trim().Split(new[] { ',', '/', '\\', '|', '\r', '\n' },
                                StringSplitOptions.RemoveEmptyEntries));
                        }
                    }

                    if(list.Count > 0)
                        words.Add(t.Word.ToLower(), list.Select(x => x.Trim().ToLower()).ToList().Where(x => x.Length > 0).Distinct(new FlagComparer(lan.LanguageCode)).ToList());//.Replace("\\", "/"));


                }
                // File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", "{" + sb.ToString().Trim(',', ' ') + "}", Encoding.UTF8);
                File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", JsonConvert.SerializeObject(words), Encoding.UTF8);
                Console.WriteLine("Code " + lan.LanguageCode);
            }
        }

        private static void ConvertToJson()
        {
            EnglishWordsEntities entity = new EnglishWordsEntities();

            var path = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFile2");

            // var filesNeed = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\JsonFile2 - Copy");

            //  var listed = filesNeed.Select(x => x.Split('\\').LastOrDefault().Replace(".json", "").ToLower()).ToList();

            var languages = entity.Languages.ToList(); // entity.GetCompletedLanguages();
            var imported = Directory.GetFiles(path.FullName, "*.*")
                .Select(x => x.Split('\\').LastOrDefault().Split('.').FirstOrDefault()).ToList();
            var needToExport = languages.Where(x => !imported.Contains(x.LanguageCode)).ToList();
            foreach(var lan in needToExport)
            {
                var sb = new StringBuilder();
                var allData = (from f in entity.AllWordFromPaymons
                               join t in entity.WordTranslates on f.ID equals t.WordID
                               where f.IsPrimary == true && t.LanguageId == lan.ID
                               orderby f.Word

                               select new Result1 {
                                   Word = f.Word,
                                   Translated = t.AllWords
                               }).ToList().GroupBy(car => car.Word)
                    .Select(g => g.First())
                    .ToList();
                var wiki = entity.Wikis.Where(x => x.languageId == lan.ID).ToList();
                Dictionary<string, string> words = new Dictionary<string, string>();
                foreach(var t in allData)
                {
                    // sb.Append($"\"{t.Word}\": \"{t.Translated}\", ");


                    words.Add(t.Word, t.Translated);//.Replace("\\", "/"));


                }
                // File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", "{" + sb.ToString().Trim(',', ' ') + "}", Encoding.UTF8);
                File.WriteAllText(path.FullName + "\\" + lan.LanguageCode + ".json", JsonConvert.SerializeObject(words), Encoding.UTF8);
                Console.WriteLine("Code " + lan.LanguageCode);
            }
        }

        public static string NormalString(string allText)
        {
            var sb = new StringBuilder();
            foreach(var c in allText)
            {
                if(char.IsLetter(c) || c == '\'')
                    sb.Append(c.ToString().ToLower());
                else
                    sb.Append(" ");
            }
            return sb.ToString();
        }
        static void CheckQueue()
        {
            do
            {
                while(Finded.Any())
                {
                    Finded.TryDequeue(out var finish);
                    File.AppendAllText(@"D:\temp\txt\Finded.txt", finish + "\r\n");
                }
                while(Finished.Any())
                {
                    Finished.TryDequeue(out var finish);
                    File.AppendAllText(@"D:\temp\txt\Finished.txt", finish + "\r\n");
                }


                Task.Delay(1000);
            } while(true);
        }
        static System.Collections.Concurrent.ConcurrentQueue<string> Finded = new System.Collections.Concurrent.ConcurrentQueue<string>();
        static System.Collections.Concurrent.ConcurrentQueue<string> Finished = new System.Collections.Concurrent.ConcurrentQueue<string>();


        public static void ProcessWord(string currentWord, int indexArray)
        {
            List<string> newWord = new List<string>();

            int found = 0;
            foreach(var item in wordInLines.Where(x => x.Count > 1 && x.Any(c => c.ToLower() == currentWord.ToLower())))
            {
                var index = item.IndexOf(currentWord);
                if(index + 1 <= item.Count - 1)
                {
                    found++;
                    newWord.Add(currentWord + " " + item[index + 1]);
                }
            }

            var statistics = newWord.GroupBy(word => word).ToDictionary(kvp => kvp.Key,
                kvp => kvp.Count()).Where(x => x.Key.Length > 1).OrderByDescending(x => x.Value).Take(7).ToList();

            var result = new StringBuilder();

            foreach(var key in statistics)
                Finded.Enqueue($"{key.Key}");//: {key.Value}");

            Finished.Enqueue(currentWord);
            Console.WriteLine($"index word {indexArray} word {currentWord} found {found}");// Console.WriteLine($" ");
        }
        static List<List<string>> wordInLines = new List<List<string>>();
        //  public static int indexCollection = -1;
        public static void FindImmediately()
        {
            List<string> processed = new List<string>();
            if(File.Exists(@"D:\temp\txt\Finished.txt"))
            {
                var processedTemp = File.ReadAllLines(@"D:\temp\txt\Finished.txt");
                if(processedTemp.Any())
                    processed.AddRange(processedTemp);
            }



            var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");


            Console.WriteLine("Preparing words");
            List<string> processWords = new List<string>();
            foreach(var c in LineToLine)
            {
                var res = NormalString(c).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if(res.Any())
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
            for(; indexCollection < words.Count;)
            {
                currenTask.Add(Task.Factory.StartNew(() => {
                    var current = Interlocked.Increment(ref indexCollection);
                    if(indexCollection < words.Count)
                        ProcessWord(words[current], current);

                }));
                if(currenTask.Count >= 100)
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
                if(File.Exists(@"D:\temp\txt\Finished.txt"))
                {
                    var processedTemp = File.ReadAllLines(@"D:\temp\txt\Finished.txt");
                    if(processedTemp.Any())
                        processed.AddRange(processedTemp);
                }



                var LineToLine = File.ReadAllLines(@"D:\temp\txt\txt.txt");


                Console.WriteLine("Preparing words");

                foreach(var c in LineToLine)
                {
                    var res = NormalString(c).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if(res.Any() && res.Length > 1)
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


                for(var indexTh = -1; indexTh < threadCount;)
                {
                    currenTask.Add(Task.Factory.StartNew(() => {
                        indexTh++;
                        if(indexTh < threadCount)
                        {
                            Console.WriteLine("Start Thread " + indexTh);
                            //Interlocked.Increment(ref indexCollection);
                            var forLast = (wordInLines.Count - (indexTh * count));
                            var range = wordInLines.GetRange(count * indexTh,
                                indexTh + 1 == threadCount ? forLast : count);
                            for(int i = 0; i < range.Count; i++)
                                ProcessLines(range[i], i + (count * indexTh));
                        }

                    }));
                }
                Task.WaitAll(currenTask.ToArray());
                var result = new StringBuilder();
                var statistics = processWords.GroupBy(word => word.FirstWord).ToList();
                foreach(var s in statistics)
                {
                    Console.WriteLine("word process : " + s.Key + " " + s.Count());
                    var list = s.GroupBy(x => x.Total).ToDictionary(kvp => kvp.Key, kvp => kvp.Count()).Where(x => x.Key.Length > 1)
                        .OrderByDescending(x => x.Value).Take(7).ToList();
                    foreach(var l in list)
                        result.AppendLine($"{l.Key} : {l.Value}");
                }
                File.WriteAllText(@"D:\temp\txt\FinalResult.txt", result.ToString(), Encoding.UTF8);
            } catch(Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }

        private static void ProcessLines(List<string> current, int index)
        {
            //if (index % 100000 == 0 || index % 10000 == 0)
            //     Console.WriteLine("Lines : " + index);
            for(var j = 0; j < current.Count - 1; j++)
            {
                if(ignoreWords.Contains(current[j + 1]))
                    continue;
                lock(processWords)
                {
                    processWords.Add(new WordClass() {
                        FirstWord = current[j],
                        SecondWord = current[j + 1],
                    });
                }
            }
        }
    }
}
