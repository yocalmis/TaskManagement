using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class FirmaMusavirTur
    {
        public static int firma = 1;
        public static int musavir = 2;

        public static string getFirmaMusavirText(int tur)
        {
            string text = "";
            if (tur == firma)
            {
                text = "Firma";
            }
            else if (tur == musavir)
            {
                text = "Müşavir";
            }
            return text;
        }
    }
}