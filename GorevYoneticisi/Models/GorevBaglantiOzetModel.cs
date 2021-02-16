using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class GorevBaglantiOzetModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int gorev_id { get; set; }
        public int bagli_gorev { get; set; }
        public int ekleyen { get; set; }
        public string isim { get; set; }
    }
}