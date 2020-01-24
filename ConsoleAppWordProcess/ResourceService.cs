using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace ConsoleAppWordProcess
{
    public class ResourceService : IDisposable
    {
        private EnglishWordsEntities entity;
        public ResourceService()
        {
            entity = new EnglishWordsEntities();
        }
        internal List<GetWordForTranslate_Result> GetResourceNeedTranslate()
        {
            entity.Database.CommandTimeout = int.MaxValue;
            var result = entity.GetWordForTranslate().ToList();
            return result;
        }

        internal List<AllWordFromPaymon> GetNeedToDownloadOxford()
        {
            return entity.AllWordFromPaymons.Where(x => x.OxfordLearnersDictionariesState == null).ToList();
        }
        internal List<AllWordFromPaymon> GetAll()
        {
            return entity.AllWordFromPaymons.ToList();
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    entity.Dispose();
                GC.Collect();
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ResourceService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        internal void SaveResourceTranslated(GetWordForTranslate_Result data, CallBankService objectT)
        {

            entity.WordTranslates.Add(new WordTranslate
            {
                WordID = data.WordID,
                LanguageId = data.LangId,
                Translated = objectT.Text,
                AllWords = objectT.All.Aggregate((x, y) => x + ", " + y).Trim(' ', ','),
                AllData = data.Translated,
                CreateDate = DateTime.Now
            });
            entity.SaveChanges();
            try
            {
                if (entity.WordTranslates.Count(x => x.WordID == data.WordID) == entity.Languages.Count())
                {
                    entity.AllWordFromPaymons.Where(x => x.ID == data.WordID)
                        .UpdateFromQuery(x => new AllWordFromPaymon { Translated = true });
                }
            }
            catch
            {
            }
        }
        internal void SaveResourceTranslatedCompare(GetWordForTranslate_Result data, CallBankService objectT)
        {
            var ob = new WordTranslateCompare
            {
                WordID = data.WordID,

                LanguageId = data.LangId,
                Translated = objectT.Text,
                AllWords = objectT.All.Aggregate((x, y) => x + ", " + y).Trim(' ', ','),
                AllData = data.Translated,
                CreateDate = DateTime.Now
            };
            var ret = TranslatorServiceCompare.ProcessTranslateFiles(ob.Translated, ob);
            ob.Translated = ret.First;
            ob.AllWords = ret.AllWords;

            entity.WordTranslateCompares.Add(ob);
            entity.SaveChanges();
            try
            {
                if (entity.WordTranslateCompares.Count(x => x.WordID == data.WordID) == 1) ;//entity.Languages.Count())
                {
                    entity.AllWordFromPaymonCompares.Where(x => x.ID == data.WordID)
                        .UpdateFromQuery(x => new AllWordFromPaymonCompare { Translated = true });
                }
            }
            catch
            {
            }
        }
        internal void SetStatusOxfordDownload(int wordId, int status)
        {
            entity.AllWordFromPaymons.Where(x => x.ID == wordId)
                .UpdateFromQuery(x => new AllWordFromPaymon { OxfordLearnersDictionariesState = status });
        }

        internal void BatchInsertPhonetic(List<Phonetic> data)
        {
            entity.Phonetics.AddRange(data);
            entity.SaveChanges();
        }

        #endregion
    }
}