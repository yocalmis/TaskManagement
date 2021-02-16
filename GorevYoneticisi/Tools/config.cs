using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class config
    {
        public static string projeİsmi = "Görev Yönetim Sistemi";
        #if DEBUG
        public static string url = "http://localhost:55586/";
        #else
            public static string url = "http://gorevyonetimi.net/gorev-yonetimi/";
            //public static string url = "http://gorev.varulf.com/";
            // public static string url = "https://musavire-destek.com/gorev/";
#endif

        public static string imgUrl = url + "public/img/";
    }
}