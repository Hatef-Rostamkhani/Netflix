﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleAppWordProcess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class EnglishWords3Entities : DbContext
    {
        public EnglishWords3Entities()
            : base("name=EnglishWords3Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AllWordFromPaymon> AllWordFromPaymons { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<WordTranslate> WordTranslates { get; set; }
        public virtual DbSet<WordTranslation> WordTranslations { get; set; }
    
        [DbFunction("EnglishWords3Entities", "Split")]
        public virtual IQueryable<Split_Result> Split(string @string, string delimiter)
        {
            var stringParameter = @string != null ?
                new ObjectParameter("String", @string) :
                new ObjectParameter("String", typeof(string));
    
            var delimiterParameter = delimiter != null ?
                new ObjectParameter("Delimiter", delimiter) :
                new ObjectParameter("Delimiter", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<Split_Result>("[EnglishWords3Entities].[Split](@String, @Delimiter)", stringParameter, delimiterParameter);
        }
    
        public virtual ObjectResult<GetCompletedLanguages_Result> GetCompletedLanguages()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCompletedLanguages_Result>("GetCompletedLanguages");
        }
    
        public virtual ObjectResult<string> GetJsonFile(Nullable<int> languageId)
        {
            var languageIdParameter = languageId.HasValue ?
                new ObjectParameter("LanguageId", languageId) :
                new ObjectParameter("LanguageId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("GetJsonFile", languageIdParameter);
        }
    
        public virtual ObjectResult<GetWordForTranslate_Result> GetWordForTranslate()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetWordForTranslate_Result>("GetWordForTranslate");
        }
    }
}
