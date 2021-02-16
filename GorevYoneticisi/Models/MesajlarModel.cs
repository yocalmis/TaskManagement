using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class MesajlarModel
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int gonderen_id { get; set; }
        public int alan_id { get; set; }
        public string mesaj { get; set; }
        public int firma_id { get; set; }
        public string url { get; set; }
        public string parent_url { get; set; }
    }
}