using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class ProjeSurecModel
    {
        public int oncelik { get; set; }
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int firma_id { get; set; }
        public int ekleyen { get; set; }
        public int tur { get; set; }
        public System.DateTime baslangic_tarihi { get; set; }
        public System.DateTime bitis_tarihi { get; set; }
        public string isim { get; set; }
        public string aciklama { get; set; }
        public int durum { get; set; }
        public int yuzde { get; set; }
        public int periyot_turu { get; set; }
        public int parent_vid { get; set; }
        public string url { get; set; }
        public int mevcut_donem { get; set; }
    }
}