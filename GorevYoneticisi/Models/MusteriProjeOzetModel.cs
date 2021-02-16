using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class MusteriProjeOzetModel
    {
        public int id { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string firma_adi { get; set; }
        public string kUrl { get; set; }
        public int kId { get; set; }
        public int musteri_id { get; set; }
    }
}