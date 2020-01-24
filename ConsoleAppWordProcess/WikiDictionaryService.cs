using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ConsoleAppWordProcess
{
    public class WikiDictionaryService
    {
        public static ConcurrentQueue<Wiktionary> Queue = new ConcurrentQueue<Wiktionary>();
        public static Task SaveToDataBase()
        {
            var entity = new EnglishWordsEntities();
            entity.Configuration.AutoDetectChangesEnabled = false;
            do
            {
                int counter = 0;
                while (!Queue.IsEmpty)
                {
                    Queue.TryDequeue(out var dic);
                    entity.Set<Wiktionary>().Add(dic);
                    counter++;
                    if (counter >= 10000)
                    {
                        entity.SaveChanges();
                        counter = 0;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Insert 10000\tfrom\t" + Queue.Count);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                entity.SaveChanges();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Insert data\tfrom\t" + Queue.Count);
                Console.ForegroundColor = ConsoleColor.Gray;
                Task.Delay(1000).Wait();
            } while (true);
        }

        private static Task savedata;
        public static async Task GetData()
        {


            try
            {
                var addressFile = @"D:\WikiDictionary_Work\enwiktionary-latest-pages-articles.xml";

                savedata = Task.Factory.StartNew(SaveToDataBase);
                await TestReader(addressFile);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            //var data = File.ReadAllText(addressFile);



            //XDocument doc = XDocument.Parse(data);
            //var pages = doc.Descendants("page");


            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.Async = true;


            //new StreamReader()

            //while (reader.Read())
            //{
            //    if (reader.IsStartElement())
            //    {
            //        if (reader.IsEmptyElement)
            //            Console.WriteLine("<{0}/>", reader.Name);
            //        else
            //        {
            //            Console.Write("<{0}> ", reader.Name);
            //            reader.Read(); // Read the start tag.
            //            if (reader.IsStartElement())  // Handle nested elements.
            //                Console.Write("\r\n<{0}>", reader.Name);
            //            Console.WriteLine(reader.ReadString());  //Read the text content of the element.
            //        }
            //    }
            //}

            //Console.WriteLine();
        }

        public static string NormalFirstAndLast(string str)
        {
            return str.Trim('{', '}', ' ', ',', '(').Trim('{', '}', ' ', ',', '(').Trim('{', '}', ' ', ',', '(');
        }

        static async Task TestReader(string adderss)
        {
            try
            {
                await WorkNow(adderss);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task WorkNow(string adderss)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;

            //var allDic = new List<MyDictionary>();
            using (XmlReader reader = XmlReader.Create(adderss, settings))
            {
                //var dic = new MyDictionary();
                var currentWord = "";
                var textStarted = true;
                while (await reader.ReadAsync())
                {

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.ToLower() == "page")
                            {
                                //  dic = new MyDictionary();

                            }

                            if (reader.Name.Equals("title", StringComparison.OrdinalIgnoreCase))
                            {
                                currentWord = await reader.ReadElementContentAsStringAsync();
                                Console.WriteLine("Start Element {0}", currentWord);
                                //if (currentWord != "go")
                                //    continue;
                            }

                            if (reader.Name.ToLower() == "text")
                            {
                                var datatext = await reader.ReadElementContentAsStringAsync();
                                //if (currentWord != "go")
                                //    continue;
                                var startIndex = 0;
                                var key = "====Translations====";
                                do
                                {

                                    var indexOf = datatext.IndexOf("====Translations====", startIndex, StringComparison.OrdinalIgnoreCase);
                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    Console.WriteLine(e);
                                    //}


                                    if (indexOf == -1)
                                        break;
                                    var startKey = indexOf + key.Length;
                                    var lastIndex = datatext.IndexOf("==", startKey,
                                        StringComparison.OrdinalIgnoreCase);
                                    var count = lastIndex == -1 ? datatext.Length - startKey : lastIndex - startKey;

                                    var textTraslations = datatext.Substring(startKey, count);



                                    Wiktionary trFirst = new Wiktionary();

                                    string ParentLanguage = "";
                                    string PreviosLanguage = "";
                                    bool IsSubLan = false;
                                    string Category = "";

                                    var lines = textTraslations.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    for (var index = 0; index < lines.Length; index++)
                                    {

                                        var line = lines[index].Trim('\r', '\n', ' ');
                                        if (line.StartsWith("{{"))
                                        {
                                            var firstLine = line.Split('\n').FirstOrDefault();
                                            if (firstLine != null)
                                            {
                                                var category = firstLine.Split('|');

                                                //List<string> array = new List<string>() { "" }

                                                if (firstLine.StartsWith("{{trans-bottom}}") || firstLine == "{{trans-top}}"
                                                    || firstLine.StartsWith("{{ttbc-top}}") || firstLine.StartsWith("{{ttbc-bottom}}") ||
                                                                                             firstLine.StartsWith("{{checktrans-bottom}}") ||
                                                                                             firstLine.StartsWith("{{Webster 1913}}"))
                                                {
                                                    Category = null;
                                                    continue;
                                                }
                                                else if (firstLine.StartsWith("{{trans-see"))
                                                {
                                                    trFirst.TransSee = NormalFirstAndLast(firstLine);
                                                }
                                                else if (firstLine.StartsWith("{{checktrans-top}}") || firstLine.StartsWith("{{checktrans-mid}}"))
                                                {
                                                    Category = "checktrans";
                                                    continue;
                                                }
                                                else if (category.Length == 2)
                                                    Category = NormalFirstAndLast(category[1]);
                                                else if (category.Length > 2)
                                                    Category = NormalFirstAndLast(firstLine);
                                                else if (firstLine == "{{trans-mid}}" || firstLine == "{{ttbc-mid}}")
                                                    continue;
                                                else if (firstLine.StartsWith("{{picdic") || firstLine.StartsWith("{{picdiclabel") || firstLine.StartsWith("{{slim-wikipedia"))
                                                    continue;
                                                else
                                                    continue;
                                                //throw new Exception("category[0] != \"trans-top\"\t" + firstLine);
                                            }
                                            continue;
                                        }

                                        if (line.StartsWith("*"))
                                        {
                                            if (line.StartsWith("*:"))
                                            {
                                                IsSubLan = true;
                                                //if (!string.IsNullOrEmpty(tr.ParentLanguageName))
                                                line = line.Remove(0, 2);
                                                if (string.IsNullOrEmpty(ParentLanguage))
                                                    ParentLanguage = PreviosLanguage;
                                            }
                                            else
                                            {
                                                ParentLanguage = null;
                                                IsSubLan = false;
                                            }

                                            var language = line.Split(new[] { ':' },
                                                StringSplitOptions.RemoveEmptyEntries);
                                            if (language.Length > 1)
                                            {
                                                trFirst.Category = Category;
                                                if (IsSubLan)
                                                    trFirst.ParentLanguageName = ParentLanguage;
                                                var lan = language.FirstOrDefault();
                                                lan = lan?.Trim(' ', '*', '=');

                                                trFirst.LanguageName = lan;
                                                PreviosLanguage = lan;

                                                var trans = line.Split(new[] { lan + ":" },
                                                    StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                                                if (trans != null)
                                                {
                                                    var items = trans.Split(new[] { "}}, ", "}}/ ", "}}/", "}}," },
                                                        StringSplitOptions.RemoveEmptyEntries);
                                                    foreach (var t in items)
                                                    {
                                                        var findBest = t.Trim().Split(new[] { "{{" }, StringSplitOptions.RemoveEmptyEntries);
                                                        foreach (var item in findBest)
                                                        {
                                                            var realItem = NormalFirstAndLast(item);
                                                            if (realItem.Split('|').Length <= 2)
                                                                continue;

                                                            var tr = new Wiktionary
                                                            {
                                                                Word = currentWord,
                                                                ParentLanguageName = trFirst.ParentLanguageName,
                                                                LanguageName = trFirst.LanguageName,
                                                                Category = trFirst.Category,
                                                                TransSee = trFirst.TransSee
                                                            };

                                                            var part = "";
                                                            var partindex = 0;
                                                            var counter = 0;
                                                            var tempPart = "";
                                                            var SetTemp = false;
                                                            var ClosedTemp = false;
                                                            foreach (var r in realItem)
                                                            {
                                                                if (partindex < 2 && r == '|')
                                                                {
                                                                    partindex++;
                                                                    switch (partindex)
                                                                    {
                                                                        case 1:
                                                                            tr.Priority = part;
                                                                            break;
                                                                        case 2:
                                                                            tr.Code = part;
                                                                            break;
                                                                    }
                                                                    part = null;
                                                                }
                                                                else
                                                                {
                                                                    if (partindex == 2)
                                                                    {
                                                                        if (r == '[')
                                                                        {
                                                                            SetTemp = true;
                                                                            ClosedTemp = false;
                                                                        }
                                                                        //else if (r == ']')
                                                                        //{
                                                                        //    SetTemp = false;
                                                                        //    ClosedTemp = true;
                                                                        //}
                                                                        if (SetTemp && r == '|')
                                                                            tempPart = "";
                                                                        else if (!SetTemp && r == '|' || r == '}')
                                                                            break;
                                                                    }
                                                                    if (SetTemp)
                                                                    {
                                                                        if (SetTemp && r == ']')
                                                                        {
                                                                            SetTemp = false;
                                                                            ClosedTemp = true;
                                                                            part += tempPart;
                                                                            tempPart = "";
                                                                        }
                                                                        else if (r != '[' && r != ']' && r != '|')
                                                                            tempPart += r;
                                                                    }
                                                                    else if (r != '[' && r != ']' && r != '|') part += r;

                                                                    if (ClosedTemp && r == '|')
                                                                        break;
                                                                }

                                                            }
                                                            tr.Translated = part;

                                                            Queue.Enqueue(tr);
                                                            Console.WriteLine(
                                                                $"{tr.Word}\t{tr.Category}\t{tr.Code}\t{tr.ParentLanguageName}\t{tr.LanguageName}\t{tr.Translated}");
                                                        }
                                                    }
                                                }
                                            }
                                            //else if (language.Length == 1)
                                            //{
                                            //    ParentLanguage = language.FirstOrDefault().Trim(' ', '*');
                                            //}
                                        }
                                    }
                                    startIndex = lastIndex;
                                } while (startIndex != -1);

                            }

                            break;
                        case XmlNodeType.Text:
                            var data = await reader.GetValueAsync();
                            //Console.WriteLine("Text Node: {0}", data);
                            break;
                        case XmlNodeType.EndElement:
                            //if (reader.Name.ToLower() == "page")
                            //{
                            //    //allDic.Add(dic);
                            //}
                            // textStarted = reader.Name.ToLower() != "text";
                            //Console.WriteLine("End Element {0}", reader.Name);
                            break;
                        default:
                            //Console.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
                            break;
                    }
                }
            }
        }
    }



    //public class Translation
    //{
    //    public string Word { get; set; }
    //    public string Priority { get; set; }
    //    public string ParentLanguageName { get; set; }
    //    public string LanguageName { get; set; }
    //    public string Code { get; set; }
    //    public string Translated { get; set; }
    //    public string Category { get; set; }
    //}
}
