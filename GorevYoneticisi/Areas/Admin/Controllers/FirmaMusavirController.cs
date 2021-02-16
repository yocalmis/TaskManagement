using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class FirmaMusavirController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<firma_musavir> fmList = db.firma_musavir.Where(e => e.flag != durumlar.silindi).ToList();
            return View(fmList);
        }
        public ActionResult yeniFirmaMusavir()
        {
            return View();
        }
        [HttpPost]
        public JsonResult newFirmaMusavir()
        {
            try
            {
                firmaMusavirIslemleri mic = new firmaMusavirIslemleri();
                string sonuc = mic.yeniFirmaMusavir(Request);
                if (sonuc.Equals(""))
                {
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult firmaMusavirDuzenle(string id)
        {
            try
            {
                firma_musavir fm = db.firma_musavir.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (fm == null)
                {
                    return RedirectToAction("Index");
                }
                
                return View(fm);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public JsonResult editFirmaMusavir(string url)
        {
            try
            {
                firmaMusavirIslemleri mic = new firmaMusavirIslemleri();
                string sonuc = mic.firmaMusavirDuzenle(url, Request);
                if (sonuc.Equals(""))
                {
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult silFirmaMusavir(string id)
        {
            try
            {
                firma_musavir fm = db.firma_musavir.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (fm == null)
                {
                    return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                }
                fm.flag = durumlar.silindi;
                db.Entry(fm).State = EntityState.Modified;

                if (fm.id == GetCurrentUser.GetUser().firma_id)
                {
                    kullaniciIslemleri ki = new kullaniciIslemleri();
                    ki.resetLoginInfo();
                }
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