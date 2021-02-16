using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class MesajlarimController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        int mesajSize = 3;
        
        public async Task<ActionResult> Index()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string queryGorevCount = "select m.*, "
                + "(case when (m.alan_id = " + lgm.id + ") then k2.ad else k1.ad END) as alan_ad, (case when (m.alan_id = " + lgm.id + ") then k2.soyad else k1.soyad END) as alan_soyad, "
                + "(case when (m.gonderen_id = " + lgm.id + ") then k2.ad else k1.ad END) as gonderen_ad, (case when (m.gonderen_id = " + lgm.id + ") then k2.soyad else k1.soyad END) as gonderen_soyad "
                + "from (SELECT m1.* FROM mesajlar m1 LEFT JOIN mesajlar m2 ON (m1.parent_url = m2.parent_url AND m1.date < m2.date) WHERE m2.id IS NULL) as m "
                + "inner join kullanicilar as k1 on k1.id = m.alan_id "
                + "inner join kullanicilar as k2 on k2.id = m.gonderen_id "
                + "where m.flag != " + durumlar.silindi + " and (m.alan_id = " + lgm.id + " or m.gonderen_id = " + lgm.id + ") order by m.date desc;";
            var m = db.Database.SqlQuery<MesajlarDetayModel2>(queryGorevCount).ToListAsync();

            await Task.WhenAll(m);

            List<MesajlarDetayModel2> mesajList = m.Result;
            return View(mesajList);
        }
        public async Task<ActionResult> MesajimGoster(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();

            List<object> nesneler = new List<object>();

            var kl = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.id != lgm.id).ToListAsync();
            var m = db.mesajlar.Where(e => e.flag != durumlar.silindi && e.url.Equals(id)).FirstOrDefaultAsync();

            await Task.WhenAll(kl, m);
            List<kullanicilar> kullaniciList = kl.Result;
            mesajlar msj = m.Result;

            if (msj == null)
            {
                msj = new mesajlar();
            }

            List<mesajlar> mesajList = db.mesajlar.Where(e => e.flag != durumlar.silindi && e.parent_url.Equals(msj.parent_url)).OrderByDescending(e => e.date).Take(mesajSize).ToList();

            nesneler.Add(kullaniciList);
            nesneler.Add(msj);
            nesneler.Add(mesajList.OrderBy(e => e.date).ToList());
            nesneler.Add(lgm);


            if (msj.alan_id == lgm.id)
            {
                mesajIslemleri.okunduIsaretle(msj.parent_url);
            }

            return View(nesneler);
        }
        public JsonResult mesajGonder(string url)
        {
            try
            {
                mesajIslemleri mic = new mesajIslemleri();
                JsonSonuc sonuc = mic.yeniMesaj(Request);
                if (sonuc.Equals("") || sonuc.Equals("proje_sayisi_hatasi"))
                {
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<JsonResult> mesajList(string url, string son_tarih, int once1Sonra2)
        {
            try
            {
                DateTime dt = DateTime.Parse(son_tarih);
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string queryGorevCount = "";
                if (once1Sonra2 == 1)
                {
                    queryGorevCount = "select m.* "
                    + "from mesajlar as m "
                    + "where m.flag != " + durumlar.silindi + " and m.date < DATE_FORMAT('" + dt.ToString("yyyy-MM-dd HH:mm:ss") + " ','%Y-%m-%d %H:%i:%s') and m.parent_url = '" + url + "' and m.firma_id = " + lgm.firma_id + " and (m.alan_id = " + lgm.id + " or m.gonderen_id = " + lgm.id + ") order by m.date desc Limit " + mesajSize.ToString() + ";";
                }
                else
                {
                    queryGorevCount = "select m.* "
                    + "from mesajlar as m "
                    + "where m.flag != " + durumlar.silindi + " and m.date > DATE_FORMAT('" + dt.ToString("yyyy-MM-dd HH:mm:ss") + " ','%Y-%m-%d %H:%i:%s') and m.parent_url = '" + url + "' and m.firma_id = " + lgm.firma_id + " and (m.alan_id = " + lgm.id + " or m.gonderen_id = " + lgm.id + ") order by m.date;";
                }

                var m = db.Database.SqlQuery<MesajlarDetayModel>(queryGorevCount).ToListAsync();

                await Task.WhenAll(m);

                List<MesajlarDetayModel> mesajList = m.Result;

                if (mesajList.Count != 0 && mesajList[0].alan_id == lgm.id)
                {
                    mesajIslemleri.okunduIsaretle(mesajList[0].parent_url);
                }

                if (once1Sonra2 == 1)
                {
                    return Json(JsonSonuc.sonucUret(true, mesajList), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(true, mesajList.OrderBy(e => e.date).ToList()), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}