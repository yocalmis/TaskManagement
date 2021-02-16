using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class OurFunctions
    {
        public static string ourSubString(string text, int lenght)
        {
            string sonuc = "";
            if (text != null && text.Length > lenght)
            {
                sonuc = text.Substring(0, lenght);
            }
            else if (text != null)
            {
                sonuc = text;
            }
            return sonuc;
        }

        public static string etiketleriDuzenle(string etiketStr)
        {
            if (etiketStr.Length >= 3)
            {
                etiketStr = etiketStr.Substring(1, etiketStr.Length - 3);
            }
            return etiketStr;
        }

        public static int WeeksInYear(DateTime date)
        {
            /*GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);*/

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
    }
}