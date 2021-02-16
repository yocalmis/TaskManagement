using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class KullaniciTurleri
    {
        //Bu sıra değiştirilmemeli
        public static int super_admin = 1;
        public static int firma_admin = 20;
        public static int firma_yetkili = 30;//sonradan ismi firma temsilcisi oldu
        public static int user = 40;

        public static string getKullaniciTurText(int kullanici_tur)
        {
            string tur = "";
            if (kullanici_tur == super_admin)
            {
                tur = "Süper Admin";
            }
            else if (kullanici_tur == firma_admin)
            {
                tur = "Firma Yöneticisi";
            }
            else if (kullanici_tur == firma_yetkili)
            {
                tur = "Firma Temsilcisi";
            }
            else if (kullanici_tur == user)
            {
                tur = "Kullanıcı";
            }
            return tur;
        }
    }
}