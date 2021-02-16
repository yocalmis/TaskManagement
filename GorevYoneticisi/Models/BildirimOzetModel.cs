using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class BildirimOzetModel
    {
        public int vid { get; set; }
        public string ilgili_url { get; set; }
        public string mesaj { get; set; }
        public string date { get; set; }
        public string okundu { get; set; }
    }
}