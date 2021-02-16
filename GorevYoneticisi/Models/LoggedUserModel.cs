using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class LoggedUserModel
    {
        public int id { get; set; }
        //public int flag { get; set; }
        //public int vid { get; set; }
        //public int sort { get; set; }
        //public System.DateTime date { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        //public string password { get; set; }
        public string email { get; set; }
        public string tel { get; set; }
        //public int ekleyen { get; set; }
        public int firma_id { get; set; }
        public int kullanici_turu { get; set; }
        public string url { get; set; }
        public string satis_musteri_id { get; set; }
        //public string sgk_no { get; set; }
        //public string adres { get; set; }
        //public string tc_no { get; set; }
        //public string vergi_dairesi { get; set; }
        //public string vergi_no { get; set; }
        public FirmaMusavirModel fm { get; set; }
        public int mail_permission { get; set; }
        public int sms_permission { get; set; }
        public string mail_port { get; set; }
        public string mail_ssl { get; set; }
        public string mail_host { get; set; }
        public string mail_psw { get; set; }
        public System.DateTime bitis_tarihi { get; set; }
        public System.DateTime baslangic_tarihi { get; set; }
    }
}