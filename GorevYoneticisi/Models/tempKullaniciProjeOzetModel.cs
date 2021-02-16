using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class tempKullaniciProjeOzetModel
    {
        public string ad { get; set; }
        public string soyad { get; set; }
        public string kullaniciUrl { get; set; }
        public int kullaniciId { get; set; }
        public List<int> musteriIdList = new List<int>();
        public List<int> idList = new List<int>();
    }
}