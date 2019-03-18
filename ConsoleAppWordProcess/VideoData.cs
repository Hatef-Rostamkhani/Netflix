using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
namespace ConsoleAppWordProcess
{


    public class VideoData
    {
        [JsonProperty("@context")]
        public Uri Context { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("contentRating")]
        public string ContentRating { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("image")]
        public Uri Image { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("actors")]
        public List<Ctor> Actors { get; set; }

        [JsonProperty("creator")]
        public List<object> Creator { get; set; }

        [JsonProperty("director")]
        public List<Ctor> Director { get; set; }
    }

    public class Ctor
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
