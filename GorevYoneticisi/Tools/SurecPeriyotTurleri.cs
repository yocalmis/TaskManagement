using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class SurecPeriyotTurleri
    {
        public static int gun = 1;
        public static int hafta = 2;
        public static int ay = 3;
        public static int ay3 = 4;
        public static int ay6 = 5;
        public static int yil = 6;

        public static string getPeriyotText(int periyotId)
        {
            string sonuc = "";
            if (periyotId == gun)
            {
                sonuc = "Günlük";
            }
            else if (periyotId == hafta)
            {
                sonuc = "Haftalık";
            }
            else if (periyotId == ay)
            {
                sonuc = "Aylık";
            }
            else if (periyotId == ay3)
            {
                sonuc = "3 Aylık";
            }
            else if (periyotId == ay6)
            {
                sonuc = "6 Aylık";
            }
            else if (periyotId == yil)
            {
                sonuc = "Yıllık";
            }
            return sonuc;
        }

        public static List<StringWithValue> getPeriyotList()
        {
            List<StringWithValue> swvList = new List<StringWithValue>();
            swvList.Add(new StringWithValue(getPeriyotText(gun), gun.ToString()));
            swvList.Add(new StringWithValue(getPeriyotText(hafta), hafta.ToString()));
            swvList.Add(new StringWithValue(getPeriyotText(ay), ay.ToString()));
            swvList.Add(new StringWithValue(getPeriyotText(ay3), ay3.ToString()));
            swvList.Add(new StringWithValue(getPeriyotText(ay6), ay6.ToString()));
            swvList.Add(new StringWithValue(getPeriyotText(yil), yil.ToString()));
            return swvList;
        }
    }
}