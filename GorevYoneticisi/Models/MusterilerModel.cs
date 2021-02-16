using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class MusterilerModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string firma { get; set; }
        public string aciklama { get; set; }
        public string url { get; set; }
        public int ekleyen { get; set; }
        public string firma_adi { get; set; }
        public string vergi_dairesi { get; set; }
        public string vergi_no { get; set; }
        public string adres { get; set; }
        public int firma_id { get; set; }
        public string tel { get; set; }
    }
}