using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class bildirimIslemleri
    {
        public static JsonSonuc yeniBildirim(int kullanici_id, int bildirim_turu, int ilgili_id, string ilgili_url, string mesaj)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.sistem_bildirimleri.Count() != 0)
                {
                    vid = db.sistem_bildirimleri.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.sistem_bildirimleri.Count() != 0)
                {
                    sort = db.sistem_bildirimleri.Max(e => e.sort) + 1;
                }

                sistem_bildirimleri bldrm = new sistem_bildirimleri();

                bldrm.kullanici_id = kullanici_id;
                bldrm.flag = durumlar.aktif;
                bldrm.date = DateTime.Now;
                bldrm.vid = vid;
                bldrm.ekleyen = GetCurrentUser.GetUser().id;
                bldrm.sort = sort;
                bldrm.okundu = bildirimOkunmaDurum.beklemede;
                bldrm.bildirim_turu = bildirim_turu;
                bldrm.ilgili_id = ilgili_id;
                bldrm.ilgili_url = "";
                bldrm.mesaj = mesaj;
                if (bildirim_turu == BildirimTurleri.gorev)
	            {
                    bldrm.ilgili_url = "Gorev/" + ilgili_url;
	            }
                else if (bildirim_turu == BildirimTurleri.proje)
	            {
                    bldrm.ilgili_url = "Proje/" + ilgili_url;
	            }
                else if (bildirim_turu == BildirimTurleri.surec)
	            {
                    bldrm.ilgili_url = "Surec/" + ilgili_url;
	            }
                else if (bildirim_turu == BildirimTurleri.musteri)
	            {
		             bldrm.ilgili_url = "Musteri/" + ilgili_url;
	            }
                else if (bildirim_turu == BildirimTurleri.mesaj)
                {
                    bldrm.ilgili_url = "Mesaj/" + ilgili_url;
                }
                else
                {
                    bldrm.ilgili_url = ilgili_url;
                }

                db.sistem_bildirimleri.Add(bldrm);
                db.SaveChanges();

                kullanicilar dbUsr = db.kullanicilar.Where(e => e.id == bldrm.kullanici_id).FirstOrDefault();
                if (dbUsr != null)
                {
                    if (dbUsr.mail_permission == Permissions.granted)
                    {
                        string emailMesaj = bldrm.mesaj + " </br>İlgili bağlantı için <a href='" + Tools.config.url + bldrm.ilgili_url + "'>tıklayınız.</a>";
                        EmailFunctions.sendEmailGmail(emailMesaj, config.projeİsmi + " - Bildirim", dbUsr.email, MailHedefTur.kullanici, bldrm.kullanici_id, "", 0, "", "", "", "", 0);
                    }
                    if (dbUsr.sms_permission == Permissions.granted)
                    {
                        List<string> numaraList = new List<string>();
                        numaraList.Add(dbUsr.tel);
                        SendSms sms = new SendSms();
                        sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();

                        LoggedUserModel lgm = GetCurrentUser.GetUser();
                        string musteri_no = "";
                        if (lgm.fm != null)
                        {
                            musteri_no = lgm.fm.musteri_no;
                        }
                        bool sonuc = sms.SendSMS(numaraList.ToArray(), bldrm.mesaj, sa.sms_header, musteri_no);
                        if (sonuc == false)
                        {
                            return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                        }
                    }
                }                

                return JsonSonuc.sonucUret(true, bldrm.ilgili_url);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public static JsonSonuc bildirimDuzenle(int vid, int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                sistem_bildirimleri dbPrj = db.sistem_bildirimleri.Where(e => e.vid.Equals(vid) && e.flag != durumlar.silindi).FirstOrDefault();

                db.Entry(dbPrj).State = EntityState.Modified;
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, dbPrj.ilgili_url);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public static JsonSonuc bildirimleriGetir(string gorev_url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                //gorevler gorev = db.gorevler.Where(e => e.flag == durumlar.aktif && e.url.Equals(gorev_url)).FirstOrDefault();

                string queryGorevCount = "select y.* from yapilacaklar as y "
                     + "inner join gorevler as g on g.id = y.gorev_id and g.flag = " + durumlar.aktif + " and g.url = '" + gorev_url + "' "
                     + "where y.flag = " + durumlar.aktif;
                List<GorevlerModel> gorevList = db.Database.SqlQuery<GorevlerModel>(queryGorevCount).ToList();

                JsonSonuc sonuc = JsonSonuc.sonucUret(true, gorevList);
                return sonuc;
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "Bildirimler getirilirken bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public static JsonSonuc bildirimOkundu(int vid)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                List<sistem_bildirimleri> bldrmList = db.sistem_bildirimleri.Where(e => e.vid.Equals(vid)).ToList();
                foreach (sistem_bildirimleri bldrm in bldrmList)
                {
                    bldrm.okundu = bildirimOkunmaDurum.okundu;
                    bldrm.okunma_tarihi = DateTime.Now;
                    db.Entry(bldrm).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Başarılı.");
        }
    }
}