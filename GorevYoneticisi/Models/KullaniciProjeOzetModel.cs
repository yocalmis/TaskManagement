using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class KullaniciProjeOzetModel
    {
        public string ad { get; set; }
        public string url { get; set; }
        public string soyad { get; set; }
        public int id { get; set; }
        public int kullanici_id { get; set; }
        public int musteri_id { get; set; }
    }
}