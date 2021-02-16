using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class durumlar
    {
        //aktif
        public static int aktif = 1;
        //silinen
        public static int silindi = 2;
        //başarısız
        public static int basarisiz = 3;
        //emailOnayBekliyor
        public static int emailOnayBekliyor = 4;
        public static int bekliyor = 5;

        public static string getDurum(int durum)
        {
            string durumStr = "";
            if (durum == aktif)
            {
                durumStr = "Aktif";
            }
            else if (durum == silindi)
            {
                durumStr = "Silindi";
            }
            else if (durum == basarisiz)
            {
                durumStr = "Başarısız";
            }
            else if (durum == emailOnayBekliyor)
            {
                durumStr = "e-Mail Onayı Bekliyor";
            }
            else if (durum == bekliyor)
            {
                durumStr = "Bekliyor";
            }
            return durumStr;
        }
    }
}