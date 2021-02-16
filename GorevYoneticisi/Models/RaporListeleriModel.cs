using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class RaporListeleriModel
    {
        public string isim { get; set; }
        public int yuzde { get; set; }
        public string tur { get; set; }
        public string baslangic_tarihi { get; set; }
        public string bitis_tarihi { get; set; }
        public int flag { get; set; }
        public int durum { get; set; }
        public string url { get; set; }
        public string ad_soyad { get; set; }
    }
}