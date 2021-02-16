using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class MusterilerController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<musteriler> musteriList = db.musteriler.Where(e => e.flag != durumlar.silindi).OrderBy(e => e.firma_adi).ToList();
            return View(musteriList);
        }
        public async Task<ActionResult> Musteri(string id)
        {
            List<object> nesneler = new List<object>();
            var m = db.musteriler.Where(e => e.flag != durumlar.silindi && e.url.Equals(id)).FirstOrDefaultAsync();
            var fm = db.firma_musavir.Where(e => e.flag != durumlar.silindi).ToListAsync();
            await Task.WhenAll(m, fm);
            musteriler mstr = m.Result;
            if (mstr == null)
            {
                mstr = new musteriler();
            }
            List<firma_musavir> firmaList = fm.Result;
            nesneler.Add(mstr);
            nesneler.Add(firmaList);
            return View(nesneler);
        }
        [HttpPost]
        public JsonResult MusteriDuzenle(string url)
        {
            try
            {
                int firma_id = Convert.ToInt32(Request["firma_id"].ToString());
                musteriIslemleri mic = new musteriIslemleri();
                string sonuc = mic.musteriDuzenle(url, firma_id, Request);
                if (sonuc.Equals("") || sonuc.Equals("musteri_sayisi_hatasi"))
                {
                    if (sonuc.Equals("musteri_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu firmaya başka müşteri/mükellef eklenemez."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult silMusteri(string id)
        {
            try
            {
                musteriler mstr = db.musteriler.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (mstr == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Müşteri/Mükellef bulunamadı."), JsonRequestBehavior.AllowGet);
                }
                mstr.flag = durumlar.silindi;
                db.Entry(mstr).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonSonuc.sonucUret(true, "Müşteri/Mükellef silindi."), JsonRequestBehavior.AllowGet);
        }
    }
}