using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class KullanicilarModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string tel { get; set; }
        public int ekleyen { get; set; }
        public int firma_id { get; set; }
        public int kullanici_turu { get; set; }
        public string url { get; set; }
        public string satis_musteri_id { get; set; }
        public string sgk_no { get; set; }
        public string adres { get; set; }
        public string tc_no { get; set; }
        public string vergi_dairesi { get; set; }
        public string vergi_no { get; set; }
        public string reset_guid { get; set; }
        public Nullable<System.DateTime> reset_guidexpiredate { get; set; }
    }
}