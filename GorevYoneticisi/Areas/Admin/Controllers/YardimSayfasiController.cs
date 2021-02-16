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
    public class YardimSayfasiController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        public ActionResult Index()
        {
            List<yardim> yardimList = db.yardim.Where(e => e.flag == durumlar.aktif).OrderByDescending(e => e.id).ToList();
            return View(yardimList);
        }
        public ActionResult YardimEkle()
        {
            return View();
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult YardimEkle(string icerik)
        {
            try
            {
                int vid = 1;
                if (db.yardim.Count() != 0)
                {
                    vid = db.yardim.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.yardim.Count() != 0)
                {
                    sort = db.yardim.Max(e => e.sort) + 1;
                }

                yardim yrd = new yardim();
                foreach (var property in yrd.GetType().GetProperties())
                {
                    try
                    {
                        var response2 = Request[property.Name];
                        if (response2 == null && property.PropertyType != typeof(int))
                        {
                            if (response2 == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = yrd.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(yrd, Convert.ChangeType(Decimal.Parse(response2.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response2 == null)
                                {
                                    propertyS.SetValue(yrd, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(yrd, Convert.ChangeType(Decimal.Parse(response2.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(yrd, Convert.ChangeType(response2, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }               

                string strImageName = StringFormatter.OnlyEnglishChar(yrd.baslik);

                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                yardim pg = new yardim();
                do
                {
                    pg = db.yardim.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
                    if (pg != null)
                    {
                        tempUrl = tempUrl + i.ToString();
                    }
                    else
                    {
                        createdUrl = tempUrl;
                        bulundu = true;
                    }
                    i++;
                } while (!bulundu);
                yrd.url = createdUrl;

                yrd.icerik = icerik;
                yrd.flag = durumlar.aktif;
                yrd.vid = vid;
                yrd.date = DateTime.Now;
                yrd.sort = sort;
                yrd.ekleyen = GetCurrentUser.GetUser().id;

                db.yardim.Add(yrd);
                db.SaveChanges();

                return Json(JsonSonuc.sonucUret(true, yrd.url), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult YardimDuzenle(string id)
        {
            yardim yrd = db.yardim.Where(e => e.url.Equals(id)).FirstOrDefault();
            if (yrd == null)
            {
                return RedirectToAction("Index");
            }
            return View(yrd);
        }
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult YardimDuzenle(string url, string icerik)
        {
            try
            {
                yardim yrd = db.yardim.Where(e => e.url.Equals(url)).FirstOrDefault();
                string tempUrl = yrd.url;
                foreach (var property in yrd.GetType().GetProperties())
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
                            PropertyInfo propertyS = yrd.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(yrd, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(yrd, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }
                yrd.url = tempUrl;
                yrd.icerik = icerik;
                db.Entry(yrd).State = EntityState.Modified;
                db.SaveChanges();

                return Json(JsonSonuc.sonucUret(true, yrd.url), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult silYardim(string id)
        {
            try
            {
                yardim yrd = db.yardim.Where(e => e.url.Equals(id)).FirstOrDefault();
                if (yrd == null)
                {
                    return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                }
                yrd.flag = durumlar.silindi;
                db.Entry(yrd).State = EntityState.Modified;
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