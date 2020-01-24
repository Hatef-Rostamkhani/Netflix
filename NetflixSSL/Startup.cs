using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetflixSSL.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetflixSSL
{
    public class Startup
    {
        public static Dictionary<string, string> dictionary = new Dictionary<string, string>();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Initial2();
        }

        private void Initial2()
        {
            var data = File.ReadAllText(@"C:\Users\President\Downloads\netflix-UK-shows.json", Encoding.UTF8);
            var list = JsonConvert.DeserializeObject<List<VideoListFromPaymon1>>(data);

            var needProcess = list.Select(x => x.Url.Split('/').LastOrDefault()).ToList().Distinct().OrderBy(c => c).ToList();

            var processed = File.ReadAllLines(@"F:\Projects\Geeksltd\Netflix\NetflixSSL\VideosProccessed.txt", Encoding.UTF8).Distinct().OrderBy(c => c).ToList();


            var needProcess2 = needProcess.Where(x => !processed.Contains(x)).ToList();


            //File.WriteAllLines(@"F:\Projects\Geeksltd\Netflix\NetflixSSL\Videos.txt", list.Select(x => x.Url.Split('/').LastOrDefault()).ToList().Distinct().OrderBy(c => c).ToList());
            File.WriteAllLines(@"F:\Projects\Geeksltd\Netflix\NetflixSSL\Videos.txt", needProcess2.OrderBy(c => c).ToList());
        }

        private static void Initial()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Images\\");
            var dic = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\fa.json");
            dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(dic);

            var words = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Images\\", "*.*");
            var downloadedWords = words.Select(x =>
                x.Split('\\').LastOrDefault()
                    ?.Split(new string[] { ".jpg" }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault()).ToList();

            foreach (var word in downloadedWords)
                dictionary.Remove(word.Trim());

            string[] skipedWords = null;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Skiped.txt"))
                skipedWords = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\Skiped.txt");
            if (skipedWords != null)
                foreach (var word in skipedWords)
                    dictionary.Remove(word.Trim());

        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(options =>
                options
                    .SetPreflightMaxAge(TimeSpan.FromDays(1))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowAnyOrigin()
            .AllowCredentials()

            );

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
