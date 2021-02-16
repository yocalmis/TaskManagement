using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class SmslerCountModel
    {
        public int count { get; set; }
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int gonderen_id { get; set; }
        public string hedef_numara { get; set; }
        public string icerik { get; set; }
        public int hedef_id { get; set; }
        public int hedef_tur { get; set; }
        public string url { get; set; }
        public int sms_grup_id { get; set; }
        public int firma_id { get; set; }
    }
}