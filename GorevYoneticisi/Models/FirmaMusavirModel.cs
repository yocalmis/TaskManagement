using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class FirmaMusavirModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public string isim { get; set; }
        public string aciklama { get; set; }
        public int proje_sayisi { get; set; }
        public int surec_sayisi { get; set; }
        public int gorev_sayisi { get; set; }
        public int musteri_sayisi { get; set; }
        public int kullanici_sayisi { get; set; }
        public int ekleyen { get; set; }
        public string url { get; set; }
        public string firma_adi { get; set; }
        public string vergi_dairesi { get; set; }
        public string vergi_no { get; set; }
        public string adres { get; set; }
        public int fm_tur { get; set; }
        public System.DateTime baslangic_tarihi { get; set; }
        public System.DateTime bitis_tarihi { get; set; }
        public string musteri_no { get; set; }
    }
}