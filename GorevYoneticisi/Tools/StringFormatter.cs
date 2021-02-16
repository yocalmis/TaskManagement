using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class StringFormatter
    {
        public static string OnlyEnglishChar(string gelen)
        {
            string sonuc = gelen;
            Encoding iso = Encoding.GetEncoding("Cyrillic");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(sonuc);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            sonuc = msg;

            //string replaced = "";

            //string regex = "?[a-zA-Z]w*(.w+)+(/w*(.w+)*)*(?.+)*";
            //Regex regEx = new Regex(regex);

            // make it all lower case
            sonuc = sonuc.ToLower();
            // remove entities
            sonuc = Regex.Replace(sonuc, @"&\w+;", "");
            // remove anything that is not letters, numbers, dash, or space
            sonuc = Regex.Replace(sonuc, @"[^a-z0-9\-\s]", "");
            // replace spaces
            sonuc = sonuc.Replace(' ', '-');
            // collapse dashes
            sonuc = Regex.Replace(sonuc, @"-{2,}", "-");
            // trim excessive dashes at the beginning
            sonuc = sonuc.TrimStart(new[] { '-' });
            // if it's too long, clip it
            if (sonuc.Length > 80)
                sonuc = sonuc.Substring(0, 79);
            // remove trailing dashes
            sonuc = sonuc.TrimEnd(new[] { '-' });

            //sonuc = replaced;

            return sonuc;
        }
    }
}