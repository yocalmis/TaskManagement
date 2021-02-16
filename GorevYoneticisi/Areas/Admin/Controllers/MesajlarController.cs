using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class MesajlarController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<object> nesneler = new List<object>();
            string query = "select count(s.id) as count, s.* from smsler as s  "
                + "where s.sms_grup_id > 0 group by sms_grup_id;";
            List<SmslerCountModel> mailList = db.Database.SqlQuery<SmslerCountModel>(query).ToList();
            nesneler.Add(mailList);
            return View(nesneler);
        }
        public async Task<ActionResult> yeniSms(string id)
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
                string query2 = "select hedef_id from smsler as s where s.sms_grup_id = " + id;
                var k = db.Database.SqlQuery<KullaniciFirmaOzetModel>(query).ToListAsync();
                var s = db.smsler.Where(e => e.flag == durumlar.aktif && e.sms_grup_id == intId).FirstOrDefaultAsync();
                var k2 = db.Database.SqlQuery<int>(query2).ToListAsync();
                await Task.WhenAll(k, s, k2);
                List<KullaniciFirmaOzetModel> kullaniciList = k.Result;
                smsler sms = s.Result;
                List<int> kullaniciIdList = k2.Result;
                nesneler.Add(kullaniciList);
                nesneler.Add(sms);
                nesneler.Add(kullaniciIdList);
                return View(nesneler);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }
        public JsonResult smsGonder(string[] kullaniciList)
        {
            try
            {
                string icerik = Request["icerik"];
                int groupId = SendSms.getGroupId();
                if (icerik.Length > 160)
                {
                    return Json(JsonSonuc.sonucUret(false, "Sms mesajı en fazla 160 karakter olabilir. Lütfen mesajı kısaltıp tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                List<string> numaraList = new List<string>();
                List<kullanicilar> userList = new List<kullanicilar>();
                foreach (string str in kullaniciList)
                {
                    int userId = Convert.ToInt32(str);
                    kullanicilar usr = db.kullanicilar.Where(e => e.id == userId).FirstOrDefault();                  
                    if (usr != null && usr.sms_permission == Permissions.granted)
                    {
                        numaraList.Add(usr.tel);
                        userList.Add(usr);
                    }
                }
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                SendSms sms = new SendSms();
                sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();
                bool sonuc = sms.SendSMS(numaraList.ToArray(), icerik, sa.sms_header, "_admin_");
                if (!sonuc)
                {
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                foreach (kullanicilar usr in userList)
                {
                    SendSms.smsKaydet(icerik, durumlar.aktif, MailHedefTur.kullanici, usr.id, usr.tel, lgm.id, groupId);
                }
                return Json(JsonSonuc.sonucUret(true, "Sms Gönderildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}