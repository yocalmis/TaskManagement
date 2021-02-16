using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class HaritaOzetModel
    {
        public int id { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string date { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string url { get; set; }
    }
}