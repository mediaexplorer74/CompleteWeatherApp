using System;
using System.Collections.Generic;
using System.Text;

namespace CompleteWeatherApp.Models
{
    public class BackgroundInfo
    {
        public int total_results { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
        public Photo[] photos { get; set; }
        public string nex_page { get; set; }
    }

    public class Photo
    {
        public int id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string url { get; set; }
        public string photographer { get; set; }
        public string photographer_url { get; set; }
        public Src src { get; set; }
        public bool liked { get; set; }
    }

    public class Src
    {
        public string original { get; set; }
        public string large2x { get; set; }
        public string large { get; set; }
        public string medium { get; set; }
        public string small { get; set; }
        public string portrait { get; set; }
        public string landscape { get; set; }
        public string tiny { get; set; }
    }
}
