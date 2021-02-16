using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using GorevYoneticisi.KayitveGuncellemeIslemleri;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class SureclerController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<proje_surec> surecList = db.proje_surec.Where(e => e.flag != durumlar.silindi && e.tur == ProjeSurecTur.surec).OrderBy(e => e.bitis_tarihi).ToList();
            return View(surecList);
        }
        public async Task<ActionResult> SurecBilgisi(string id)
        {
            List<object> nesneler = new List<object>();

            var p = db.proje_surec.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.tur == ProjeSurecTur.surec).FirstOrDefaultAsync();
            var fl = db.firma_musavir.Where(e => e.flag != durumlar.silindi).ToListAsync();

            GorevlerController hc = new GorevlerController();
            var grvl = hc.GorevleriGetir(id);

            await Task.WhenAll(p, fl, grvl);

            ViewResult grvResult = (ViewResult)grvl.Result;
            List<GorevVeProjeOzetModel> gorevList = (List<GorevVeProjeOzetModel>)(grvResult.Model);

            proje_surec prj = p.Result;
            if (prj == null)
            {
                prj = new proje_surec();
                prj.baslangic_tarihi = DateTime.Now;
                prj.bitis_tarihi = DateTime.Now.AddMonths(1);
            }

            List<firma_musavir> firmaList = fl.Result;


            vrlfgysdbEntities db2 = new vrlfgysdbEntities();
            var ml = db2.musteriler.Where(e => e.flag == durumlar.aktif && e.firma_id == prj.firma_id).ToListAsync();
            /*vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == prj.firma_id).ToListAsync();*/
            var pm = db.proje_musteri.Where(e => e.proje_id == prj.id && e.flag == durumlar.aktif).FirstOrDefaultAsync();

            //await Task.WhenAll(ml, kl, pm);
            await Task.WhenAll(ml, pm);

            List<musteriler> musteriList = ml.Result;

            proje_musteri pmust = pm.Result;
            if (pmust == null)
            {
                pmust = new proje_musteri();
            }

            //List<kullanicilar> kullaniciList = kl.Result;

            nesneler.Add(prj);
            nesneler.Add(musteriList);
            nesneler.Add(pmust);
            //nesneler.Add(kullaniciList);
            nesneler.Add(firmaList);
            nesneler.Add(gorevList);

            return View(nesneler);
        }
        [HttpPost]
        public JsonResult SurecDuzenle(string url)
        {
            try
            {
                surecIslemleri mic = new surecIslemleri();
                int firma_id = Convert.ToInt32(Request["firma_id"].ToString());
                string sonuc = mic.surecDuzenle(url, firma_id, Request);
                if (sonuc.Equals("") || sonuc.Equals("surec_sayisi_hatasi") || sonuc.Equals("surec_isim_hatasi"))
                {
                    if (sonuc.Equals("surec_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu firmaya başka Süreç eklenemez."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("surec_isim_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu isimde ve tarihte başka bir süreç olduğundan süreç eklenemedi."), JsonRequestBehavior.AllowGet);
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
        public JsonResult silSurec(string id)
        {
            surecIslemleri pis = new surecIslemleri();
            JsonSonuc sonuc = pis.silSurec(id, 0);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }
    }
}