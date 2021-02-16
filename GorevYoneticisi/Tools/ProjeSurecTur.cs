using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class ProjeSurecTur
    {
        public static int proje = 1;
        public static int surec = 2;

        public static string getProjeSurecString(int tur)
        {
            string turStr = "";
            if (tur == proje)
            {
                turStr = "Proje";
            }
            else if (tur == surec)
            {
                turStr = "Süreç"; 
            }
            return turStr;
        }
        public static string getProjeSurecRenk(int? tur)
        {
            string turRenk = "";
            if (tur == null || tur == 0)
            {
                return "#d62263";
            }
            else if (tur == proje)
            {
                turRenk = "#f98e02";
            }
            else if (tur == surec)
            {
                turRenk = "#53ad56";
            }

            return turRenk;
        }
        public static string getProjeSurecClass(int? tur)
        {
            string turStr = "";
            if (tur == null || tur == 0)
            {
                return "file-text";
            }
            else if (tur == proje)
            {
                turStr = "briefcase";
            }
            else if (tur == surec)
            {
                turStr = "refresh";
            }

            return turStr;
        }
    }
}