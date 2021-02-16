using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class BugunModel
    {
        public string isim { get; set; }
        public string baslangic_tarihi { get; set; }
        public string bitis_tarihi { get; set; }
        public string url { get; set; }
        public int tur { get; set; }
    }
}