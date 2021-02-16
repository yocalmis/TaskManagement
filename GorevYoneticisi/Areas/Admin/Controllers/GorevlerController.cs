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
    public class GorevlerController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string gorevQuery = "select g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag != " + durumlar.silindi.ToString() + " order by g.bitis_tarihi";

            List<GorevVeProjeOzetModel> gorevList = db.Database.SqlQuery<GorevVeProjeOzetModel>(gorevQuery).ToList();
            return View(gorevList);
        }
        public async Task<ActionResult> GorevleriGetir(string id)//id proje url yerine geçiyor
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string gorevQuery = "select g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag != " + durumlar.silindi.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + " order by g.bitis_tarihi";

            List<GorevVeProjeOzetModel> gorevList = db.Database.SqlQuery<GorevVeProjeOzetModel>(gorevQuery).ToList();
            return View(gorevList);
        }
        public async Task<ActionResult> GorevBilgisi(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();

            List<object> nesneler = new List<object>();

            var g = db.gorevler.Where(e => e.flag != durumlar.silindi && e.url.Equals(id)).FirstOrDefaultAsync();
            vrlfgysdbEntities db2 = new vrlfgysdbEntities();
            var ml = db2.musteriler.Where(e => e.flag == durumlar.aktif).ToListAsync();
            vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif).ToListAsync();
            vrlfgysdbEntities db4 = new vrlfgysdbEntities();
            var p = db4.proje_surec.Where(e => e.flag == durumlar.aktif && e.tur == ProjeSurecTur.proje).ToListAsync();
            vrlfgysdbEntities db5 = new vrlfgysdbEntities();
            var s = db5.proje_surec.Where(e => e.flag == durumlar.aktif && e.tur == ProjeSurecTur.surec).ToListAsync();

            await Task.WhenAll(g, ml, kl, p, s);
            var grv = g.Result;
            List<musteriler> musteriList = ml.Result;
            if (grv == null)
            {
                grv = new gorevler();
                grv.baslangic_tarihi = DateTime.Now;
                grv.bitis_tarihi = DateTime.Now.AddMonths(1);
            }

            List<kullanicilar> kullaniciList = kl.Result;

            nesneler.Add(grv);
            nesneler.Add(p.Result);
            nesneler.Add(s.Result);
            nesneler.Add(musteriList);
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin || lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(kullaniciList);
            }
            else
            {
                nesneler.Add(null);
            }
            vrlfgysdbEntities db6 = new vrlfgysdbEntities();
            var ytk = db6.kullanici_gorev.Where(e => e.flag == durumlar.aktif && e.gorev_id == grv.id && e.kullanici_id == lgm.id).FirstOrDefaultAsync();
            vrlfgysdbEntities db7 = new vrlfgysdbEntities();
            var gp = db7.gorev_proje.Where(e => e.flag == durumlar.aktif && e.gorev_id == grv.id).FirstOrDefaultAsync();
            vrlfgysdbEntities db8 = new vrlfgysdbEntities();
            var frm = db8.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == grv.firma_id).FirstOrDefaultAsync();

            await Task.WhenAll(ytk, gp, frm);

            kullanici_gorev kullaniciGorev = ytk.Result;
            gorev_proje gorevProje = gp.Result;
            firma_musavir firma = frm.Result;

            if (gorevProje == null)
            {
                gorevProje = new gorev_proje();
            }
            nesneler.Add(gorevProje);
            nesneler.Add(lgm);
            nesneler.Add(firma);

            return View(nesneler);
        }

        public JsonResult gorevKullanicilariGetir(int id)
        {
            try
            {
                List<KullaniciProjeOzetModel> ozetKullaniciList = gorevKullanicisiIslemleri.getGorevKullanicilarOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcılar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gorevMusterileriGetir(int id)
        {
            try
            {
                List<MusteriProjeOzetModel> ozetMusteriList = gorevIslemleri.getGorevMusterilerOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetMusteriList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Müşteriler getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gorevDosyalariGetir(int id)
        {
            try
            {
                List<dosyaOzetModel> ozetDosyaList = gorevIslemleri.getGorevDosyalarOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetDosyaList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Dosyalar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult gorevLoglariGetir(int id)
        {
            try
            {
                List<GorevLogOzet> ozetLogList = gorevIslemleri.getGorevLoglariOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetLogList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Dosyalar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult yapilacaklarList(string gorev_url)
        {
            try
            {
                yapilacakIslemleri yis = new yapilacakIslemleri();
                JsonSonuc sonuc = yis.yapilacaklariGetir(gorev_url);
                return Json(sonuc, JsonRequestBehavior.AllowGet); ;
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Yapılacaklar listesi getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}