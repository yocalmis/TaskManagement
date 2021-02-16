using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class StringWithValue
    {
        public string text { get; set;}
        public string value { get; set; }

        public StringWithValue()
        { }
        public StringWithValue(string txt, string vl)
        {
            text = txt;
            value = vl;
        }
    }
}