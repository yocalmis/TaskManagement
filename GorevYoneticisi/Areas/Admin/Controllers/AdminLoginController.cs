using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace GorevYoneticisi.Areas.Admin.Controllers
{
    public class AdminLoginController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();
        [AllowAnonymous]
        public ActionResult Index()
        {
            //if (String.IsNullOrEmpty(HttpContext.User.Identity.Name) || !User.Identity.IsAuthenticated)
            LoggedUserModel lum = GetCurrentUser.GetUser();
            if (lum == null || !(lum.kullanici_turu == KullaniciTurleri.super_admin))
            {
                FormsAuthentication.SignOut();
                return View();
            }
            return Redirect("/Admin/AHome");
        }
        [AllowAnonymous]
        [HttpPost]
        public JsonResult Index(string username, string password)
        {
            /*var response = Request["g-recaptcha-response"];
            if (!validateCaptcha.validateC(response))
            {
                return Json(FormReturnTypes.captchaHatasi, JsonRequestBehavior.AllowGet);
            }*/
            string sifre = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
            kullanicilar usr = db.kullanicilar.Where(e => e.username == username && e.password == sifre && e.flag == durumlar.aktif && e.kullanici_turu == KullaniciTurleri.super_admin).FirstOrDefault();
            if (usr != null)
            {
                LoggedUserModel loggedUser = new LoggedUserModel();
                foreach (var property in loggedUser.GetType().GetProperties())
                {
                    try
                    {
                        var response = usr.GetType().GetProperty(property.Name).GetValue(usr, null).ToString();
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = loggedUser.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(loggedUser, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(loggedUser, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(loggedUser, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                            }
                            else
                            {
                                propertyS.SetValue(loggedUser, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                Guid gd = new Guid();
                gd = Guid.NewGuid();

                string query = "select * from firma_musavir where flag = " + durumlar.aktif.ToString() + " and id = " + loggedUser.firma_id;
                FirmaMusavirModel fmm = db.Database.SqlQuery<FirmaMusavirModel>(query).FirstOrDefault();
                loggedUser.fm = fmm;

                SetAuthCookie(gd.ToString(), true, loggedUser);
                return Json(FormReturnTypes.basarili, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LogOff(string nextUrl)
        {
            FormsAuthentication.SignOut();
            //Session.Abandon();
            if (nextUrl == null || nextUrl.Equals(""))
            {
                return RedirectToAction("Index", "Adminlogin");
            }
            //System.Web.HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            /*HttpCookie currentUserCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            System.Web.HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
            currentUserCookie.Expires = DateTime.Now.AddDays(-10);
            currentUserCookie.Value = null;
            System.Web.HttpContext.Current.Response.SetCookie(currentUserCookie);*/
            //System.Web.HttpContext.Current.Response.Cookies.Remove(GetCurrentUser.GetUserAdmin().email);
            return Redirect("~" + nextUrl);
        }
        public void SetAuthCookie(string userName, bool createPersistentCookie, LoggedUserModel userData)
        {
            HttpCookie cookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie);
            //String jsonUser = Json(userData, JsonRequestBehavior.AllowGet).ToString();

            var serializer = new JavaScriptSerializer();
            string jsonUser = serializer.Serialize(userData);

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                 ticket.Version, ticket.Name, ticket.IssueDate, ticket.IssueDate.AddYears(1)
                , ticket.IsPersistent, jsonUser, ticket.CookiePath
            );

            string encTicket = FormsAuthentication.Encrypt(newTicket);
            cookie.Value = encTicket;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        } // End Sub SetAuthCookie
    }
}