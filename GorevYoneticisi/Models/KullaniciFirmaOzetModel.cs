using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class KullaniciFirmaOzetModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string email { get; set; }
        public string tel { get; set; }
        public int kullanici_turu { get; set; }
        public string url { get; set; }
        public string firma_adi { get; set; }
    }
}