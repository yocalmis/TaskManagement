using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace GorevYoneticisi.Tools
{
    public class EmailFunctions
    {
        public static string mailAdresi = null;
        public static string mailSifresi = null;
        static string gmailSsl = null;
        static string gmailPort = null;
        static string gmailHost = null;
        static DateTime sonGuncelleme;
        public static void sistemAyarlariniGetir()
        {
            if (mailAdresi == null || DateTime.Now.AddMinutes(2) >= sonGuncelleme)
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();
                mailAdresi = sa.mail_address;
                mailSifresi = sa.mail_pswd;
                gmailSsl = sa.mail_ssl;
                gmailPort = sa.mail_port;
                gmailHost = sa.mail_host;
                sonGuncelleme = DateTime.Now;
            }
        }

        /*smtp.gmail.com
        SSL gerektirir: Evet
        TLS gerektirir: Evet (varsa)
        Kimlik doğrulama gerektirir: Evet
        SSL Bağlantı Noktası: 465
        TLS/STARTTLS Bağlantı Noktası: 587*/
        public static bool sendEmailGmail(string mesaj, string subject, string emailTo, int mailHedefTur, int hedefId, string gonderenMail, int gonderenId, string mail_psw, string mail_port, string mail_ssl, string mail_host, int mailGroupId)
        {
            try
            {
                string mailAdres = mailAdresi;
                if (gonderenMail != null && !gonderenMail.Equals(string.Empty))
                {
                    mailAdres = gonderenMail;
                }
                else if (mailAdresi == null)
                {
                    sistemAyarlariniGetir();
                }

                string mailSifre = mailSifresi;
                if (mail_psw != null && !mail_psw.Equals(string.Empty))
                {
                    mailSifre = mail_psw;
                }

                string mailHost = gmailHost;
                if (mail_host != null && !mail_host.Equals(string.Empty))
                {
                    mailHost = mail_host;
                }

                string mailPort = gmailPort;
                if (mail_port != null && !mail_port.Equals(string.Empty))
                {
                    mailPort = mail_port;
                }

                string mailSsl = gmailSsl;
                if (mail_ssl != null && !mail_ssl.Equals(string.Empty))
                {
                    mailSsl = mail_ssl;
                }                

                emailTo = emailTo.Replace('ı', 'i');
                emailTo = emailTo.Replace('İ', 'I');
                emailTo = emailTo.Replace('ç', 'c');
                emailTo = emailTo.Replace('Ç', 'C');
                emailTo = emailTo.Replace('ü', 'u');
                emailTo = emailTo.Replace('Ü', 'u');
                emailTo = emailTo.Replace('Ş', 's');
                emailTo = emailTo.Replace('ş', 's');
                emailTo = emailTo.Replace('ğ', 'g');
                emailTo = emailTo.Replace('Ğ', 'g');
                emailTo = emailTo.Replace('ö', 'o');
                emailTo = emailTo.Replace('Ö', 'o');
                try
                {
                    SmtpClient sc = new SmtpClient();

                    sc.Port = Convert.ToInt32(mailPort);
                    sc.Host = mailHost;
                    sc.EnableSsl = Convert.ToBoolean(mailSsl);

                    sc.Credentials = new NetworkCredential(mailAdres, mailSifre);

                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress(mailAdres, config.projeİsmi);

                    mail.To.Add(emailTo);

                    mail.Subject = subject;
                    mail.IsBodyHtml = true;

                    mail.Body = mesaj;
                    sc.Send(mail);

                    mailKaydet(subject, mesaj, mailAdres, emailTo, durumlar.aktif, mailHedefTur, hedefId, gonderenMail, gonderenId, mailGroupId);
                }
                catch (Exception ex)
                {
                    mailKaydet(subject, mesaj, mailAdres, emailTo, durumlar.basarisiz, mailHedefTur, hedefId, gonderenMail, gonderenId, mailGroupId);
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static int getGroupId()
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            int groupId = 1;
            if (db.mailler.Count() != 0)
            {
                groupId = db.mailler.Max(e => e.mail_grup_id) + 1;
            }
            return groupId;
        }
        public static void mailKaydet(string baslik, string mesaj, string kimden, string kime, int flag, int mailHedefTur, int hedefId, string gonderenMail, int gonderenId, int mailGroupId)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            int vid = 1;
            if (db.mailler.Count() != 0)
            {
                vid = db.mailler.Max(e => e.vid) + 1;
            }
            int sort = 1;
            if (db.mailler.Count() != 0)
            {
                sort = db.mailler.Max(e => e.sort) + 1;
            }
            mailler mail = new mailler();
            mail.konu = baslik;
            mail.flag = flag;
            mail.date = DateTime.Now;
            mail.icerik = mesaj;
            mail.gonderen_mail = kimden;
            mail.alan_mail = kime;
            mail.vid = vid;
            mail.gonderen_id = gonderenId;
            mail.hedef_id = hedefId;
            mail.hedef_tur = mailHedefTur;
            mail.mail_grup_id = mailGroupId;
            mail.sort = sort;
            mail.url = "";
            mail.firma_id = lgm.firma_id;

            db.mailler.Add(mail);
            db.SaveChanges();
        }
    }
}