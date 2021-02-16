using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class AHomeController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public async Task<ActionResult> Index()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<object> nesneler = new List<object>();
            var fm = db.firma_musavir.Where(e => e.flag == durumlar.aktif).ToListAsync();
            var k = db.kullanicilar.Where(e => e.flag == durumlar.aktif).ToListAsync();

            string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ((select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select ps.id, 2 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select ps.id, 3 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where (ps.durum = " + TamamlamaDurumlari.pasif + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select g.id, 1 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + " and gp.id is null) "
                        + "union "
                        + "(select g.id, 2 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") and gp.id is null) "
                        + "union "
                        + "(select g.id, 3 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") "
                        + " and gp.id is null) order by oncelik, bitis_tarihi) as tbl";

            var psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToListAsync();

            await Task.WhenAll(fm, k, psg);

            List<GorevVeProjeSurecOzetModel> projeSurecGorevList2 = psg.Result;
            List<firma_musavir> fmList = fm.Result;
            List<kullanicilar> kList = k.Result;
            nesneler.Add(fmList);
            nesneler.Add(kList);
            nesneler.Add(projeSurecGorevList2);
            return View(nesneler);

            //return View();
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult kullaniciProjeGorevleri(string kullanici_url)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ((select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select ps.id, 2 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select ps.id, 3 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select g.id, 1 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + "and kg.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "union "
                        + "(select g.id, 2 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") and kg.flag = " + durumlar.aktif.ToString() + " "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "union "
                        + "(select g.id, 3 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") and kg.flag = " + durumlar.aktif.ToString() + " "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "order by oncelik, bitis_tarihi) as tbl";
                List<GorevVeProjeSurecOzetModel> psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToList();
                return Json(JsonSonuc.sonucUret(true, psg), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcı görevleri getirilemedi. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult musteriProjeGorevleri(string musteriUrl)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ( "
                    + "(select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                    + "inner join firma_musavir as fm on fm.id = ps.firma_id and fm.flag = " + durumlar.aktif + " "
                    + "where ps.flag = " + durumlar.aktif + " and fm.url = '" + musteriUrl + "')" 
                    + " union "
                    + "(select g.id, 1 as oncelik, g.isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                    + "inner join firma_musavir as fm on fm.id = g.firma_id and fm.flag = " + durumlar.aktif + " "
                    + "where g.flag = " + durumlar.aktif + " and fm.url = '" + musteriUrl + "')) as tbl";
                List<GorevVeProjeSurecOzetModel> psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToList();
                return Json(JsonSonuc.sonucUret(true, psg), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcı görevleri getirilemedi. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult anasayfaProjeGorevleriniGetir(string id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select g.id, 1 as oncelik, g.isim, DATE_FORMAT(g.baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                + "inner join gorev_proje as gp on gp.flag = " + durumlar.aktif + " and g.id = gp.gorev_id and gp.proje_id = " + id + " "
                + "where g.flag = " + durumlar.aktif + " order by g.isim";
            List<GorevVeProjeSurecOzetModel> mpList = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(pkQuery).ToList();
            return Json(JsonSonuc.sonucUret(true, mpList));
        }



        public ActionResult ayarlar()
        {
            int currenId = GetCurrentUser.GetUser().id;
            kullanicilar user = db.kullanicilar.Where(e => e.id == currenId).FirstOrDefault();
            return View(user);
        }
        [HttpPost]
        public JsonResult ayarlarKaydet(string mail_permission, string sms_permission)
        {
            try
            {
                kullaniciIslemleri mic = new kullaniciIslemleri();
                string sonuc = mic.kullaniciDuzenle(GetCurrentUser.GetUser().url, "", "", mail_permission, sms_permission, Request);
                if (sonuc.Equals("") || sonuc.Equals("email_unique") || sonuc.Equals("username_unique"))
                {
                    if (sonuc.Equals("email_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("username_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz kullanıcı adını başka bir kullanıcı kullanmaktadır. Lütfen farklı bir kullanıcı adı deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, "Bilgileriniz Güncellendi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("unique_email"))
                {
                    //return Json(FormReturnTypes.unique_email, JsonRequestBehavior.AllowGet);
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult sifreAyarlari()
        {
            return View();
        }
        [HttpPost]
        public JsonResult sifreAyarlari(string current_password, string password, string password_control)
        {
            try
            {
                if (!password.Equals(password_control))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz şifreler eşleşmiyor. Lütfen şifrelerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                string sifre = HashWithSha.ComputeHash(current_password, "SHA512", Encoding.ASCII.GetBytes(current_password));
                int id = GetCurrentUser.GetUser().id;
                kullanicilar dbUser = db.kullanicilar.Where(e => e.id == id && e.password == sifre && e.flag == durumlar.aktif).FirstOrDefault();
                if (dbUser == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Mevcut şifreniz doğrulanamadi."), JsonRequestBehavior.AllowGet);
                }
                if (!password.Equals(""))
                {
                    dbUser.password = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
                }

                db.Entry(dbUser).State = EntityState.Modified;
                db.SaveChanges();

                return Json(JsonSonuc.sonucUret(true, "Şifreniz Güncellendi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> getFirmaMusteri(string firma_id)
        {
            try
            {
                string queryGorevCount = "select * from musteriler where flag = " + durumlar.aktif.ToString() + " and firma_id = " + firma_id + ";";

                var m = db.Database.SqlQuery<MusterilerModel>(queryGorevCount).ToListAsync();

                await Task.WhenAll(m);

                List<MusterilerModel> musteriList = m.Result;
                return Json(JsonSonuc.sonucUret(true, musteriList.OrderBy(e => e.date).ToList()), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}