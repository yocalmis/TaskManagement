using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class GorevVeProjeSurecOzetModel
    {
        public int id { get; set; }
        public int oncelik { get; set; }
        public string isim { get; set; }
        public string baslangic_tarihi { get; set; }
        public string bitis_tarihi { get; set; }
        public int yuzde { get; set; }
        public int flag { get; set; }
        public int tur { get; set; }
        public string url { get; set; }
        public int durum { get; set; }
    }
}