using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class GorevVeProjeOzetModel
    {
        public string gorev_ismi { get; set; }
        public string proje_ismi { get; set; }
        public int yuzde { get; set; }
        public int? tur { get; set; }
        public DateTime baslangic_tarihi { get; set; }
        public DateTime bitis_tarihi { get; set; }
        public int gorev_flag { get; set; }
        public int durum { get; set; }
        public string url { get; set; }     
    }
}