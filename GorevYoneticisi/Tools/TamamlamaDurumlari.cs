using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class TamamlamaDurumlari
    {
        public static int bekliyor = 1;
        public static int basladi = 2;
        public static int tamamlandi = 3;
        public static int pasif = 4;
        public static int oncekiGorevBekleniyor = 5;

        public static string getTamamlamaDurum(int durum)
        {
            string durumStr = "";
            if (durum == bekliyor)
            {
                durumStr = "Bekliyor";
            }
            else if (durum == basladi)
            {
                durumStr = "Devam Ediyor";
            }
            else if (durum == tamamlandi)
            {
                durumStr = "Tamamlandı";
            }
            else if (durum == pasif)
            {
                durumStr = "Pasif";
            }
            else if (durum == oncekiGorevBekleniyor)
            {
                durumStr = "Önceki Görev Bekleniyor";
            }
            return durumStr;
        }
        public static List<StringWithValue> getTamamlamaList()
        {
            List<StringWithValue> swtList = new List<StringWithValue>();
            swtList.Add(new StringWithValue("Hepsi", "0"));
            swtList.Add(new StringWithValue("Bekleyen", bekliyor.ToString()));
            swtList.Add(new StringWithValue("Devam Eden", basladi.ToString()));
            swtList.Add(new StringWithValue("Tamamlanan", tamamlandi.ToString()));
            swtList.Add(new StringWithValue("Pasif", pasif.ToString()));
            return swtList;
        }
    }
}