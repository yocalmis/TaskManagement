using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class KullanicilarController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string userQuery = "select k.id, k.flag, k.ad, k.soyad, k.email, k.tel, k.kullanici_turu, k.url, fm.firma_adi from kullanicilar as k "
                + "left join firma_musavir as fm on k.firma_id = fm.id and fm.flag = " + durumlar.aktif.ToString() + " where k.id != " + lgm.id + " and k.flag != " + durumlar.silindi.ToString();
            List<KullaniciFirmaOzetModel> users = db.Database.SqlQuery<KullaniciFirmaOzetModel>(userQuery).ToList();
            return View(users);
        }
        public ActionResult yeniKullanici()
        {
            List<firma_musavir> firmaMusavirList = db.firma_musavir.Where(e => e.flag == durumlar.aktif).OrderBy(e => e.firma_adi).ToList();
            return View(firmaMusavirList);
        }
        [HttpPost]
        public JsonResult newKullanici(string password, string password_control, string mail_permission, string sms_permission)
        {
            try
            {
                if (password.Equals(string.Empty))
                {
                    return Json(JsonSonuc.sonucUret(false, "Şifre alanı boş bırakılamaz."), JsonRequestBehavior.AllowGet);
                }
                if (!password.Equals(password_control))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz şifreler eşleşmiyor. Lütfen şifrelerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                kullaniciIslemleri mic = new kullaniciIslemleri();
                string sonuc = mic.yeniKullanici(password, password_control, mail_permission, sms_permission, Request);
                if (sonuc.Equals("") || sonuc.Equals("email_unique")
                    || sonuc.Equals("username_unique") || sonuc.Equals("kullanici_sayisi_hatasi"))
                {
                    if (sonuc.Equals("email_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("username_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz kullanıcı adını başka bir kullanıcı kullanmaktadır. Lütfen farklı bir kullanıcı adı deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("kullanici_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu firmaya başka kullanıcı eklenemez."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("email_unique"))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult kullaniciDuzenle(string id)
        {
            try
            {
                List<object> nesneler = new List<object>();
                kullanicilar user = db.kullanicilar.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (user == null)
                {
                    return RedirectToAction("Index");
                }
                List<firma_musavir> firmaMusavirList = db.firma_musavir.Where(e => e.flag == durumlar.aktif).OrderBy(e => e.firma_adi).ToList();
                nesneler.Add(user);
                nesneler.Add(firmaMusavirList);
                return View(nesneler);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public ActionResult editKullanici(string url, string password, string password_control, string mail_permission, string sms_permission)
        {
            try
            {
                if (!password.Equals(password_control))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz şifreler eşleşmiyor. Lütfen şifrelerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                kullaniciIslemleri mic = new kullaniciIslemleri();
                string sonuc = mic.kullaniciDuzenle(url, password, password_control, mail_permission, sms_permission, Request);
                if (sonuc.Equals("") || sonuc.Equals("email_unique") || sonuc.Equals("kullanici_sayisi_hatasi") || sonuc.Equals("username_unique"))
                {
                    if (sonuc.Equals("email_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("username_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz kullanıcı adını başka bir kullanıcı kullanmaktadır. Lütfen farklı bir kullanıcı adı deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("kullanici_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu firmaya başka kullanıcı eklenemez."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("unique_email"))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpPost]
        public JsonResult silKullanici(string id)
        {
            try
            {
                kullanicilar user = db.kullanicilar.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (user == null)
                {
                    return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                }
                user.flag = durumlar.silindi;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
            }
            return Json(FormReturnTypes.basarili, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult emailOnayla(string id)
        {
            try
            {
                kullanicilar user = db.kullanicilar.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (user == null)
                {
                    return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                }
                user.flag = durumlar.aktif;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
            }
            return Json(FormReturnTypes.basarili, JsonRequestBehavior.AllowGet);
        }
    }
}