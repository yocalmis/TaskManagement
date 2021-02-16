using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    [AreaAuthorize("Admin", "")]
    public class SystemController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        // GET: Admin/System
        public ActionResult Index()
        {
            sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();
            if (sa == null)
            {
                sa = new sistem_ayarlari();
                sa.mail_ssl = "true";
            }
            return View(sa);
        }
        [HttpPost]
        public JsonResult ayarlariKaydet()
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int vid = 1;
                if (db.sistem_ayarlari.Count() != 0)
                {
                    vid = db.sistem_ayarlari.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.sistem_ayarlari.Count() != 0)
                {
                    sort = db.sistem_ayarlari.Max(e => e.sort) + 1;
                }
                sistem_ayarlari sa = new sistem_ayarlari();
                foreach (var property in sa.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null)
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = sa.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(sa, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(sa, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }
                sa.vid = vid;
                sa.date = DateTime.Now;
                sa.ekleyen = lgm.id;
                sa.sort = sort;
                sa.flag = durumlar.aktif;

                List<sistem_ayarlari> saList = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).ToList();
                foreach (sistem_ayarlari dbSa in saList)
                {
                    dbSa.flag = durumlar.silindi;
                    db.Entry(dbSa).State = EntityState.Modified;
                }

                db.sistem_ayarlari.Add(sa);
                db.SaveChanges();

                return Json(JsonSonuc.sonucUret(true, "Ayarlar kaydedildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
    }
}