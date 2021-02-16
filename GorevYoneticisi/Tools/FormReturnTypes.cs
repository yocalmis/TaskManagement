using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class FormReturnTypes
    {
        public static int basarili = 1;
        public static int captchaHatasi = 2;
        public static int basarisiz = 3;
        public static int unique_email = 4;
        public static int unique_username = 5;
        public static int unique = 6;

        public static formReturnClass cevapOlustur(int sonuc, string text)
        {
            formReturnClass frc = new formReturnClass();
            frc.sonuc = sonuc;
            frc.text = text;
            return frc;
        }
    }
}