using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class YardimModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public string baslik { get; set; }
        public string icerik { get; set; }
        public string video { get; set; }
        public int ekleyen { get; set; }
        public string url { get; set; }
    }
}