using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class JsonSonuc
    {
        bool isSuccess;
        public bool IsSuccess
        {
            get { return isSuccess; }
            set { isSuccess = value; }
        }
        object message;
        public object Message
        {
            get { return message; }
            set { message = value; }
        }

        public static JsonSonuc sonucUret(bool success, object message)
        {
            JsonSonuc sonuc = new JsonSonuc();
            sonuc.message = message;
            sonuc.isSuccess = success;
            return sonuc;
        }
    }
}