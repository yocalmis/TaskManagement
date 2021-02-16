using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class CreateCevap
    {
        public static CevapModel cevapOlustur(bool sonuc, string mesaj, object ek)
        {
            CevapModel sonucM = new CevapModel();
            sonucM.sonuc = sonuc;
            sonucM.mesaj = mesaj;
            sonucM.ek = ek;
            return sonucM;
        }
    }
}