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
    public class MaillerController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<object> nesneler = new List<object>();
            string query = "select count(m.id) as count, m.* from mailler as m "
                + "where m.mail_grup_id > 0 group by mail_grup_id;";
            List<MaillerCountModel> mailList = db.Database.SqlQuery<MaillerCountModel>(query).ToList();
            nesneler.Add(mailList);
            return View(nesneler);
        }
        public async Task<ActionResult> yeniMail(string id)
        {
            try
            {
                if (id == null)
                {
                    id = "-200";
                }
                int intId = Convert.ToInt32(id);
                List<object> nesneler = new List<object>();
                string query = "select k.id, k.ad, k.soyad, k.email, k.tel, k.kullanici_turu, k.url, fm.firma_adi from kullanicilar as k "
                    + "inner join firma_musavir as fm on fm.id = k.firma_id "
                    + "where k.flag = " + durumlar.aktif + " and fm.flag = " + durumlar.aktif + " order by k.firma_id, k.ad;";
                string query2 = "select hedef_id from mailler as m where m.mail_grup_id = " + id;
                var k = db.Database.SqlQuery<KullaniciFirmaOzetModel>(query).ToListAsync();
                var m = db.mailler.Where(e => e.flag == durumlar.aktif && e.mail_grup_id == intId).FirstOrDefaultAsync();
                var k2 = db.Database.SqlQuery<int>(query2).ToListAsync();
                await Task.WhenAll(k, m, k2);
                List<KullaniciFirmaOzetModel> kullaniciList = k.Result;
                mailler mail = m.Result;
                List<int> kullaniciIdList = k2.Result;
                nesneler.Add(kullaniciList);
                nesneler.Add(mail);
                nesneler.Add(kullaniciIdList);
                return View(nesneler);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
        public JsonResult mailGonder(string[] kullaniciList)
        {
            try
            {
                string icerik = Request.Unvalidated["icerik"];
                string konu = Request["konu"];
                int groupId = EmailFunctions.getGroupId();
                foreach (string str in kullaniciList)
                {
                    int userId = Convert.ToInt32(str);
                    kullanicilar usr = db.kullanicilar.Where(e => e.id == userId).FirstOrDefault();
                    if (usr != null && usr.mail_permission == Permissions.granted)
                    {
                        bool mailSonuc = EmailFunctions.sendEmailGmail(icerik, konu, usr.email, MailHedefTur.kullanici, usr.id, EmailFunctions.mailAdresi, 0, "", "", "", "", groupId);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, "Mail Gönderildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}