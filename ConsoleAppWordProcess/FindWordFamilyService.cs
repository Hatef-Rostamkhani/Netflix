using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppWordProcess
{
    public class FindWordFamilyService
    {
        private static Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();

        static FindWordFamilyService()
        {
            //List.Add("be", new List<string>() { "am", "are", "is", "was", "were", "being", "been" });
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

        public static void StartDownloadAsync()
        {

            var VocabularyCom = AppDomain.CurrentDomain.BaseDirectory + "\\VocabularyCom\\";
            var DictionaryCom = AppDomain.CurrentDomain.BaseDirectory + "\\DictionaryCom\\";

            Directory.CreateDirectory(VocabularyCom);
            Directory.CreateDirectory(DictionaryCom);
            do
            {

                var entity = new EnglishWordsEntities();
                List<Task> taskList = new List<Task>();
                var rows = entity.Roots.Where(x => x.DictionaryCom == null && x.Grouped).Take(100).ToList();
                if (rows.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        var task = Task.Factory.StartNew(() =>
                        {
                            try
                            {

                                var resulT = GetDataDictionary(row, DictionaryCom + row.ID + ".txt",
                                    "https://www.dictionary.com/noresult?term=" + row.Word);
                                resulT.Wait();
                                var result = resulT.Result;

                                var entity2 = new EnglishWordsEntities();
                                entity2.Roots.AddOrUpdate(result);
                                entity2.SaveChanges();
                                if (result.DictionaryCom.HasValue && result.DictionaryCom.Value)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine("Dictionary Found ... " + row.Word);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Dictionary Not Found ... " + row.Word);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e);
                            }
                        });
                        taskList.Add(task);
                    }

                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();

                    //row.VocabularyCom = await GetDataVocabulary(row, VocabularyCom + row.ID + ".txt",
                    //  "https://www.vocabulary.com/dictionary/" + row.Word);
                }
                else
                {
                    break;
                }
            } while (true);

        }

        private static Timer timerDownloadVocabulary;

        public static void StartDownloadVoabularyTimer()
        {
            //            timerDownloadVocabulary = new Timer(StartDownloadVoabulary, null, 1000, 1000);
            //Task.Factory.StartNew(() => ExtractWordFamiliy());

            //MakeWordFamiliy();

            //ExtractWordFamiliyFromDictionary();

            //Task.Factory.StartNew(() => StartDownloadVoabulary(null));
            //StartDownloadVoabulary(null);
            //StartDownloadVoabulary(null);


            MakeWordFamiliyFromDataBase();
            //MakeCount();


        }

        public static void MakeCount()
        {
            var AllWords = File.ReadAllLines(@"D:\temp\txt\txt.txt").ToList();
            List<string> processWords = new List<string>();
            foreach (var c in AllWords)
            {
                var res = NormalString(c).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (res.Any())
                    processWords.AddRange(res);
            }

            var groupedWord = processWords.GroupBy(x => x).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());

            var entity = new EnglishWordsEntities();
            var allroot = entity.Roots.ToList();
            List<Task> taskList = new List<Task>();
            foreach (var word in groupedWord)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    var w = allroot.FirstOrDefault(x => x.Word == word.Key);
                    if (w != null)
                    {
                        w.Count = word.Value;
                        var entity2 = new EnglishWordsEntities();
                        entity2.Roots.AddOrUpdate(w);
                        entity2.SaveChanges();
                        Console.WriteLine($"{w.Word}\t{word.Value}");
                    }
                }));
                if (taskList.Count > 20)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }

        }

        public static ConcurrentDictionary<int, List<int>> family = new ConcurrentDictionary<int, List<int>>();

        public static ConcurrentDictionary<int, FamilyResult> FamilyGrouped = new ConcurrentDictionary<int, FamilyResult>();

        public class FamilyResult
        {
            public string SVC { get; set; }
            public int Count { get; set; }
        }

        public static void MakeWordFamiliyFromDataBase()
        {
            var entity = new EnglishWordsEntities();
            var allroot = entity.Roots.ToList();
            var roots = allroot.Where(x => x.Parent == null && x.Count != null && x.Grouped == false).OrderBy(c => c.Word).ToList();


            List<Task> taskList = new List<Task>();
            foreach (var r in roots)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    FindAllDependency(allroot, r.ID, r.ID, 0);
                    Console.WriteLine($"Grouped {r.Word} {family[r.ID].Count}");

                }));
                if (taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }


            foreach (var f in family)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    StringBuilder currentSb = new StringBuilder();
                    var root = allroot.FirstOrDefault(x => x.ID == f.Key);
                    if (root != null)
                    {
                        var count = 0;

                        count += root.Count ?? 0;

                        currentSb.Append(root.Word);
                        currentSb.Append(",");

                        if (f.Value != null && f.Value.Count > 0)
                        {
                            var words = allroot.Where(x => f.Value.Contains(x.ID)).OrderBy(x => x.Word).ToList();
                            count += words.Sum(x => x.Count ?? 0);

                            var familiy = words.Select(x => x.Word)
                                .Aggregate((x, y) => x + "," + y).Trim(',');
                            currentSb.Append($"\"{familiy}\"");
                        }

                        currentSb.Append(",");
                        currentSb.Append(count);
                        Console.WriteLine(currentSb);
                        var model = new FamilyResult()
                        {
                            Count = count,
                            SVC = currentSb.ToString().Replace("\r", "").Replace("\n", "")
                        };
                        FamilyGrouped.TryAdd(f.Key, model);

                        Console.WriteLine($"FamilyGrouped {f.Key} {model.SVC}");

                    }
                }));
                if (taskList.Count > 10)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
            Task.WaitAll(taskList.ToArray());
            Console.WriteLine("Aggrigating...");
            try
            {
                var ordered = FamilyGrouped.Values.OrderByDescending(x => x.Count).ToList().Select(x => x.SVC).Aggregate((x, y) => x + "\r\n" + y);
                Console.WriteLine("Aggrigatined");
                Console.WriteLine("Writing to file...");
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\WordFamily_NotGrouped.txt", ordered, Encoding.UTF8);
                Console.WriteLine("Wroted file");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }



        }

        public static void FindAllDependency(List<Root> roots, int firstRoot, int secondRoot, int level)
        {
            var finded = roots.Where(x => x.Parent == secondRoot && x.Grouped == false).Select(x => x.ID).ToList();
            if (finded.Count == 0 && level == 0)
            {
                family.TryAdd(firstRoot, new List<int>());
            }
            else
            {
                family.AddOrUpdate(firstRoot, new List<int>(finded), (key, old) =>
                {
                    old.AddRange(finded);
                    return old.Distinct().ToList();
                });
                foreach (var f in finded)
                    FindAllDependency(roots, firstRoot, f, level + 1);
            }
        }



        public static void MakeWordFamiliy()
        {
            var entity = new EnglishWordsEntities();
            var allroot = entity.Roots.ToList();

            var allwordFamily = allroot.Where(x => !string.IsNullOrEmpty(x.VocabularyWordFamilyJson)).Select(x =>
                JsonConvert.DeserializeObject<List<WordFamily>>(x.VocabularyWordFamilyJson))
                .ToList();

            List<WordFamily> wordFamilies = new List<WordFamily>();
            foreach (var f in allwordFamily)
                wordFamilies.AddRange(f);

            var distincted = wordFamilies.Select(x => new WordFamilySimple()
            {
                parent = x.parent,
                word = x.word,
            }).Distinct().ToList();
            List<Task> taskList = new List<Task>();
            var NeedToSearch = allroot.Where(x => x.Parent == null).ToList();
            foreach (var t in NeedToSearch)
            {
                taskList.Add(Task.Factory.StartNew(() =>
                {
                    var r = t;
                    var dis = distincted.FirstOrDefault(x => x.word == r.Word);
                    if (dis == null) return;
                    var parent = allroot.FirstOrDefault(x => x.Word == dis.parent);
                    if (parent == null) return;
                    Console.WriteLine($"Found Parent {parent.Word}\t{r.Word}");
                    r.Parent = parent.ID;
                    var entity2 = new EnglishWordsEntities();
                    entity2.Roots.AddOrUpdate(r);
                    entity2.SaveChanges();

                }));
                if (taskList.Count > 20)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }

            }
        }


        public static void ExtractWordFamiliyFromDictionary()
        {

            var files = Directory.GetFiles(@"F:\Projects\Geeksltd\Netflix\ConsoleAppWordProcess\bin\Debug\DictionaryCom", "*.*");
            var entity2 = new EnglishWordsEntities();


            var allword = entity2.Roots.ToList();

            var idsHaveData = allword
                .Where(x => !string.IsNullOrEmpty(x.DictionaryWordFamily) || x.Parent != null)
                .Select(x => x.ID)
                .ToList();


            var mustToRead = files.Select(x => new
            {
                FilePath = x,
                ID = int.Parse(x.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Replace(".txt", ""))
            }).Where(x => !idsHaveData.Contains(x.ID)).ToList();  //.Select(x => x.FilePath)  //.ToList();


            List<Task> listTask = new List<Task>();
            foreach (var file in mustToRead)
            {
                //listTask.Add(Task.Factory.StartNew(() =>
                // {
                FileInfo fi = new FileInfo(file.FilePath);
                var doc = new HtmlDocument();
                doc.Load(file.FilePath);


                var currentWord = allword.FirstOrDefault(x => x.ID == file.ID);
                if (currentWord.Parent.HasValue)
                    continue;


                var h1Element = doc.DocumentNode.Descendants("h1");
                if (h1Element.Count() > 1)
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\h1.txt", "\r\n" + currentWord.ID);
                    Console.WriteLine("File Have more than H1 " + currentWord.Word);
                }
                else
                {
                    var h1 = h1Element.FirstOrDefault().InnerText;
                    if (h1 != null && currentWord != null && h1.ToLower() != currentWord.Word.ToLower())
                    {
                        var lowered = h1.ToLower();
                        var parent = allword.FirstOrDefault(x => x.Word == lowered);
                        if (parent != null)
                        {
                            currentWord.Parent = parent.ID;
                            currentWord.DictionaryWordFamily = lowered;
                            var entity = new EnglishWordsEntities();
                            entity.Roots.AddOrUpdate(currentWord);
                            entity.SaveChanges();

                            Console.WriteLine($"Found Origin  {lowered}\t{currentWord.Word}");
                        }
                    }
                    else
                    {
                        var section = doc.DocumentNode.SelectSingleNode("//h2[@id='wordOrigin']");

                        if (section != null && section.ParentNode != null)
                        {

                            var parentNode = section.ParentNode;
                            var origin = parentNode.Descendants()
                                .Where(x => x.Name == "a" && x.GetAttributeValue("class", "") == "luna-xref").Select(x => x.InnerText).ToList().OrderByDescending(x => x).FirstOrDefault();

                            if (origin != null && origin.ToLower() != currentWord.Word)
                            {
                                var lowered = origin.ToLower();
                                var parent = allword.FirstOrDefault(x => x.Word == lowered);
                                if (parent != null)
                                {
                                    currentWord.Parent = parent.ID;
                                    currentWord.DictionaryWordFamily = lowered;
                                    var entity = new EnglishWordsEntities();
                                    entity.Roots.AddOrUpdate(currentWord);
                                    entity.SaveChanges();
                                    Console.WriteLine($"Found Origin  {lowered}\t{currentWord.Word}");
                                }

                            }
                        }

                    }
                }


                //var titleSection = doc.DocumentNode
                //     .Descendants("section")
                //    .Where(e => e.GetAttributeValue("class", "").Contains("css-0")).ToList();




                //   va//r h1 = titleSection.SelectMany(c => c.ChildNodes).FirstOrDefault(x => x.Name == "h1");



                var wordFamilyRoot = doc.DocumentNode.Descendants("vcom:wordfamily").ToList();
                if (wordFamilyRoot.Any())
                {
                    var data = wordFamilyRoot.FirstOrDefault()?.GetAttributeValue("data", "");
                    var jsonData = System.Web.HttpUtility.HtmlDecode(data);
                    var wordFamily = JsonConvert.DeserializeObject<List<WordFamily>>(jsonData);
                    if (wordFamily.Count > 0)
                    {

                        var id = int.Parse(fi.Name.Replace(".txt", ""));
                        var entity = new EnglishWordsEntities();
                        var word = entity.Roots.FirstOrDefault(x => x.ID == id);
                        word.VocabularyWordFamilyJson = jsonData;
                        entity.Roots.AddOrUpdate(word);
                        entity.SaveChanges();
                        Console.WriteLine("Saved " + word.Word + " count: " + wordFamily.Count);
                    }
                }
                // }));
                //  if (listTask.Count > 10)
                //  {
                //     Task.WaitAll(listTask.ToArray());
                //     listTask.Clear();
                //  }

            }
        }

        public static void ExtractWordFamiliy()
        {

            var files = Directory.GetFiles(@"F:\Projects\Geeksltd\Netflix\ConsoleAppWordProcess\bin\Debug\VocabularyCom", "*.*");
            var entity2 = new EnglishWordsEntities();
            var idsHaveData = entity2
                .Roots
                .Where(x => !string.IsNullOrEmpty(x.VocabularyWordFamilyJson))
                .Select(x => x.ID)
                .ToList();

            var mustToRead = files.Select(x => new
            {
                FilePath = x,
                ID = int.Parse(x.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Replace(".txt", ""))
            }).ToList().Where(x => !idsHaveData.Contains(x.ID)).Select(x => x.FilePath).ToList();

            List<Task> listTask = new List<Task>();
            foreach (var file in mustToRead)
            {
                listTask.Add(Task.Factory.StartNew(() =>
                {
                    FileInfo fi = new FileInfo(file);
                    var doc = new HtmlDocument();
                    doc.Load(file);
                    var wordFamilyRoot = doc.DocumentNode.Descendants("vcom:wordfamily").ToList();
                    if (wordFamilyRoot.Any())
                    {
                        var data = wordFamilyRoot.FirstOrDefault()?.GetAttributeValue("data", "");
                        var jsonData = System.Web.HttpUtility.HtmlDecode(data);
                        var wordFamily = JsonConvert.DeserializeObject<List<WordFamily>>(jsonData);
                        if (wordFamily.Count > 0)
                        {

                            var id = int.Parse(fi.Name.Replace(".txt", ""));
                            var entity = new EnglishWordsEntities();
                            var word = entity.Roots.FirstOrDefault(x => x.ID == id);
                            word.VocabularyWordFamilyJson = jsonData;
                            entity.Roots.AddOrUpdate(word);
                            entity.SaveChanges();
                            Console.WriteLine("Saved " + word.Word + " count: " + wordFamily.Count);
                        }
                    }
                }));
                if (listTask.Count > 10)
                {
                    Task.WaitAll(listTask.ToArray());
                    listTask.Clear();
                }

            }
        }

        public class WordFamilySimple
        {
            public string parent { get; set; }
            public string word { get; set; }
            public override int GetHashCode()
            {
                return $"{parent}/{word}".GetHashCode();
            }
        }


        public class WordFamily
        {
            public string word { get; set; }
            public bool hw { get; set; }
            public string parent { get; set; }
            public decimal freq { get; set; }
            public decimal ffreq { get; set; }
            public int size { get; set; }
            public int type { get; set; }
        }

        public static void StartDownloadVoabulary(object obj)
        {

            var VocabularyCom = AppDomain.CurrentDomain.BaseDirectory + "\\VocabularyCom\\";

            Directory.CreateDirectory(VocabularyCom);

            do
            {

                var entity = new EnglishWordsEntities();
                List<Task> taskList = new List<Task>();
                var rows = entity.Roots.Where(x =>
                        x.VocabularyCom == null //&& x.DictionaryCom != null
                                                //&& x.DictionaryCom.Value
                    )
                    .Take(10).ToList();
                if (rows.Count > 0)
                {
                    foreach (var row in rows)
                    {
                        var task = Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                var resulT = GetDataVocabulary(row, VocabularyCom + row.ID + ".txt",
                                    "https://www.vocabulary.com/dictionary/" + row.Word);
                                resulT.Wait();
                                var result = resulT.Result;
                                var entity2 = new EnglishWordsEntities();
                                result.CreateDate = DateTime.Now;
                                if (result.VocabularyComStatusCode == 403)
                                    result.VocabularyCom = null;
                                entity2.Roots.AddOrUpdate(result);
                                entity2.SaveChanges();

                                if (result.VocabularyComStatusCode == 200)
                                {
                                    if (result.VocabularyCom != null && result.VocabularyCom.Value)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"{DateTime.Now} Vocabulary Found ... " + row.Word);
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine($"{DateTime.Now} Vocabulary Not Found ... " + row.Word);
                                    }
                                }
                                else if (result.VocabularyComStatusCode == 403)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"{DateTime.Now} Vocabulary Forrbiden ... " + row.Word);
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine($"{DateTime.Now} Vocabulary {result.VocabularyComStatusCode ?? 0}... " + row.Word);
                                }
                                //Console.WriteLine("Saved " + result.Word);
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e);
                            }
                        });
                        taskList.Add(task);
                    }
                    Task.WaitAll(taskList.ToArray());
                    //Task.Delay(1000).Wait();
                    taskList.Clear();
                    entity.Dispose();
                    GC.Collect();

                }
                else
                {
                    break;
                }
            } while (true);
        }



        public static async Task<Root> GetDataDictionary(Root row, string path, string url)
        {
            var hc = new HttpClient();
            var dataResult = await hc.GetAsync(url);
            if (dataResult.IsSuccessStatusCode)
            {
                row.DictionaryCom = true;
                var result = await dataResult.Content.ReadAsStringAsync();
                File.WriteAllText(path, result, Encoding.UTF8);
            }
            else
            {
                row.DictionaryCom = false;
                row.DictionaryComStatusCode = (int)dataResult.StatusCode;
            }

            return row;
        }

        public static async Task<Root> GetDataVocabulary(Root row, string path, string url)
        {
            var hc = new HttpClient();
            var dataResult = await hc.GetAsync(url);
            if (dataResult.IsSuccessStatusCode)
            {
                row.VocabularyComStatusCode = (int)dataResult.StatusCode;
                var result = await dataResult.Content.ReadAsStringAsync();
                row.VocabularyCom = !result.Contains("class=\"noresults\"");
                if (row.VocabularyCom.Value)
                    File.WriteAllText(path, result, Encoding.UTF8);
            }
            else
            {
                row.VocabularyCom = false;
                row.VocabularyComStatusCode = (int)dataResult.StatusCode;
            }

            return row;
        }


        public static void StartCalculate()
        {

            //var processWords = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\needprocess.txt").ToList();


            //Console.WriteLine("Preparing words");
            var AllWords = File.ReadAllLines(@"D:\temp\txt\txt.txt").ToList();
            List<string> processWords = new List<string>();
            foreach (var c in AllWords)
            {
                var res = NormalString(c).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (res.Any())
                    processWords.AddRange(res);
            }

            // var groupedWord = processWords.GroupBy(x => x).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());
            //var wordList = File.ReadAllText(@"D:\temp\txt\Word forms.csv").Split(new char[] { ',', '"', '\r', '\n', ' ' },
            //    StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();


            //processWords.RemoveAll(x => wordList.Contains(x));

            var entity = new EnglishWordsEntities();
            entity.Configuration.AutoDetectChangesEnabled = false;
            entity.Configuration.ValidateOnSaveEnabled = false;
            var exists = entity.Roots.Select(x => x.Word).ToList();

            var distinct = processWords.Distinct().ToList().OrderBy(x => x).ToList().Where(x => !exists.Contains(x)).ToList();

            //File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\MustAddToDatabase.txt", distinct,
            //    Encoding.UTF8);
            entity.BulkInsert(distinct.Select(x => new Root()
            {
                Word = x,
                Grouped = true,
            }));
            //entity.Roots.AddRange(distinct.Select(x => new Root()
            //{
            //    Word = x
            //}));
            //entity.SaveChanges();



            Console.WriteLine("Added to database");
            Console.Read();





            ConcurrentDictionary<string, List<string>> familyGroup = new ConcurrentDictionary<string, List<string>>();

            var root = entity.Roots.ToList();

            //File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\needprocess.txt", processWords, Encoding.UTF8);
            var container = distinct.Where(x => root.Select(c => c.Word).Contains(x)).ToList();
            foreach (var word in container)
            {
                var rootWord = root.FirstOrDefault(x => x.Word == word);
                if (rootWord != null)
                {
                    if (rootWord.Parent.HasValue)
                    {
                        familyGroup.AddOrUpdate(rootWord.Root2.Word, new List<string>() { rootWord.Word },
                            (key, old) =>
                            {
                                if (!old.Contains(rootWord.Word))
                                    old.Add(rootWord.Word);
                                return old;
                            });
                    }
                    else
                    {
                        familyGroup.AddOrUpdate(rootWord.Word, new List<string>() { rootWord.Word },
                            (key, old) =>
                            {
                                if (!old.Contains(rootWord.Word))
                                    old.Add(rootWord.Word);
                                return old;
                            });
                    }

                }
            }

            foreach (var item in familyGroup)
            {
                Console.WriteLine($"{item.Key} : {item.Value.Aggregate((x, y) => x + "," + y)}");
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
