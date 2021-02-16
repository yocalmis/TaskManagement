using GorevYoneticisi.KayitveGuncellemeIslemleri;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace GorevYoneticisi.Controllers
{
    public class HomeController : Controller
    {
        vrlfgysdbEntities db = new vrlfgysdbEntities();

        public JsonResult passwordEncoder(string text)
        { 
            string password = text;
            password = HashWithSha.ComputeHash(text, "SHA512", Encoding.ASCII.GetBytes(text));
            return Json(JsonSonuc.sonucUret(true, password), JsonRequestBehavior.AllowGet);
        }

        #region kullanıcı ve login işlemleri
        public ActionResult LoginCTest()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            if (Session["LoginC"] != null)
            {
                /*kullanicilar kullanici = Session["LoginC"] as kullanicilar;
                JsonResult sonuc = Login(kullanici.email, kullanici.password);*/
                return View("Uyari");
            }
            //if (String.IsNullOrEmpty(HttpContext.User.Identity.Name) || !User.Identity.IsAuthenticated)
            LoggedUserModel lum = GetCurrentUser.GetUser();
            if (lum == null || !(lum.kullanici_turu == KullaniciTurleri.user || lum.kullanici_turu == KullaniciTurleri.firma_admin || lum.kullanici_turu == KullaniciTurleri.firma_yetkili))
            {
                FormsAuthentication.SignOut();
                return View();
            }
            return Redirect("/");
        }
        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            /*var response = Request["g-recaptcha-response"];
            if (!validateCaptcha.validateC(response))
            {
                return Json(FormReturnTypes.captchaHatasi, JsonRequestBehavior.AllowGet);
            }*/
            string sifre = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
            kullanicilar usr = db.kullanicilar.Where(e => e.username == username && e.password == sifre && e.flag == durumlar.aktif).FirstOrDefault();
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

                DateTime now = DateTime.Now;
                string query = "select * from firma_musavir where flag = " + durumlar.aktif.ToString() + " and id = " + loggedUser.firma_id;
                FirmaMusavirModel fmm = db.Database.SqlQuery<FirmaMusavirModel>(query).FirstOrDefault();
                loggedUser.fm = fmm;

                if (loggedUser.fm == null || !(now.AddDays(-1) >= loggedUser.fm.baslangic_tarihi && now <= loggedUser.fm.bitis_tarihi))
                {
                    return Json(JsonSonuc.sonucUret(false, "Firmanızın üyeliği bulunamadı. Üyeliğinizin süresi dolmuş ya da üyeliğiniz dondurulmuş olabilir. Lütfen sistem yöneticileri ile irtibata geçiniz."), JsonRequestBehavior.AllowGet);
                }

                Guid gd = new Guid();
                gd = Guid.NewGuid();

                SetAuthCookie(gd.ToString(), true, loggedUser);

                return Json(JsonSonuc.sonucUret(true, ""), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonSonuc.sonucUret(false, "e-Mail adresi yada şifre bulunamadı. Lütfen bilgilerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LogOff(string nextUrl)
        {
            FormsAuthentication.SignOut();
            //Session.Abandon();
            if (nextUrl == null || nextUrl.Equals(""))
            {
                return Redirect("/");
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

            int a = FormsAuthentication.Timeout.Minutes;
            double b = FormsAuthentication.Timeout.TotalMinutes;
            

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

            DateTime c = ticket.IssueDate;

            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                 ticket.Version, ticket.Name, ticket.IssueDate, ticket.IssueDate.AddYears(1)
                , ticket.IsPersistent, jsonUser, ticket.CookiePath
            );

            string encTicket = FormsAuthentication.Encrypt(newTicket);
            cookie.Value = encTicket;
            System.Web.HttpContext.Current.Response.Cookies.Add(cookie);
        } // End Sub SetAuthCookie

        /*[HttpPost]
        public JsonResult LoginC(string email, string password, string password2)
        {

            string guid = "c775daa1-1325-4258-a89a-24bd7c570877";
            //string datee = DateTime.Now.ToString("dd.MM.yyyy");
            //string hash = datee + "_" + guid;
            string hash = guid;
            string hashed = MD5Hash.GetMd5Hash(hash);
            
            if (!password2.Equals(hashed))
            {
                return Json(JsonSonuc.sonucUret(false, "Giriş Başarısız."), JsonRequestBehavior.AllowGet);
            }

            string sifre = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
            kullanicilar usr = db.kullanicilar.Where(e => e.email == email && e.password == sifre && e.flag == durumlar.aktif).FirstOrDefault();
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

                string query = "select * from firma_musavir where flag = " + durumlar.aktif.ToString() + " and id = " + loggedUser.firma_id;
                FirmaMusavirModel fmm = db.Database.SqlQuery<FirmaMusavirModel>(query).FirstOrDefault();
                loggedUser.fm = fmm;

                if (loggedUser.fm == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Herhangi bir firma üyesi olmadığınızdan ya da firmanızın üyeliği iptal edildiğinden giriş başarısız oldu. Bir hata olduğunu düşünüyorsanız sistem yöneticileri ile irtibata geçiniz."), JsonRequestBehavior.AllowGet);
                }

                Guid gd = new Guid();
                gd = Guid.NewGuid();

                SetAuthCookie(gd.ToString(), true, loggedUser);

                return Json(JsonSonuc.sonucUret(true, ""), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonSonuc.sonucUret(false, "e-Mail adresi yada şifre bulunamadı. Lütfen bilgilerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }*/

        public ActionResult LoginC(string username, string password, string password2)
        {

            string guid = "c775daa1-1325-4258-a89a-24bd7c570877";
            //string datee = DateTime.Now.ToString("dd.MM.yyyy");
            //string hash = datee + "_" + guid;
            string hash = guid;
            string hashed = MD5Hash.GetMd5Hash(hash);
            
            if (!password2.Equals(hashed))
            {
                return Json(JsonSonuc.sonucUret(false, "Giriş Başarısız."), JsonRequestBehavior.AllowGet);
            }

            string sifre = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
            kullanicilar usr = db.kullanicilar.Where(e => e.username == username && e.password == sifre && e.flag == durumlar.aktif).FirstOrDefault();
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

                string query = "select * from firma_musavir where flag = " + durumlar.aktif.ToString() + " and id = " + loggedUser.firma_id;
                FirmaMusavirModel fmm = db.Database.SqlQuery<FirmaMusavirModel>(query).FirstOrDefault();
                loggedUser.fm = fmm;

                if (loggedUser.fm == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Herhangi bir firma üyesi olmadığınızdan ya da firmanızın üyeliği iptal edildiğinden giriş başarısız oldu. Bir hata olduğunu düşünüyorsanız sistem yöneticileri ile irtibata geçiniz."), JsonRequestBehavior.AllowGet);
                }

                Guid gd = new Guid();
                gd = Guid.NewGuid();

                SetAuthCookie(gd.ToString(), true, loggedUser);
                kullanicilar kullanici = new kullanicilar();
                kullanici.email = usr.email;
                kullanici.password = password;
                Session["LoginC"] = kullanici;
                return RedirectToAction("MainPage");
                //return Json(JsonSonuc.sonucUret(true, ""), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(JsonSonuc.sonucUret(false, "e-Mail adresi yada şifre bulunamadı. Lütfen bilgilerinizi kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult sifremiUnuttum()
        {
            return View();
        }
        [HttpPost]
        public JsonResult sifremiUnuttum(string email)
        {
            try
            {
                string sonuc = "";
                kullanicilar user = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.email.Equals(email)).FirstOrDefault();
                if (user == null)
                {
                    sonuc = "Kullanıcı bulunamadı.";
                    return Json(JsonSonuc.sonucUret(false, sonuc), JsonRequestBehavior.AllowGet);
                }
                Guid gd = new Guid();
                gd = Guid.NewGuid();
                user.reset_guidexpiredate = DateTime.Now.AddDays(5);
                user.reset_guid = gd.ToString();

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                string icerik = "<div>Şifre sıfırlama talebiniz üzerine bu mail gönderilmiştir. </br><a href=\"" + config.url + "sifreSifirla/" + user.reset_guid + "\">Şifrenizi sıfırlamak için tıklayınız.</a></div>";
                string baslik = "Şifre Sıfırlama Talebi";
                bool mailSonuc = EmailFunctions.sendEmailGmail(icerik, baslik, user.email, MailHedefTur.kullanici, user.id, EmailFunctions.mailAdresi, 0, "", "", "", "", -1);
                if (mailSonuc)
                {
                    sonuc = "Şifre sıfırlama maili mail adresinize gönderilmiştir.";
                    return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
                }
                sonuc = "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.";
                return Json(JsonSonuc.sonucUret(false, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult sifreSifirla(string id)
        {
            if (id == null || id.Equals(string.Empty))
            {
                Session["Uyari"] = "Sıfırlama bilgisi bulunamadı.";
                return RedirectToAction("Uyari", "Home");
            }
            kullanicilar usr = db.kullanicilar.Where(e => e.reset_guid.Equals(id) && e.flag == durumlar.aktif).FirstOrDefault();
            if (usr == null)
            {
                return RedirectToAction("Uyari", "Home");
            }
            else
            {
                if (usr.reset_guidexpiredate < DateTime.Now)
                {
                    Session["Uyari"] = "Şifre sıfırlama linkinizin tarihi geçmiş. Lütfen tekrar şifre sıfırlama isteği oluşturunuz.";
                    Guid gd = new Guid();
                    gd = Guid.NewGuid();
                    usr.reset_guid = gd.ToString();
                    db.Entry(usr).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Uyari", "Home");
                }
                else
                {
                    ViewBag.guid = id;
                    return View();
                }
            }
        }
        [HttpPost]
        public JsonResult sifreSifirla(string id, string pwd1, string pwd2)
        {
            try
            {
                if (!pwd1.Equals(pwd2))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz şifreler eşleşmiyor. Lütfen şifreleri kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                kullanicilar usr = db.kullanicilar.Where(e => e.reset_guid.Equals(id) && e.flag == durumlar.aktif).FirstOrDefault();
                if (usr == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Kullanıcı bulunamadı. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                usr.password = HashWithSha.ComputeHash(pwd1, "SHA512", Encoding.ASCII.GetBytes(pwd1));

                usr.reset_guid = "";

                db.Entry(usr).State = EntityState.Modified;
                db.SaveChanges();

                string icerik = "<div>Şifreniz sıfırlanmıştır.</div>";
                string baslik = "Şifre Sıfırlama Talebi";
                bool mailSonuc = EmailFunctions.sendEmailGmail(icerik, baslik, usr.email, MailHedefTur.kullanici, usr.id, EmailFunctions.mailAdresi, 0, "", "", "", "", -1);
                if (mailSonuc)
                {
                    return Json(JsonSonuc.sonucUret(true, "Şifreniz güncellenmiştir. Yeni şifrenizle giriş yapabilirsiniz."), JsonRequestBehavior.AllowGet);
                }
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult EmailOnay(string id)
        {
            if (id == null || id.Equals(string.Empty))
            {
                return RedirectToAction("Index");
            }
            kullanicilar usr = db.kullanicilar.Where(e => e.reset_guid.Equals(id)).FirstOrDefault();
            if (usr == null)
            {
                Session["Uyari"] = "Kullanıcı bulunamadı.";
            }
            else
            {
                if (usr.reset_guidexpiredate < DateTime.Now)
                {
                    Session["Uyari"] = "Onay linkinizin geçerlilik tarihi geçmiştir. E-mail adresinize yeni bir aktivasyon linki gönderilmiştir.";
                    Guid gd = new Guid();
                    gd = Guid.NewGuid();
                    usr.reset_guid = gd.ToString();
                    db.Entry(usr).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Session["Uyari"] = "E-mail adresiniz onaylanmıştır. Giriş yapabilirsiniz.";
                    usr.reset_guid = "";
                    usr.flag = durumlar.aktif;
                    db.Entry(usr).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Uyari");
        }
        public ActionResult Uyari()
        {
            if (Session["Uyari"] != null)
            {
                string uyr = Session["Uyari"].ToString();
                ViewBag.uyari = uyr;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        #endregion kullanıcı ve login işlemleri
        #region anasayfa işlemleri
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> MainPage()
        {
            /*List<string> phones = new List<string>();
            phones.Add("05064768590");
            SendSms sms = new SendSms();
            sms.SendSMS(phones.ToArray(), "Test Mesajı 123");*/

            List<object> nesneler = new List<object>();
            List<BugunModel> tumListe = new List<BugunModel>();
            List<int> projeSurecGorevList = new List<int>();
            List<kullanicilar> userList = new List<kullanicilar>();
            List<GorevVeProjeSurecOzetModel> projeSurecGorevList2 = new List<GorevVeProjeSurecOzetModel>();
            List<MusterilerModel> musteriList = new List<MusterilerModel>();
            try
            {
                string now = DateTime.Now.ToString("dd.MM.yyyy");
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string projeProjeBased = "select * from ( "
                    + "select ps.isim, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('proje/', ps.url)) as url, " + ProjeSurecTur.proje + " as tur "
                    + "from proje_surec as ps "
                    + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                    + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = " + durumlar.aktif + " "
                    + "and kp.kullanici_id = " + lgm.id.ToString() + " and ps.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y') "
                    + " union  "
                    + "select ps.isim, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('surec/', ps.url)) as url, " + ProjeSurecTur.surec + " as tur "
                    + "from proje_surec as ps "
                    + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                    + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = " + durumlar.aktif + " and kp.flag = " + durumlar.aktif + " and kp.kullanici_id = " + lgm.id.ToString() + " "
                    + "and ps.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y') "
                    + " union "
                    + "select g.isim, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('gorev/', g.url)) as url, 0 as tur "
                    + "from gorevler as g "
                    + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id and kg.flag = " + durumlar.aktif + " "
                    + "where g.flag = " + durumlar.aktif + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") and g.firma_id = " + lgm.firma_id.ToString() + " and g.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y')"
                    + ") as tbl order by bitis_tarihi limit 0,5";

                if (lgm.kullanici_turu < KullaniciTurleri.user)
                {
                    projeProjeBased = "select * from("
                    + "select ps.isim, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('proje/', ps.url)) as url, " + ProjeSurecTur.proje + " as tur "
                    + "from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = " + durumlar.aktif + " and ps.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y') "
                    + " union "
                    + "select ps.isim, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('surec/', ps.url)) as url, " + ProjeSurecTur.surec + " as tur "
                    + "from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = " + durumlar.aktif + " "
                    + "and ps.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y') "
                    + " union "
                    + "select g.isim, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d') as bitis_tarihi, (concat('gorev/', g.url)) as url, 0 as tur "
                    + "from gorevler as g "
                    + "where g.flag = " + durumlar.aktif + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") and g.firma_id = " + lgm.firma_id.ToString() + " and g.baslangic_tarihi <= STR_TO_DATE('" + now + "', '%d.%m.%Y')"
                    + ") as tbl order by bitis_tarihi limit 0,5";
                }

                var p1 = db.Database.SqlQuery<BugunModel>(projeProjeBased).ToListAsync();

                string projeSurecGorevCounts = "";
                if (lgm.kullanici_turu < KullaniciTurleri.user)
                {
                    projeSurecGorevCounts = "select count(ps.isim) as size from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif.ToString() + " and ps.tur = " + ProjeSurecTur.proje
                        + " union all "
                        + "select count(ps.isim) as size from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif.ToString() + " and ps.tur = " + ProjeSurecTur.surec
                        + " union all "
                        + "select count(g.isim) as size "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and g.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + "";
                }
                else
                {
                    projeSurecGorevCounts = "select count(ps.isim) as size from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif.ToString() + " and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id = " + lgm.id.ToString() + ""
                        + " union all "
                        + "select count(ps.isim) as size from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif.ToString() + " and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id = " + lgm.id.ToString() + ""
                        + " union all "
                        + "select count(g.isim) as size "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and g.flag = " + durumlar.aktif.ToString() + " and kg.flag = " + durumlar.aktif + " and g.firma_id = " + lgm.firma_id.ToString() + "";
                }

                var psgc = db.Database.SqlQuery<int>(projeSurecGorevCounts).ToListAsync();
                var usr = db.kullanicilar.Where(e => e.firma_id == lgm.firma_id && e.flag == durumlar.aktif).ToListAsync();

                #region anasayfa alt bölüm işlemleri
                string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ((select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select ps.id, 2 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select ps.id, 3 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + ") "
                        + "union "
                        + "(select g.id, 1 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + " and g.firma_id = " + lgm.firma_id.ToString() + " and gp.id is null) "
                        + "union "
                        + "(select g.id, 2 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and gp.id is null) "
                        + "union "
                        + "(select g.id, 3 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and gp.id is null) "
                        + "order by oncelik, bitis_tarihi) as tbl";
                string musteriQuery = "select * from musteriler where firma_id = " + lgm.firma_id.ToString();

                var psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToListAsync();
                var mstr = db.Database.SqlQuery<MusterilerModel>(musteriQuery).ToListAsync();
                #endregion anasayfa alt bölüm işlemleri

                await Task.WhenAll(p1, psgc, usr, psg, mstr);

                List<BugunModel> projeSurecGorev = p1.Result;
                tumListe.AddRange(projeSurecGorev);
                tumListe = tumListe.OrderBy(e => e.baslangic_tarihi).ToList();

                projeSurecGorevList = psgc.Result;

                userList = usr.Result;

                projeSurecGorevList2 = psg.Result;
                musteriList = mstr.Result;
            }
            catch (Exception e)
            {
                tumListe = new List<BugunModel>();
                projeSurecGorevList = new List<int>();
                userList = new List<kullanicilar>();
                projeSurecGorevList2 = new List<GorevVeProjeSurecOzetModel>();
                musteriList = new List<MusterilerModel>();
            }
            nesneler.Add(tumListe);
            nesneler.Add(projeSurecGorevList);
            nesneler.Add(userList);
            nesneler.Add(projeSurecGorevList2);
            nesneler.Add(musteriList);
            return View(nesneler);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult kullaniciProjeGorevleri(string kullanici_url)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ((select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select ps.id, 2 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select ps.id, 3 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "inner join kullanicilar as k on k.id = kp.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") "
                        + "and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and k.url = '" + kullanici_url + "') "
                        + "union "
                        + "(select g.id, 1 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ") "
                        + "and kg.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "union "
                        + "(select g.id, 2 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") and kg.flag = " + durumlar.aktif.ToString() + " "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "union "
                        + "(select g.id, 3 as oncelik, g.isim as isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum "
                        + "from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.gorev_id = g.id "
                        + "inner join kullanicilar as k on k.id = kg.kullanici_id and k.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") and kg.flag = " + durumlar.aktif.ToString() + " "
                        + "and g.firma_id = " + lgm.firma_id.ToString() + " and k.url = '" + kullanici_url + "' and gp.id is null) "
                        + "order by oncelik, bitis_tarihi) as tbl";
                List<GorevVeProjeSurecOzetModel> psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToList();
                return Json(JsonSonuc.sonucUret(true, psg), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcı görevleri getirilemedi. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }            
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult musteriProjeGorevleri(string musteriUrl)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string projeSurecGorevQuery = "select id, oncelik, isim, DATE_FORMAT(baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, yuzde, flag, tur, url, durum from ((select ps.id, 1 as oncelik, ps.isim, ps.baslangic_tarihi, ps.bitis_tarihi, ps.yuzde, ps.flag, ps.tur, ps.url, ps.durum from proje_surec as ps "
                    + "inner join proje_musteri as pm on ps.id = pm.proje_id and pm.flag = " + durumlar.aktif + " "
                    + "inner join musteriler as m on m.id = pm.musteri_id and m.flag = " + durumlar.aktif + " "
                    + "where ps.flag = " + durumlar.aktif + " and m.url = '" + musteriUrl + "') "
                    + "union "
                    + "(select g.id, 1 as oncelik, g.isim, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                    + "inner join gorev_musteri as gm on g.id = gm.gorev_id and gm.flag = " + durumlar.aktif + " "
                    + "inner join musteriler as m on m.id = gm.musteri_id and m.flag = " + durumlar.aktif + " "
                    + "where g.flag = " + durumlar.aktif + " and m.url = '" + musteriUrl + "')) as tbl";
                List<GorevVeProjeSurecOzetModel> psg = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(projeSurecGorevQuery).ToList();
                return Json(JsonSonuc.sonucUret(true, psg), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcı görevleri getirilemedi. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult anasayfaProjeGorevleriniGetir(string id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select g.id, 1 as oncelik, g.isim, DATE_FORMAT(g.baslangic_tarihi, '%d.%m.%Y') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%d.%m.%Y') as bitis_tarihi, g.yuzde, g.flag, 3 as tur, g.url, g.durum from gorevler as g "
                + "inner join gorev_proje as gp on gp.flag = " + durumlar.aktif + " and g.id = gp.gorev_id and gp.proje_id = " + id + " "
                + "where g.flag = " + durumlar.aktif + " order by g.isim";
            List<GorevVeProjeSurecOzetModel> mpList = db.Database.SqlQuery<GorevVeProjeSurecOzetModel>(pkQuery).ToList();
            return Json(JsonSonuc.sonucUret(true, mpList));
        }
        #endregion anasayfa işlemleri
        #region firma yönetimi
        [AreaAuthorize("Yonetici", "")]
        public ActionResult Firma()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            firma_musavir fm = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == lgm.firma_id).FirstOrDefault();
            if (fm == null)
            {
                return RedirectToAction("MainPage");
            }
            return View(fm);
        }
        [AreaAuthorize("Yonetici", "")]
        public JsonResult FirmaDuzenle(string firma_adi, string vergi_dairesi, string adres, string konum_periyot, string firma_mail, string mail_port, string mail_ssl, string mail_host, string mail_pass)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                if (lgm.fm == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Kullanıcı profilinize tanımlı bir firma bulunamamıştır."), JsonRequestBehavior.AllowGet);
                }
                if (!(lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin))
                {
                    return Json(JsonSonuc.sonucUret(false, "Bu işlemi yapma yetkiniz bulunmamaktadır."), JsonRequestBehavior.AllowGet);
                }
                firma_musavir dbFm = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.url.Equals(lgm.fm.url)).FirstOrDefault();
                dbFm.firma_adi = firma_adi;
                dbFm.vergi_dairesi = vergi_dairesi;
                dbFm.adres = adres;
                dbFm.konum_periyot = Convert.ToInt32(konum_periyot);
                dbFm.firma_mail = firma_mail;
                dbFm.mail_port = mail_port;
                dbFm.mail_ssl = mail_ssl;
                dbFm.mail_host = mail_host;
                dbFm.mail_pass = mail_pass;
                db.Entry(dbFm).State = EntityState.Modified;
                db.SaveChanges();
                return Json(JsonSonuc.sonucUret(true, "Firma bilgileriniz güncellenmiştir."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion firma yönetimi
        #region Kullanicilar
        [AreaAuthorize("Yetkili", "")]
        public ActionResult Kullanicilar()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            if (lgm.kullanici_turu != KullaniciTurleri.super_admin && lgm.kullanici_turu != KullaniciTurleri.firma_yetkili && lgm.kullanici_turu != KullaniciTurleri.firma_admin)
            {
                return RedirectToAction("MainPage");
            }
            int incelenecekKullaniciTurleri = lgm.kullanici_turu;
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin)
            {
                incelenecekKullaniciTurleri = 0;    
            }
            List<kullanicilar> kullaniciList = db.kullanicilar.Where(e => e.flag != durumlar.silindi && e.firma_id == lgm.firma_id && e.kullanici_turu > incelenecekKullaniciTurleri && e.id != lgm.id).OrderBy(e => (e.ad + "" + e.soyad)).ToList();
            return View(kullaniciList);
        }
        [AreaAuthorize("Yetkili", "")]
        public ActionResult Kullanici(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            if (lgm.kullanici_turu != KullaniciTurleri.super_admin && lgm.kullanici_turu != KullaniciTurleri.firma_yetkili && lgm.kullanici_turu != KullaniciTurleri.firma_admin)
            {
                return RedirectToAction("Kullanicilar");
            }
            int incelenecekKullaniciTurleri = lgm.kullanici_turu;
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin)
            {
                incelenecekKullaniciTurleri = 0;
            }
            kullanicilar usr = db.kullanicilar.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.firma_id == lgm.firma_id && e.kullanici_turu > incelenecekKullaniciTurleri).FirstOrDefault();
            if (usr == null)
            {
                usr = new kullanicilar();
            }
            
            return View(usr);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult KullaniciDuzenle(string url, string password, string password_control, string mail_permission, string sms_permission)
        {
            try
            {
                if (!password.Equals(password_control))
                {
                    return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                }

                int firma_id = Convert.ToInt32(Request["firma_id"].ToString());
                if (firma_id != GetCurrentUser.GetUser().firma_id)
                {
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                kullaniciIslemleri mic = new kullaniciIslemleri();
                string sonuc = mic.kullaniciDuzenle(url, password, password_control, mail_permission, sms_permission, Request);
                if (sonuc.Equals("") || sonuc.Equals("email_unique") || sonuc.Equals("kullanici_sayisi_hatasi") || sonuc.Equals("username_unique"))
                {
                    if (sonuc.Equals("email_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("username_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz kullanıcı adını başka bir kullanıcı kullanmaktadır. Lütfen farklı bir kullanıcı adı deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("kullanici_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Firmanıza başka kullanıcı eklenemez. Daha fazla kullanıcı ekleyebilmek için sistem yöneticimizle irtibata geçiniz."), JsonRequestBehavior.AllowGet);
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
                if (e.InnerException != null && e.InnerException.ToString().Contains("unique_email"))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult silKullanici(string id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                kullanicilar user = db.kullanicilar.Where(e => e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefault();
                if (user == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Kullanıcı bulunamadı."), JsonRequestBehavior.AllowGet);
                }
                user.flag = durumlar.silindi;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {                
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonSonuc.sonucUret(true, "Kullanıcı silindi."), JsonRequestBehavior.AllowGet);
        }
        #endregion Kullanicilar        
        #region kullanıcının kendi ayarları
        [AreaAuthorize("Kullanici", "")]
        public ActionResult ayarlar()
        {
            int currenId = GetCurrentUser.GetUser().id;
            kullanicilar user = db.kullanicilar.Where(e => e.id == currenId).FirstOrDefault();
            return View(user);
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult ayarlarKaydet(string mail_permission, string sms_permission)
        {
            try
            {
                kullaniciIslemleri mic = new kullaniciIslemleri();
                string sonuc = mic.kullaniciDuzenle(GetCurrentUser.GetUser().url, "", "", mail_permission, sms_permission, Request);
                if (sonuc.Equals("") || sonuc.Equals("email_unique") || sonuc.Equals("kullanici_sayisi_hatasi") || sonuc.Equals("username_unique"))
                {
                    if (sonuc.Equals("email_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("username_unique"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Girdiğiniz kullanıcı adını başka bir kullanıcı kullanmaktadır. Lütfen farklı bir kullanıcı adı deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("kullanici_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu firmaya başka kullanıcı eklenemez."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, "Bilgileriniz güncellenmiştir."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("unique_email"))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz e-Mail adresini başka bir kullanıcı kullanmaktadır. Lütfen farklı bir e-Mail adresi deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(false, "Bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
            }
        }
        [AreaAuthorize("Kullanici", "")]
        public ActionResult sifreAyarlari()
        {
            return View();
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult sifreAyarlari(string current_password, string password, string password_control)
        {
            try
            {
                if (!password.Equals(password_control))
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz şifreler eşleşmiyor."), JsonRequestBehavior.AllowGet);
                }
                string sifre = HashWithSha.ComputeHash(current_password, "SHA512", Encoding.ASCII.GetBytes(current_password));
                int id = GetCurrentUser.GetUser().id;
                kullanicilar dbUser = db.kullanicilar.Where(e => e.id == id && e.password == sifre && e.flag == durumlar.aktif).FirstOrDefault();
                if (dbUser == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Girdiğiniz Mevcut Şifre hatalıdır. Lütfen kontrol edip tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                if (!password.Equals(""))
                {
                    dbUser.password = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
                }
                db.Entry(dbUser).State = EntityState.Modified;
                db.SaveChanges();

                return Json(JsonSonuc.sonucUret(true, "Şifre değiştirildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion kullanıcının kendi ayarları
        #region Müşteri/Mükellef
        [AreaAuthorize("Kullanici", "")]
        public ActionResult Musteriler()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<MusterilerModel> musteriList = new List<MusterilerModel>();            

            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                string queryProjeBased = "select * from musteriler as m "
                + "inner join kullanici_musteri as km on km.musteri_id = m.id "
                + "where km.flag = 1 and m.flag = 1 and m.firma_id = " + lgm.firma_id + " and km.kullanici_id =  " + lgm.id.ToString() + " order by m.firma_adi;";
                musteriList = db.Database.SqlQuery<MusterilerModel>(queryProjeBased).ToList();
            }
            else
            {
                string queryProjeBased = "select * from musteriler as m "
                + "where m.flag = 1 and m.firma_id = " + lgm.firma_id + " order by m.firma_adi;";
                musteriList = db.Database.SqlQuery<MusterilerModel>(queryProjeBased).ToList();
            }

            return View(musteriList);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Musteri(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<object> nesneler = new List<object>();
            var m = db.musteriler.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefaultAsync();
            vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();

            await Task.WhenAll(m, kl);

            musteriler mstr = m.Result;
            if (mstr == null)
            {
                mstr = new musteriler();
            }

            List<kullanicilar> kullaniciList = kl.Result;

            nesneler.Add(mstr);
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin || lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(kullaniciList);
            }
            else
            {
                nesneler.Add(null);
            }
            return View(nesneler);
        }
        [AreaAuthorize("Yetkili", "Musteriler")]
        [HttpPost]
        public JsonResult MusteriDuzenle(string url)
        {
            try
            {
                musteriIslemleri mic = new musteriIslemleri();
                string sonuc = mic.musteriDuzenle(url, GetCurrentUser.GetUser().firma_id, Request);
                if (sonuc.Equals("") || sonuc.Equals("musteri_sayisi_hatasi"))
                {
                    if (sonuc.Equals("musteri_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Firmanıza başka müşteri/mükellef eklenemez. Daha fazla müşteri/mükellef ekleyebilmek için sistem yöneticimizle irtibata geçiniz."), JsonRequestBehavior.AllowGet);
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
        [AreaAuthorize("Yetkili", "Musteriler")]
        [HttpPost]
        public JsonResult silMusteri(string id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                musteriler mstr = db.musteriler.Where(e => e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefault();
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
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult musteriKullanicisiEkle()
        {
            try
            {
                musteriKullaniciIslemleri pki = new musteriKullaniciIslemleri();
                JsonSonuc sonuc = pki.yenimusteriKullanicisi(Request);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult musteriKullanicisiSil(int id)
        {
            try
            {
                musteriKullaniciIslemleri pki = new musteriKullaniciIslemleri();
                JsonSonuc sonuc = pki.musteriKullanicisiSil(id);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Kullanıcı Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult musteriKullanicilariGetir(int id)
        {
            try
            {
                List<KullaniciProjeOzetModel> ozetKullaniciList = musteriKullaniciIslemleri.getMusteriKullanicilarOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcılar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        #region excel işlemleri
        public ActionResult excelUpload(string sifirla)
        {
            List<musteriler> mList = new List<musteriler>();
            if (sifirla == null)
            {
                Session["liste"] = null;
            }
            else
            {
                mList = (List<musteriler>)Session["liste"];
                if (mList == null)
                {
                    mList = new List<musteriler>();
                }
            }
            return View(mList);
        }
        [HttpPost]
        public JsonResult ExcelUploadF(HttpPostedFileBase file)
        {
            try
            {
                string pathDosya = "~/public/upload/dosyalar/exc";
                if (!Directory.Exists(Server.MapPath(pathDosya)))
                {
                    Directory.CreateDirectory(Server.MapPath(pathDosya));
                }

                HttpFileCollectionBase hfc = Request.Files;

                string fullPathWithFileName = "";
                if (hfc.Count != 0)
                {
                    string ext = ".png";
                    HttpPostedFileBase hpf = hfc[0];

                    if (hpf.ContentLength > 0)
                    {

                        string fileName = "";
                        if (Request.Browser.Browser == "IE")
                        {
                            fileName = Path.GetFileName(hpf.FileName);
                        }
                        else
                        {
                            fileName = hpf.FileName;
                        }

                        string strFileName = StringFormatter.OnlyEnglishChar(fileName);

                        string createdFileName = strFileName;
                        fullPathWithFileName = pathDosya + "/" + createdFileName + ext;

                        hpf.SaveAs(Server.MapPath(fullPathWithFileName));
                    }
                    else
                    {
                        //return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                        return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    //return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }

                DataTable firmalarExcelTable = ExcelFunctions.ExcelToDataTable(Server.MapPath(@"" + fullPathWithFileName), "");
                int id = 0;
                List<musteriler> mList = new List<musteriler>();
                foreach (DataRow row in firmalarExcelTable.Rows)
                {
                    musteriler m = new musteriler();
                    m.firma_adi = row[0].ToString() + row[20].ToString();
                    m.adres = row[1].ToString();
                    m.vergi_dairesi = row[2].ToString() + "(" + row[4].ToString() + ")";
                    m.vergi_no = row[3].ToString();
                    m.id = id;
                    m.tel = row[8].ToString() + " " + row[9].ToString();
                    m.aciklama = "";
                    m.gsm = row[16].ToString();
                    m.email = row[21].ToString();
                    if (!row[5].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-TC : " + row[5].ToString();
                    }
                    if (!row[6].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-SGK No : " + row[6].ToString();
                    }
                    if (!row[7].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Bölge Çalışma No : " + row[7].ToString();
                    }
                    if (!row[18].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Şube Kodu : " + row[18].ToString();
                    }
                    if (!row[19].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Şube Adı : " + row[19].ToString();
                    }
                    if (!row[30].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Şahıs Ticaret Ünivanı : " + row[30].ToString();
                    }
                    if (!row[31].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Faaliyet Kodu : " + row[31].ToString();
                    }
                    if (!row[32].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Faaliyet Adı : " + row[32].ToString();
                    }
                    if (!row[35].ToString().Equals(string.Empty))
                    {
                        m.aciklama += "-Yapılan İşin Niteliği : " + row[35].ToString();
                    }

                    mList.Add(m);
                    id++;
                }
                Session["liste"] = mList;
                return Json(JsonSonuc.sonucUret(true, "Dosya Eklendi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz. Hata:" + e.Message), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult silTempFirma(int id)
        {
            try
            {
                List<musteriler> mList = (List<musteriler>)Session["liste"];
                musteriler m = mList.Where(e => e.id == id).FirstOrDefault();
                if (m != null)
                {
                    mList.Remove(m);
                }
                Session["liste"] = mList;
            }
            catch (Exception)
            {
                return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
            }
            return Json(FormReturnTypes.basarili, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult tempFirmalariKaydet()
        {
            try
            {
                List<musteriler> mList = (List<musteriler>)Session["liste"];
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                foreach (musteriler m in mList)
                {
                    int vid = 1;
                    if (db.musteriler.Count() != 0)
                    {
                        vid = db.musteriler.Max(e => e.vid) + 1;
                    }
                    int sort = 1;
                    if (db.musteriler.Count() != 0)
                    {
                        sort = db.musteriler.Max(e => e.sort) + 1;
                    }

                    string strImageName = StringFormatter.OnlyEnglishChar(m.firma_adi + "_" + vid.ToString());
                    string createdUrl = strImageName;
                    string tempUrl = createdUrl;
                    bool bulundu = false;
                    int i = 0;
                    musteriler pg = new musteriler();
                    do
                    {
                        pg = db.musteriler.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                    m.url = createdUrl;

                    m.flag = durumlar.aktif;
                    m.date = DateTime.Now;
                    m.vid = vid;
                    m.sort = sort;
                    m.ekleyen = lgm.id;
                    m.firma_id = lgm.firma_id;
                    m.ad = "";
                    m.soyad = "";
                    m.firma = "";

                    db.musteriler.Add(m);
                }

                db.SaveChanges();

                Session["liste"] = null;
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonSonuc.sonucUret(true, "Firmalar Kaydedildi"), JsonRequestBehavior.AllowGet);
        }
        #endregion excel işlemleri
        #endregion Müşteri/Mükellef
        #region Projeler
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Projeler()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<ProjeSurecModel> projeList = new List<ProjeSurecModel>();
            
            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                string queryProjeBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ") order by oncelik, bitis_tarihi";
                var pb = db.Database.SqlQuery<ProjeSurecModel>(queryProjeBased).ToListAsync();

                /*string queryGorevBased = "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    +" union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();*/

                //await Task.WhenAll(pb, gb);
                await Task.WhenAll(pb);

                //projeList = gb.Result;
                List<ProjeSurecModel> projeList2 = pb.Result;

                foreach (ProjeSurecModel psm in projeList2)
                {
                    ProjeSurecModel psmControl = projeList.Where(e => e.id == psm.id).FirstOrDefault();
                    if (psmControl == null)
                    {
                        projeList.Add(psm);
                    }
                }
            }
            else
            {
                string queryGorevBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1)"
                    + " union "
                    + "(select 2 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1)"
                    +" union "
                    + "(select 3 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.proje + " and ps.flag = 1) order by oncelik, bitis_tarihi";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();

                await Task.WhenAll(gb);

                projeList = gb.Result;
            }

            return View(projeList);

        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<JsonResult> ProjelerFiltre(int drmm, string bast, string bit, string order, string desc)//durum, tarih başlangıç, tarih bitiş
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<ProjeSurecModel> projeList = new List<ProjeSurecModel>();

            string whereStr = "";
            if (drmm != 0)
            {
                whereStr += " and ps.durum = " + drmm.ToString();
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and ps.baslangic_tarihi >= STR_TO_DATE('" + bast + "', '%Y-%m-%d')";
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and ps.baslangic_tarihi <= STR_TO_DATE('" + bit + "', '%Y-%m-%d')";
            }

            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                string queryProjeBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ") order by oncelik, " + order + " " + desc;
                var pb = db.Database.SqlQuery<ProjeSurecModel>(queryProjeBased).ToListAsync();

                /*string queryGorevBased = "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();*/

                //await Task.WhenAll(pb, gb);
                await Task.WhenAll(pb);

                //projeList = gb.Result;
                List<ProjeSurecModel> projeList2 = pb.Result;

                foreach (ProjeSurecModel psm in projeList2)
                {
                    ProjeSurecModel psmControl = projeList.Where(e => e.id == psm.id).FirstOrDefault();
                    if (psmControl == null)
                    {
                        projeList.Add(psm);
                    }
                }
            }
            else
            {
                string queryGorevBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and ps.flag = 1)"
                    + " union "
                    + "(select 2 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.flag = 1)"
                    +" union "
                    + "(select 3 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.proje + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.flag = 1) order by oncelik, " + order + " " + desc;
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();

                await Task.WhenAll(gb);

                projeList = gb.Result;
            }
            return Json(projeList, JsonRequestBehavior.AllowGet);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Proje(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();

            List<object> nesneler = new List<object>();

            var p = db.proje_surec.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.firma_id == lgm.firma_id && e.tur == ProjeSurecTur.proje).FirstOrDefaultAsync();
            vrlfgysdbEntities db2 = new vrlfgysdbEntities();
            var ml = db2.musteriler.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            if (id == null || id.Equals(string.Empty))
            {
                id = "";
            }
            var grvl = Gorevler(id);
            
            await Task.WhenAll(p, ml, kl, grvl);
            var prj = p.Result;
            ViewResult grvResult = (ViewResult)grvl.Result;
            List<GorevVeProjeOzetModel> gorevList = (List<GorevVeProjeOzetModel>)(grvResult.Model);
            
            List<musteriler> musteriList = ml.Result;
            if (prj == null)
            {
                prj = new proje_surec();
                prj.baslangic_tarihi = DateTime.Now;
                prj.bitis_tarihi = DateTime.Now.AddMonths(1);
            }

            List<kullanicilar> kullaniciList = kl.Result;

            nesneler.Add(prj);
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin || lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(musteriList);
            }
            else
            {
                nesneler.Add(null);
            }
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin|| lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(kullaniciList);
            }
            else
	        {
                nesneler.Add(null);
	        }
            nesneler.Add(gorevList);

            if (prj.id == 0)
            {
                Guid gd = new Guid();
                gd = Guid.NewGuid();
                ViewBag.tempGuid = gd.ToString();
            }

            return View(nesneler);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult ProjeDuzenle(string url, string tempGuid)
        {
            try
            {
                string sonuc = "";
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    sonuc = tempProjeSurecKaydet(Request, ProjeSurecTur.proje).Message.ToString();
                }
                else
                {
                    projeIslemleri mic = new projeIslemleri();
                    sonuc = mic.projeDuzenle(url, GetCurrentUser.GetUser().firma_id, Request);
                    //ViewBag.tempGuid = null;
                }
                
                if (sonuc.Equals("") || sonuc.Equals("proje_sayisi_hatasi") || sonuc.Equals("proje_isim_hatasi"))
                {
                    if (sonuc.Equals("proje_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Firmanıza başka Proje eklenemez. Daha fazla Proje ekleyebilmek için sistem yöneticimizle irtibata geçiniz."), JsonRequestBehavior.AllowGet);
                    }
                    else if (sonuc.Equals("proje_isim_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Bu isimde başka bir proje olduğundan proje kaydedilemedi."), JsonRequestBehavior.AllowGet);
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
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult silProje(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            projeIslemleri pis = new projeIslemleri();
            JsonSonuc sonuc = pis.silProje(id, lgm.firma_id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }
        #endregion Projeler
        #region Süreçler
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Surecler()
        {
            surecIslemleri sim = new surecIslemleri();
            sim.tekrarlananSurecKontrolu();

            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<ProjeSurecModel> surecList = new List<ProjeSurecModel>();

            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                string queryProjeBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ") order by oncelik, bitis_tarihi";
                var pb = db.Database.SqlQuery<ProjeSurecModel>(queryProjeBased).ToListAsync();

                /*string queryGorevBased = "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString() + " order by ps.bitis_tarihi desc)";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();*/

                //await Task.WhenAll(pb, gb);
                await Task.WhenAll(pb);

                //surecList = gb.Result;
                List<ProjeSurecModel> surecList2 = pb.Result;

                foreach (ProjeSurecModel psm in surecList2)
                {
                    ProjeSurecModel psmControl = surecList.Where(e => e.id == psm.id).FirstOrDefault();
                    if (psmControl == null)
                    {
                        surecList.Add(psm);
                    }
                }
            }
            else
            {
                string queryGorevBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 order by ps.bitis_tarihi)"
                    + " union "
                    + "(select 2 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 order by ps.bitis_tarihi)"
                    + " union "
                    + "(select 3 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " and (ps.durum = " + TamamlamaDurumlari.pasif + ") and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1 order by ps.bitis_tarihi) order by oncelik, bitis_tarihi";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();

                await Task.WhenAll(gb);

                surecList = gb.Result;
            }            

            return View(surecList);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<JsonResult> SureclerFiltre(int drmm, string bast, string bit, string order, string desc)//durum, tarih başlangıç, tarih bitiş
        {
            surecIslemleri sim = new surecIslemleri();
            sim.tekrarlananSurecKontrolu();

            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<ProjeSurecModel> surecList = new List<ProjeSurecModel>();

            string whereStr = "";
            if (drmm != 0)
            {
                whereStr += " and ps.durum = " + drmm.ToString();
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and ps.baslangic_tarihi >= STR_TO_DATE('" + bast + "', '%Y-%m-%d')";
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and ps.baslangic_tarihi <= STR_TO_DATE('" + bit + "', '%Y-%m-%d')";
            }

            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                string queryProjeBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, ps.* from proje_surec as ps "
                + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.flag = 1 and kp.flag = 1 and kp.kullanici_id =  " + lgm.id.ToString() + ") order by oncelik, " + order + " " + desc;
                var pb = db.Database.SqlQuery<ProjeSurecModel>(queryProjeBased).ToListAsync();

                /*string queryGorevBased = "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString()+ " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString()+ " order by ps.bitis_tarihi desc)"
                    + " union "
                    + "(select ps.* from proje_surec as ps "
                    + "inner join gorev_proje as gp on gp.proje_id = ps.id "
                    + "inner join kullanici_gorev as kg on kg.gorev_id = gp.gorev_id "
                    + "where ps.firma_id = " + lgm.firma_id + " and ps.tur = " + ProjeSurecTur.surec + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.flag = 1 and gp.flag = 1 and kg.flag = 1 and kg.kullanici_id = " + lgm.id.ToString()+ " order by ps.bitis_tarihi desc)";
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();*/

                //await Task.WhenAll(pb, gb);
                await Task.WhenAll(pb);

                //surecList = gb.Result;
                List<ProjeSurecModel> surecList2 = pb.Result;

                foreach (ProjeSurecModel psm in surecList2)
                {
                    ProjeSurecModel psmControl = surecList.Where(e => e.id == psm.id).FirstOrDefault();
                    if (psmControl == null)
                    {
                        surecList.Add(psm);
                    }
                }
            }
            else
            {
                string queryGorevBased = "(select 1 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.basladi + " or ps.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1)"
                    + " union "
                    + "(select 2 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1)"
                    + " union "
                    + "(select 3 as oncelik, ps.* from proje_surec as ps "
                    + "where ps.firma_id = " + lgm.firma_id + " " + (drmm != 0 ? "" : " and (ps.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and ps.tur = " + ProjeSurecTur.surec + " and ps.flag = 1) order by oncelik, " + order + " " + desc;
                var gb = db.Database.SqlQuery<ProjeSurecModel>(queryGorevBased).ToListAsync();

                await Task.WhenAll(gb);

                surecList = gb.Result;
            }

            return Json(surecList, JsonRequestBehavior.AllowGet);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Surec(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<object> nesneler = new List<object>();

            var s = db.proje_surec.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.firma_id == lgm.firma_id && e.tur == ProjeSurecTur.surec).FirstOrDefaultAsync();
            vrlfgysdbEntities db2 = new vrlfgysdbEntities();
            var ml = db2.musteriler.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            if (id == null || id.Equals(string.Empty))
            {
                id = "";
            }
            var grvl = Gorevler(id);

            await Task.WhenAll(s, ml, kl, grvl);

            List<musteriler> musteriList = ml.Result;
            proje_surec src = s.Result;
            if (src == null)
            {
                src = new proje_surec();
                src.baslangic_tarihi = DateTime.Now;
                src.bitis_tarihi = DateTime.Now.AddMonths(1);
            }

            List<kullanicilar> kullaniciList = kl.Result;

            ViewResult grvResult = (ViewResult)grvl.Result;
            List<GorevVeProjeOzetModel> gorevList = (List<GorevVeProjeOzetModel>)(grvResult.Model);

            nesneler.Add(src);
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin || lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(musteriList);
            }
            else
            {
                nesneler.Add(null);
            }
            if (lgm.kullanici_turu == KullaniciTurleri.super_admin || lgm.kullanici_turu == KullaniciTurleri.firma_admin || lgm.kullanici_turu == KullaniciTurleri.firma_yetkili)
            {
                nesneler.Add(kullaniciList);
            }
            else
            {
                nesneler.Add(null);
            }

            nesneler.Add(gorevList);

            if (src.id == 0)
            {
                Guid gd = new Guid();
                gd = Guid.NewGuid();
                ViewBag.tempGuid = gd.ToString();
            }

            return View(nesneler);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult SurecDuzenle(string url, string tempGuid)
        {
            try
            {
                string sonuc = "";
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    sonuc = tempProjeSurecKaydet(Request, ProjeSurecTur.surec).Message.ToString();
                }
                else
                {
                    surecIslemleri mic = new surecIslemleri();
                    sonuc = mic.surecDuzenle(url, GetCurrentUser.GetUser().firma_id, Request);
                    //ViewBag.tempGuid = null;
                }
                if (sonuc.Equals("") || sonuc.Equals("surec_sayisi_hatasi") || sonuc.Equals("surec_isim_hatasi"))
                {
                    if (sonuc.Equals("surec_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Firmanıza başka Süreç eklenemez. Daha fazla Süreç ekleyebilmek için sistem yöneticimizle irtibata geçiniz."), JsonRequestBehavior.AllowGet);
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
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult silSurec(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            surecIslemleri sis = new surecIslemleri();
            JsonSonuc sonuc = sis.silSurec(id, lgm.firma_id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
            /*try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                proje_surec prj = db.proje_surec.Where(e => e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefault();
                if (prj == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Süreç bulunamadı."), JsonRequestBehavior.AllowGet);
                }
                prj.flag = durumlar.silindi;
                db.Entry(prj).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonSonuc.sonucUret(true, "Süreç silindi."), JsonRequestBehavior.AllowGet);*/
        }
        #endregion Süreçler
        #region proje ve süreç ortak işlemleri
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecKullanicisiEkle()
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempYeniProjeKullanicisi(Request), JsonRequestBehavior.AllowGet);
                }
                projeKullanicisiIslemleri pki = new projeKullanicisiIslemleri();
                JsonSonuc sonuc = pki.yeniProjeKullanicisi(Request);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecKullanicisiSil(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempProjeKullaniciSil(id, tempGuid), JsonRequestBehavior.AllowGet);
                }
                projeKullanicisiIslemleri pki = new projeKullanicisiIslemleri();
                JsonSonuc sonuc = pki.projeKullanicisiSil(id);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Kullanıcı Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecKullanicilariGetir(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty) && !tempGuid.Contains("Gorev_"))
                {
                    return tempProjeKullanicilariGetir(tempGuid);
                }
                else if ((tempGuid.Equals(string.Empty) || tempGuid.Contains("Gorev_")) && id == 0)
                {
                    List<KullaniciProjeOzetModel> kOzetList = kullaniciIslemleri.getFirmaKullanicilariOzet();
                    JsonSonuc sonuc2 = JsonSonuc.sonucUret(true, kOzetList);
                    return Json(sonuc2, JsonRequestBehavior.AllowGet);
                }
                List<KullaniciProjeOzetModel> ozetKullaniciList = projeKullanicisiIslemleri.getProjectSurecKullanicilarOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcılar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecMusterisiEkle(string[] musteriList)
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempYeniProjeMusterisi(Request, musteriList), JsonRequestBehavior.AllowGet);
                }
                projeIslemleri pi = new projeIslemleri();
                JsonSonuc sonuc = pi.yeniProjeMusterisi(Request, musteriList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecMusterisiSil(string proje_id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempProjeMusterisiSil(proje_id, tempGuid), JsonRequestBehavior.AllowGet);
                }
                projeIslemleri pi = new projeIslemleri();
                JsonSonuc sonuc = pi.projeMusterisiSil(Convert.ToInt32(proje_id));
                if (sonuc.IsSuccess == true)
                {

                    sonuc = JsonSonuc.sonucUret(true, "Müşteri Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecMusteriPaylasimi(int id, string kullanici)
        {
            try
            {
                projeIslemleri pi = new projeIslemleri();
                JsonSonuc sonuc = pi.projeSurecMusterisiKullaniciGorevlendir(id, kullanici);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult projeSurecMusterileriGetir(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty) && !tempGuid.Contains("Gorev_"))
                {
                    return tempProjeMusterileriGetir(tempGuid);
                }
                else if ((tempGuid.Equals(string.Empty) || tempGuid.Contains("Gorev_")) && id == 0)
                {
                    List<MusteriProjeOzetModel> mOzetList = musteriIslemleri.getFirmaMusterilerOzet();
                    JsonSonuc sonuc2 = JsonSonuc.sonucUret(true, mOzetList);
                    return Json(sonuc2, JsonRequestBehavior.AllowGet);
                }
                List<MusteriProjeOzetModel> ozetKullaniciList = projeIslemleri.getProjectSurecMusterilerOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Müşteriler getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecTamamlandi(string id)
        {
            projeIslemleri pi = new projeIslemleri();
            JsonSonuc sonuc = pi.projeSurecTamamlandi(id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult projeSurecAktiflestir(string id)
        {
            projeIslemleri pi = new projeIslemleri();
            JsonSonuc sonuc = pi.projeSurecAktiflestir(id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }        
        #endregion proje ve süreç ortak işlemleri
        #region görevler
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Gorevler(string id)//id proje url yerine geçiyor
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string gorevQuery = "(select 1 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + " or g.durum = " + TamamlamaDurumlari.oncekiGorevBekleniyor + ") and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ")"
                + " union "
                + "(select 2 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ")"
                 + " union "
                + "(select 3 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ") order by oncelik, bitis_tarihi ";
            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                gorevQuery = "(select 1 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + " or g.durum = " + TamamlamaDurumlari.oncekiGorevBekleniyor + ") and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ")"
                + " union "
                + "(select 2 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ") and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ")" 
                + " union "
                + "(select 3 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " and (g.durum = " + TamamlamaDurumlari.pasif + ") and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + (id != null ? (" and ps.url = '" + id + "'") : ("")) + ") order by oncelik, bitis_tarihi ";
            }
            
            List<GorevVeProjeOzetModel> gorevList = db.Database.SqlQuery<GorevVeProjeOzetModel>(gorevQuery).ToList();
            return View(gorevList);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<JsonResult> GorevlerFiltre(int drmm, string bast, string bit, string order, string desc)//durum, tarih başlangıç, tarih bitiş
        {
            string whereStr = "";
            if (drmm != 0)
            {
                whereStr += " and g.durum = " + drmm.ToString();
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and g.baslangic_tarihi >= STR_TO_DATE('" + bast + "', '%Y-%m-%d')";
            }
            if (bast != null && !bast.Equals(string.Empty))
            {
                whereStr += " and g.baslangic_tarihi <= STR_TO_DATE('" + bit + "', '%Y-%m-%d')";
            }

            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string gorevQuery = "(select 1 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and g.firma_id = " + lgm.firma_id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and g.firma_id = " + lgm.firma_id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and g.firma_id = " + lgm.firma_id.ToString() + ") order by oncelik, " + order + " " + desc;
            if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
            {
                gorevQuery = "(select 1 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.basladi + " or g.durum = " + TamamlamaDurumlari.bekliyor + ")") + whereStr + " and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + ")"
                + " union "
                + "(select 2 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.tamamlandi + ")") + whereStr + " and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + ")"
                + " union "
                + "(select 3 as oncelik, g.isim as gorev_ismi, ps.isim as proje_ismi, g.baslangic_tarihi, g.bitis_tarihi, g.yuzde, g.flag as gorev_flag, ps.tur, g.url, g.durum "
                + "from gorevler as g "
                + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                + "where g.flag = " + durumlar.aktif.ToString() + " " + (drmm != 0 ? "" : " and (g.durum = " + TamamlamaDurumlari.pasif + ")") + whereStr + " and kg.flag = 1 and g.firma_id = " + lgm.firma_id.ToString() + ") order by oncelik, " + order + " " + desc;
            }

            List<GorevVeProjeOzetModel> gorevList = db.Database.SqlQuery<GorevVeProjeOzetModel>(gorevQuery).ToList();
            return Json(gorevList, JsonRequestBehavior.AllowGet);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Gorev(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();

            List<object> nesneler = new List<object>();

            var g = db.gorevler.Where(e => e.flag != durumlar.silindi && e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefaultAsync();
            vrlfgysdbEntities db2 = new vrlfgysdbEntities();
            var ml = db2.musteriler.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            vrlfgysdbEntities db3 = new vrlfgysdbEntities();
            var kl = db3.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToListAsync();
            vrlfgysdbEntities db4 = new vrlfgysdbEntities();
            var p = db4.proje_surec.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.tur == ProjeSurecTur.proje).ToListAsync();
            vrlfgysdbEntities db5 = new vrlfgysdbEntities();
            var s = db5.proje_surec.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.tur == ProjeSurecTur.surec).ToListAsync();            

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

            await Task.WhenAll(ytk, gp);

            kullanici_gorev kullaniciGorev = ytk.Result;
            gorev_proje gorevProje = gp.Result;

            if (lgm.kullanici_turu >= KullaniciTurleri.user && kullaniciGorev == null)
            {
                return RedirectToAction("Gorevler");
            }

            if (gorevProje == null)
            {
                gorevProje = new gorev_proje();
            }
            nesneler.Add(gorevProje);
            nesneler.Add(lgm);

            if (grv.id == 0)
            {
                Guid gd = new Guid();
                gd = Guid.NewGuid();
                ViewBag.tempGuid = "Gorev_" + gd.ToString();
                tempKontrol(lgm, "Gorev_" + gd.ToString(), Session);
            }

            return View(nesneler);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult GorevDuzenle(string url)
        {
            try
            {
                string tempGuid = Request["tempGuid"];                
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGoreviEkle(GetCurrentUser.GetUser(), Request, url), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri mic = new gorevIslemleri();
                string sonuc = mic.gorevDuzenle(url, GetCurrentUser.GetUser().firma_id, Request, Server);
                if (sonuc.Equals("") || sonuc.Equals("gorev_sayisi_hatasi"))
                {
                    if (sonuc.Equals("gorev_sayisi_hatasi"))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Firmanıza başka görev eklenemez. Daha fazla görev ekleyebilmek için sistem yöneticimizle irtibata geçiniz."), JsonRequestBehavior.AllowGet);
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
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult silGorev(string id, string tempGuid)
        {
            if (tempGuid != null && !tempGuid.Equals(string.Empty))
            {
                return Json(tempGorevSil(id, tempGuid), JsonRequestBehavior.AllowGet);
            }
            gorevIslemleri gis = new gorevIslemleri();
            JsonSonuc sonuc = gis.silGorev(id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
            /*try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                gorevler grv = db.gorevler.Where(e => e.url.Equals(id) && e.firma_id == lgm.firma_id).FirstOrDefault();
                if (grv == null)
                {
                    return Json(JsonSonuc.sonucUret(false, "Görev bulunamadı."), JsonRequestBehavior.AllowGet);
                }
                grv.flag = durumlar.silindi;
                db.Entry(grv).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
            return Json(JsonSonuc.sonucUret(true, "Görev silindi."), JsonRequestBehavior.AllowGet);*/
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult gorevTamamlandi(string id)
        {
            gorevIslemleri gis = new gorevIslemleri();
            JsonSonuc sonuc = gis.gorevTamamlandi(id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult goreviAktiflestir(string id)
        {
            gorevIslemleri gi = new gorevIslemleri();
            JsonSonuc sonuc = gi.gorevAktiflestir(id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        } 
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult gorevKullanicisiEkle()
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevKullaniciEkle(Request), JsonRequestBehavior.AllowGet);
                }
                gorevKullanicisiIslemleri pki = new gorevKullanicisiIslemleri();
                JsonSonuc sonuc = pki.yeniGorevKullanicisi(Request);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult gorevKullanicisiSil(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevKullaniciSil(id, tempGuid), JsonRequestBehavior.AllowGet);
                }
                gorevKullanicisiIslemleri pki = new gorevKullanicisiIslemleri();
                JsonSonuc sonuc = pki.gorevKullanicisiSil(id);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Kullanıcı Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevKullanicilariGetir(string id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return tempGorevKullanicilariGetir(tempGuid, id);
                }
                List<KullaniciProjeOzetModel> ozetKullaniciList = gorevKullanicisiIslemleri.getGorevKullanicilarOzet(Convert.ToInt32(id));
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Kullanıcılar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult gorevMusterisiEkle(string[] musteriList)
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevMusteriEkle(GetCurrentUser.GetUser(), Request, musteriList), JsonRequestBehavior.AllowGet);
                }

                gorevIslemleri gi = new gorevIslemleri();
                JsonSonuc sonuc = gi.yeniGorevMusterisi(Request, musteriList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult gorevMusterisiSil(string url, string gorev_url, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevMusterisiSil(url, gorev_url, tempGuid), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri pki = new gorevIslemleri();
                JsonSonuc sonuc = pki.gorevMusterisiSil(url, gorev_url);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Müşteri Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult gorevMusteriPaylasimi(int gorevid, string hedefkullanici, string kaynak_kullanici, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevMusterisKullaniciGorevlendir(gorevid, hedefkullanici, kaynak_kullanici, tempGuid), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri pki = new gorevIslemleri();
                JsonSonuc sonuc = pki.gorevMusterisKullaniciGorevlendir(gorevid, hedefkullanici, kaynak_kullanici);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevMusterileriGetir(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return tempGorevMusterileriGetir(tempGuid, id.ToString());
                }
                List<MusteriProjeOzetModel> ozetMusteriList = gorevIslemleri.getGorevMusterilerOzet(id);

                List<tempKullaniciProjeOzetModel> kpList = new List<tempKullaniciProjeOzetModel>();
                for (int i = 0; i < ozetMusteriList.Count; i++)
                {
                    bool yeniEklendi = false;

                    tempKullaniciProjeOzetModel tlpom = kpList.Where(e => e.kullaniciUrl == ozetMusteriList[i].kUrl).FirstOrDefault();

                    if (tlpom == null)
                    {
                        tlpom = new tempKullaniciProjeOzetModel();
                        tlpom.ad = ozetMusteriList[i].ad;
                        tlpom.soyad = ozetMusteriList[i].soyad;
                        tlpom.kullaniciUrl = ozetMusteriList[i].kUrl;
                        tlpom.kullaniciId = ozetMusteriList[i].kId;
                        yeniEklendi = true;
                    }

                    if (!tlpom.musteriIdList.Contains(ozetMusteriList[i].id))
                    {
                        tlpom.musteriIdList.Add(ozetMusteriList[i].id);
                    }
                    if (!tlpom.idList.Contains(ozetMusteriList[i].id))
                    {
                        tlpom.idList.Add(ozetMusteriList[i].id);
                    }

                    if (yeniEklendi)
                    {
                        kpList.Add(tlpom);
                    }                    
                }

                JsonSonuc sonuc = JsonSonuc.sonucUret(true, kpList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Müşteriler getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        #region görev dosyaları işlemleri
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevDosyasiEkle()
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempYeniGorevDosyasi(Request, Server), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri gi = new gorevIslemleri();
                JsonSonuc sonuc = gi.yeniGorevDosyasi(Request, Server);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevDosyasiSil(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevDosyaSil(id, tempGuid), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri gi = new gorevIslemleri();
                JsonSonuc sonuc = gi.gorevDosyasiSil(id);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Dosya Silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevDosyalariGetir(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return tempGorevDosyalariGetir(id.ToString(), tempGuid);
                }
                List<dosyaOzetModel> ozetDosyaList = gorevIslemleri.getGorevDosyalarOzet(id);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetDosyaList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Dosyalar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
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
        #endregion görev dosyaları işlemleri        

        #region görev bağlama işlemleri
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public JsonResult gorevBaglantiEkle()
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempYeniGorevBag(Request), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri gis = new gorevIslemleri();
                JsonSonuc sonuc = gis.yeniGorevBag(Request);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("yetkili", "")]
        [HttpPost]
        public JsonResult gorevBaglantiSil(int id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevBagSil(id, tempGuid), JsonRequestBehavior.AllowGet);
                }
                gorevIslemleri pki = new gorevIslemleri();
                JsonSonuc sonuc = pki.gorevBaglantisiSil(id);
                if (sonuc.IsSuccess == true)
                {
                    sonuc = JsonSonuc.sonucUret(true, "Görev bağı silindi.");
                }
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevBaglantiGetir(string id, string tempGuid)
        {
            try
            {
                if (id == null || id.Equals(string.Empty))
                {
                    id = "0";
                }
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return tempGorevBaglantilariGetir(tempGuid, id);
                }
                List<GorevBaglantiOzetModel> gorevBaglantiList = gorevIslemleri.getGorevBaglantilar(Convert.ToInt32(id));
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, gorevBaglantiList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev bağları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion görev bağlama işlemleri
        #endregion görevler
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult projeGorevleriniGetir(string id, string tempGuid)
        {
            try
            {
                if (id == null || id.Equals(string.Empty))
                {
                    id = "0";
                }
                if (tempGuid != null && !tempGuid.Equals(string.Empty) && !tempGuid.Contains("Gorev"))
                {
                    return tempGorevleriGetir(tempGuid);
                }
                List<GorevlerModel> ozetKullaniciList = projeIslemleri.getProjeGorevleri(Convert.ToInt32(id));
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, ozetKullaniciList);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev bağları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #region yapılacaklar
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult YapilacakDuzenle(string url)
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempYeniYapilacak(Request), JsonRequestBehavior.AllowGet);
                }
                yapilacakIslemleri mic = new yapilacakIslemleri();
                JsonSonuc sonuc = mic.yapilacakDuzenle(url, GetCurrentUser.GetUser().firma_id, Request);
                return Json(JsonSonuc.sonucUret(true, sonuc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult silYapilacak(string id, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                    return Json(tempGorevYapilacakSil(Convert.ToInt32(id), tempGuid), JsonRequestBehavior.AllowGet);
                }
                yapilacakIslemleri mic = new yapilacakIslemleri();
                JsonSonuc sonuc = mic.silYapilacak(id);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }            
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult yapilacaklarList(string gorev_url, string tempGuid)
        {
            try
            {
                if (tempGuid != null && !tempGuid.Equals(string.Empty))
                {
                      return tempGorevYapilacaklariGetir(gorev_url, tempGuid);
                }
                yapilacakIslemleri yis = new yapilacakIslemleri();
                JsonSonuc sonuc = yis.yapilacaklariGetir(gorev_url);
                return Json(sonuc, JsonRequestBehavior.AllowGet);;
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Yapılacaklar listesi getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult yapilacakIsaretle(string url, string value)
        {
            try
            {
                yapilacakIslemleri mic = new yapilacakIslemleri();
                int durum = YapilacaklarDurum.beklemede;
                if (value.Equals("true"))
                {
                    durum = YapilacaklarDurum.yapildi;
                }
                JsonSonuc sonuc = mic.yapilacakDurum(url, durum);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult gorevYuzdesiCek(string url)
        {
            try
            {
                string queryGorevCount = "select * from gorevler where flag = " + durumlar.aktif + " and url = '" + url + "'";
                GorevlerModel grv = db.Database.SqlQuery<GorevlerModel>(queryGorevCount).FirstOrDefault();
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, grv);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion yapılacaklar
        #region mesajlar
        int mesajSize = 20;
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Mesajlar()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string queryGorevCount = "select m.*, "
                + "(case when (m.alan_id = " + lgm.id + ") then k2.ad else k1.ad END) as ad, (case when (m.alan_id = " + lgm.id + ") then k2.soyad else k1.soyad END) as soyad "
                +"from (SELECT m1.* FROM mesajlar m1 LEFT JOIN mesajlar m2 ON (m1.parent_url = m2.parent_url AND m1.date < m2.date) WHERE m2.id IS NULL) as m "
                +"inner join kullanicilar as k1 on k1.id = m.alan_id "
                +"inner join kullanicilar as k2 on k2.id = m.gonderen_id "
                + "where m.flag != " + durumlar.silindi + " and m.firma_id = " + lgm.firma_id + " and (m.alan_id = " + lgm.id + " or m.gonderen_id = " + lgm.id + ") order by m.date desc;";
            var m = db.Database.SqlQuery<MesajlarDetayModel>(queryGorevCount).ToListAsync();

            await Task.WhenAll(m);

            List<MesajlarDetayModel> mesajList = m.Result;
            return View(mesajList);
        }
        [AreaAuthorize("Kullanici", "")]
        public async Task<ActionResult> Mesaj(string id)
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
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
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
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
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
        /*[AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult silMesaj(string id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            projeIslemleri pis = new projeIslemleri();
            JsonSonuc sonuc = pis.silProje(id, lgm.firma_id);
            return Json(sonuc, JsonRequestBehavior.AllowGet);
        }*/
        #endregion mesajlar
        #region takvim fonksiyonları
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public async Task<JsonResult> takvim()
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                /*
                1.projeler
                2.süreçler
                3.görevler
                */
                if (Tools.KullaniciTurleri.user <= lgm.kullanici_turu)
                {
                    string queryEndYokBased = "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, concat('proje/', ps.url) as "
                        + "url from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.proje.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and kp.kullanici_id = " + lgm.id.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union "
                        + "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, concat('surec/', ps.url) as url from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.surec.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and kp.kullanici_id = " + lgm.id.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union all "
                        + "select g.isim as title, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as start, concat('gorev/', g.url) as url from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and kg.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d')";
                    var ey = db.Database.SqlQuery<TakvimModel1>(queryEndYokBased).ToListAsync();

                    string queryEndVarBased = "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as end, concat('proje/', ps.url) as "
                        + "url from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.proje.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and kp.kullanici_id = " + lgm.id.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union "
                        + "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as end, concat('surec/', ps.url) as url from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.surec.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and kp.flag = " + durumlar.aktif.ToString() + " and kp.kullanici_id = " + lgm.id.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union all "
                        + "select g.isim as title, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d') as end, concat('gorev/', g.url) as url from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "inner join kullanici_gorev as kg on kg.kullanici_id = " + lgm.id.ToString() + " and kg.gorev_id = g.id "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and kg.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d')";
                    var ev = db.Database.SqlQuery<TakvimModel2>(queryEndVarBased).ToListAsync();

                    await Task.WhenAll(ev, ey);

                    List<TakvimModel1> takvimList = ey.Result;
                    List<TakvimModel2> takvimList2 = ev.Result;

                    foreach (TakvimModel2 tm2 in takvimList2)
                    {
                        takvimList.Add(tm2);
                    }

                    return Json(JsonSonuc.sonucUret(true, takvimList), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string queryEndYokBased = "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, concat('proje/', ps.url) as url from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.proje.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union "
                        + "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, concat('surec/', ps.url) as url from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.surec.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union all "
                        + "select g.isim as title, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as start, concat('gorev/', g.url) as url from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') = DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d')";
                    var ey = db.Database.SqlQuery<TakvimModel1>(queryEndYokBased).ToListAsync();

                    string queryEndVarBased = "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as end, concat('proje/', ps.url) as url from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.proje.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union "
                        + "select ps.isim as title, DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') as end, concat('surec/', ps.url) as url from proje_surec as ps "
                        + "where ps.firma_id = " + lgm.firma_id.ToString() + " and ps.tur = " + ProjeSurecTur.surec.ToString() + " and ps.flag = " + durumlar.aktif.ToString() + " and DATE_FORMAT(ps.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(ps.bitis_tarihi, '%Y-%m-%d') "
                        + "union all "
                        + "select g.isim as title, DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') as start, DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d') as end, concat('gorev/', g.url) as url from gorevler as g "
                        + "left join gorev_proje as gp on gp.gorev_id = g.id and gp.flag = " + durumlar.aktif.ToString() + " "
                        + "left join proje_surec as ps on ps.id = gp.proje_id and ps.flag = " + durumlar.aktif.ToString() + " "
                        + "where g.flag = " + durumlar.aktif.ToString() + " and g.firma_id = " + lgm.firma_id.ToString() + " and DATE_FORMAT(g.baslangic_tarihi, '%Y-%m-%d') != DATE_FORMAT(g.bitis_tarihi, '%Y-%m-%d')";
                    var ev = db.Database.SqlQuery<TakvimModel2>(queryEndVarBased).ToListAsync();

                    await Task.WhenAll(ev, ey);

                    List<TakvimModel1> takvimList = ey.Result;
                    List<TakvimModel2> takvimList2 = ev.Result;

                    foreach (TakvimModel2 tm2 in takvimList2)
                    {
                        takvimList.Add(tm2);
                    }

                    return Json(JsonSonuc.sonucUret(true, takvimList), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion takvim fonksiyonları
        #region bildirim fonksiyonları
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public async Task<JsonResult> bildirimlerim(int from)
        {
            //-kaç tane bildirim varsa onu yolla
            try
            {
                vrlfgysdbEntities db2 = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string queryBildirimler = "select vid, ilgili_url, mesaj, DATE_FORMAT(date, '%d.%m.%Y %H:%i') as date, okundu from sistem_bildirimleri where flag = " + durumlar.aktif.ToString() + " and kullanici_id = " + lgm.id.ToString() + " order by date limit " + from.ToString() + ",5";
                var bldrm = db2.Database.SqlQuery<BildirimOzetModel>(queryBildirimler).ToListAsync();

                await Task.WhenAll(bldrm);

                List<BildirimOzetModel> bildirimList = bldrm.Result;

                return Json(JsonSonuc.sonucUret(true, bildirimList), JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public async Task<JsonResult> okundu(int vid)
        {
            //-bildirimin vid yolla
            return Json(bildirimIslemleri.bildirimOkundu(vid), JsonRequestBehavior.AllowGet);
        }
        #endregion bildirim fonksiyonları
        #region harita fonksiyonları
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public async Task<JsonResult> harita()
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string queryHaritalar = "select stTable.id, stTable.latitude, stTable.longitude, stTable.date, k.ad, k.soyad, k.url from "
                    +"(SELECT st.id, st.latitude, st.longitude, DATE_FORMAT(st.date, '%d.%m.%Y %H:%i') as date, st.kullanici_id "
                    +"FROM saha_takip as st "
                    +"LEFT JOIN saha_takip m2 "
                    +"ON (st.kullanici_id = m2.kullanici_id AND st.id < m2.id) "
                    +"WHERE m2.id IS NULL and st.flag = " + durumlar.aktif + ") as stTable "
                    +"inner join kullanicilar as k on k.id = stTable.kullanici_id "
                    + "where k.flag = " + durumlar.aktif + " and k.firma_id = " + lgm.firma_id;
                var hrt = db.Database.SqlQuery<HaritaOzetModel>(queryHaritalar).ToListAsync();

                await Task.WhenAll(hrt);

                List<HaritaOzetModel> haritaList = hrt.Result;

                return Json(JsonSonuc.sonucUret(true, haritaList), JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yetkili", "")]
        [HttpPost]
        public async Task<JsonResult> kullaniciRota(string userUrl, string baslangic, string bitis)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                if (lgm.kullanici_turu >= KullaniciTurleri.user)
                {
                    userUrl = lgm.url;
                }
                /*string queryHaritalar = "SELECT st.id, st.latitude, st.longitude, DATE_FORMAT(st.date, '%d.%m.%Y %H:%i') as date, st.kullanici_id "
                    + "FROM saha_takip as st "
                    + "inner join kullanicilar as k on k.id = st.kullanici_id "
                    + "where k.flag = " + durumlar.aktif + " and st.flag = " + durumlar.aktif + " and k.firma_id = " + +lgm.firma_id + " and k.url = '" + userUrl + "' and st.date >= STR_TO_DATE('" + baslangic + "', '%Y-%m-%d') and st.date <= STR_TO_DATE('" + bitis + " 23:59', '%Y-%m-%d %H:%i') order by date desc;";*/
                string queryHaritalar = "SELECT st.latitude, st.longitude "
                    + "FROM saha_takip as st "
                    + "inner join kullanicilar as k on k.id = st.kullanici_id "
                    + "where k.flag = " + durumlar.aktif + " and st.flag = " + durumlar.aktif + " and k.firma_id = " + +lgm.firma_id + " and k.url = '" + userUrl + "' and st.date >= STR_TO_DATE('" + baslangic + "', '%Y-%m-%d') and st.date <= STR_TO_DATE('" + bitis + " 23:59', '%Y-%m-%d %H:%i') order by st.date desc;";
                var hrt = db.Database.SqlQuery<RotaEnBoyModel>(queryHaritalar).ToListAsync();

                await Task.WhenAll(hrt);

                List<RotaEnBoyModel> haritaList = hrt.Result;
                var jsonSerialiser = new JavaScriptSerializer();
                string json = jsonSerialiser.Serialize(haritaList);
                JsonSonuc sonuc = JsonSonuc.sonucUret(true, json);
                sonuc.Message = sonuc.Message.ToString().Replace("\"latitude\":", "");
                sonuc.Message = sonuc.Message.ToString().Replace("\"longitude\":", "");
                sonuc.Message = sonuc.Message.ToString().Replace("{", "[");
                sonuc.Message = sonuc.Message.ToString().Replace("}", "]");
                //sonuc.Message = sonuc.Message.ToString().Remove(0, 1);
                //sonuc.Message = sonuc.Message.ToString().Remove(sonuc.Message.ToString().Length - 1, 1);
                return Json(sonuc, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion harita fonksiyonları
        #region Raporlar
        [AreaAuthorize("Yetkili", "")]
        public async Task<ActionResult> raporlar()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<kullanicilar> kullaniciList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToList();
            return View(kullaniciList);
        }
        [AreaAuthorize("Yetkili", "")]
        public async Task<JsonResult> turRapor(int raporturu, string baslangic, string bitis, int periyot, int gorev, int surec, int proje, int bekleyen, int devameden, int tamamlanan, int pasif, int tarihTur, string kullanicilarurl)
        {
            try
            {
                #region durumquerytext
                string durumGorevStr = "(";
                string durumProjeSurecStr = "(";
                string or = " or ";
                if (bekleyen == 1)
                {
                    durumGorevStr += "g.durum = " + TamamlamaDurumlari.bekliyor;
                    durumProjeSurecStr += "ps.durum = " + TamamlamaDurumlari.bekliyor;
                }
                if (devameden == 1)
                {
                    if (!durumGorevStr.Equals("("))
                    {
                        durumGorevStr += or;
                        durumProjeSurecStr += or;
                    }
                    durumGorevStr += " g.durum = " + TamamlamaDurumlari.basladi;
                    durumProjeSurecStr += "ps.durum = " + TamamlamaDurumlari.basladi;
                }
                if (tamamlanan == 1)
                {
                    if (!durumGorevStr.Equals("("))
                    {
                        durumGorevStr += or;
                        durumProjeSurecStr += or;
                    }
                    durumGorevStr += " g.durum = " + TamamlamaDurumlari.tamamlandi;
                    durumProjeSurecStr += "ps.durum = " + TamamlamaDurumlari.tamamlandi;
                }
                if (pasif == 1)
                {
                    if (!durumGorevStr.Equals("("))
                    {
                        durumGorevStr += or;
                        durumProjeSurecStr += or;
                    }
                    durumGorevStr += " g.durum = " + TamamlamaDurumlari.pasif;
                    durumProjeSurecStr += "ps.durum = " + TamamlamaDurumlari.pasif;
                }
                if (durumGorevStr.Equals("("))
                {
                    durumGorevStr = "(g.durum = 100)";
                    durumProjeSurecStr = "(ps.durum = 100)";
                }
                else
                {
                    durumGorevStr += ")";
                    durumProjeSurecStr += ")";
                }
                #endregion durumquerytext

                #region tarih aralığı
                string baslangicTarihi = "";
                string bitisTarihi = "";
                if (periyot == raporPeriyotlari.yillik)
                {
                    baslangicTarihi = DateTime.Now.AddYears(-1).ToString("dd-MM-yyyy");
                    bitisTarihi = DateTime.Now.ToString("dd-MM-yyyy");
                }
                else if (periyot == raporPeriyotlari.aylik)
                {
                    baslangicTarihi = DateTime.Now.AddMonths(-1).ToString("dd-MM-yyyy");
                    bitisTarihi = DateTime.Now.ToString("dd-MM-yyyy");
                }
                else if (periyot == raporPeriyotlari.haftalik)
                {
                    baslangicTarihi = DateTime.Now.AddDays(-7).ToString("dd-MM-yyyy");
                    bitisTarihi = DateTime.Now.ToString("dd-MM-yyyy");
                }
                else
                {
                    baslangicTarihi = DateTime.Parse(baslangic).ToString("dd-MM-yyyy");
                    bitisTarihi = DateTime.Parse(bitis).ToString("dd-MM-yyyy");
                }
                #endregion tarih aralığı

                #region tarih query text
                string gorevTarihStr = "";
                string projeTarihStr = "";
                string groupBy = "";
                if (tarihTur == raporTarihTur.baslangic)
                {
                    gorevTarihStr += " and g.baslangic_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') ";
                    projeTarihStr += " and ps.baslangic_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') ";
                    groupBy = "baslangic_tarihi";
                }
                else if (tarihTur == raporTarihTur.bitis)
                {
                    gorevTarihStr += " and g.bitis_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') ";
                    projeTarihStr += " and ps.bitis_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') ";
                    groupBy = "bitis_tarihi";
                }
                else if (tarihTur == raporTarihTur.tamamlama)
                {
                    gorevTarihStr += " and g.tamamlanma_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') and g.durum = " + TamamlamaDurumlari.tamamlandi;
                    projeTarihStr += " and ps.tamamlanma_tarihi between STR_TO_DATE('" + baslangicTarihi + "', '%d-%m-%Y') and STR_TO_DATE('" + bitisTarihi + " 23:59', '%d-%m-%Y %H:%i') and ps.durum = " + TamamlamaDurumlari.tamamlandi;
                    groupBy = "tamamlanma_tarihi";
                }
                #endregion tarih query text

                LoggedUserModel lgm = GetCurrentUser.GetUser();
                DataModel data = new DataModel();
                string queryRapor = "SET lc_time_names='tr_TR';";
                if (raporturu == 0)
                {
                    #region tarihlere göre  
                    DatasetModel grvDataset = new DatasetModel();
                        grvDataset.label = "Görev";
                        grvDataset.borderColor = raporTurleri.gorevRenk;
                        DatasetModel prjDataset = new DatasetModel();
                        prjDataset.label = "Proje";
                        prjDataset.borderColor = raporTurleri.projeRenk;
                        DatasetModel srcDataset = new DatasetModel();
                        srcDataset.label = "Süreç";
                        srcDataset.borderColor = raporTurleri.surecRenk;
                    if (periyot == raporPeriyotlari.yillik)
                    {
                        #region yıllık
                        string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%b-%Y\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id.ToString() + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%m-%Y\");";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and " + durumProjeSurecStr + " and ps.tur = " + ProjeSurecTur.surec + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\");";
                        }
                        if (proje == 1)
                        {
                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and " + durumProjeSurecStr + " and ps.tur = " + ProjeSurecTur.proje + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporTurlereGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporTurlereGoreModel> gyList = gyl.Result;
                        List<RaporTurlereGoreModel> srcList = sl.Result;
                        List<RaporTurlereGoreModel> prjList = pl.Result;
                        DateTime now = DateTime.Now;
                        for (int i = 11; i >= 0; i--)
                        {
                            string ayStr = now.AddMonths(-i).ToString("MM-yyyy");
                            string ayStrText = now.AddMonths(-i).ToString("MMM-yyyy");
                            RaporTurlereGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporTurlereGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporTurlereGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporTurlereGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporTurlereGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporTurlereGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }
                        #endregion yıllık
                    }
                    else if (periyot == raporPeriyotlari.aylik)
                    {
                        #region aylık
                        string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%b-%Y\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%V-%m-%x\");";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\");";
                        }
                        if (proje == 1)
                        {
                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporTurlereGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporTurlereGoreModel> gyList = gyl.Result;
                        List<RaporTurlereGoreModel> srcList = sl.Result;
                        List<RaporTurlereGoreModel> prjList = pl.Result;

                        DateTime now = DateTime.Now;
                        for (int i = 3; i >= 0; i--)
                        {
                            int weekOfYear = OurFunctions.WeeksInYear(now.AddDays(-(i * 7))) - 1;
                            string ayStr = weekOfYear + "-" + now.AddDays(-(i * 7)).ToString("MM-yyyy");
                            string ayStrText = (i + 1).ToString() + " Hafta Önce";
                            RaporTurlereGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporTurlereGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporTurlereGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporTurlereGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporTurlereGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporTurlereGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }
                        #endregion aylık
                    }
                    else if (periyot == raporPeriyotlari.haftalik)
                    {
                        #region haftalık
                        string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%a\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (proje == 1)
                        {

                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporTurlereGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporTurlereGoreModel> gyList = gyl.Result;
                        List<RaporTurlereGoreModel> srcList = sl.Result;
                        List<RaporTurlereGoreModel> prjList = pl.Result;

                        DateTime now = DateTime.Now;
                        for (int i = 6; i >= 0; i--)
                        {
                            string ayStr = now.AddDays(-i).ToString("dd-MM-yyyy");
                            string ayStrText = (i + 1).ToString() + " Gün Önce";
                            RaporTurlereGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporTurlereGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporTurlereGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporTurlereGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporTurlereGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporTurlereGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }
                        #endregion haftalık
                    }
                    else
                    {
                        #region custom
                        string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;                        
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%a\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (proje == 1)
                        {

                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporTurlereGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporTurlereGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporTurlereGoreModel> gyList = gyl.Result;
                        List<RaporTurlereGoreModel> srcList = sl.Result;
                        List<RaporTurlereGoreModel> prjList = pl.Result;

                        DateTime now = DateTime.Parse(bitis);                        
                        int toplamGun = (int)(DateTime.Parse(bitis) - DateTime.Parse(baslangic)).TotalDays;
                        for (int i = toplamGun; i >= 0; i--)
                        {
                            string ayStr = now.AddDays(-i).ToString("dd-MM-yyyy");
                            string ayStrText = ayStr;
                            RaporTurlereGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporTurlereGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporTurlereGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporTurlereGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporTurlereGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporTurlereGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }
                        #endregion custom
                    }

                    if (gorev == 1)
                    {
                        data.datasets.Add(grvDataset);
                    }
                    if (surec == 1)
                    {
                        data.datasets.Add(srcDataset);
                    }
                    if (proje == 1)
                    {
                        data.datasets.Add(prjDataset);
                    }

                    /*var jsonSerialiser = new JavaScriptSerializer();
                    string json = jsonSerialiser.Serialize(data);
                    JsonSonuc sonuc = JsonSonuc.sonucUret(true, json);
                    return Json(sonuc, JsonRequestBehavior.AllowGet);*/

                    var jsonSerialiser = new JavaScriptSerializer();
                    string json = jsonSerialiser.Serialize(data);

                    RaporDonusModel rdm = new RaporDonusModel();
                    rdm.json = json;
                    rdm.liste = new List<object>();

                    JsonSonuc sonuc = JsonSonuc.sonucUret(true, rdm);

                    return Json(sonuc, JsonRequestBehavior.AllowGet);
                    #endregion tarihlere göre
                }
                else if (raporturu == 1)
                {
                    #region kullanicilara göre

                    #region kullanici text parçala
                    List<string> kullaniciList = kullanicilarurl.Split(';').ToList();
                    if (kullaniciList.Count == 0 || kullaniciList[0].Equals(string.Empty))
                    {
                        return Json(JsonSonuc.sonucUret(false, "Kullanıcı bulunamadı."), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        kullaniciList.RemoveAt(kullaniciList.Count - 1);
                    }
                    string userUrls = "(";
                    foreach (string usrUrl in kullaniciList)
                    {
                        if (userUrls.Equals("("))
                        {
                            userUrls += "k.url = '" + usrUrl + "'";
                        }
                        else
                        {
                            userUrls += " or k.url = '" + usrUrl + "'";
                        }
                    }
                    userUrls += ")";
                    #endregion kullanici text parçala

                    string queryGorev = queryRapor;
                    string querySurec = queryRapor;
                    string queryProje = queryRapor;

                    /*string queryGorevDetay = "select g.isim, g.yuzde, 'Görev' as tur, DATE_FORMAT(g.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, g.flag, g.durum, concat('gorev/',g.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad from gorevler as g inner join kullanicilar as k on k.id = -1 where g.id = -1";
                    string querySurecDetay = "select ps.isim, ps.yuzde, 'Süreç' as tur, DATE_FORMAT(ps.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, ps.flag, ps.durum, concat('surec/',ps.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad from proje_surec as ps inner join kullanicilar as k on k.id = -1 where ps.id = -1";
                    string queryProjeDetay = "select ps.isim, ps.yuzde, 'Süreç' as tur, DATE_FORMAT(ps.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, ps.flag, ps.durum, concat('surec/',ps.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad from proje_surec as ps inner join kullanicilar as k on k.id = -1 where ps.id = -1";*/
                    string detayQuery = "";
                    int toplamGun = (int)(DateTime.Parse(bitis) - DateTime.Parse(baslangic)).TotalDays;
                    if (gorev == 1)
                    {
                        queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(g." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                        + "from gorevler as g "
                        + "inner join kullanici_gorev as kp on kp.gorev_id = g.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by k.url;";

                        detayQuery = "select g.isim, g.yuzde, 'Görev' as tur, DATE_FORMAT(g.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(g.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, g.flag, g.durum, concat('gorev/',g.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad "
                        + "from gorevler as g "
                        + "inner join kullanici_gorev as kp on kp.gorev_id = g.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + "";
                    }
                    if (surec == 1)
                    {
                        querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(ps." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                        + "from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by k.url; ";

                        if (detayQuery != string.Empty)
                        {
                            detayQuery += " union ";
                        }
                        detayQuery += " select ps.isim, ps.yuzde, 'Süreç' as tur, DATE_FORMAT(ps.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, ps.flag, ps.durum, concat('surec/',ps.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad "
                        + "from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " ";
                    }
                    if (proje == 1)
                    {

                        queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(ps." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                        + "from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by k.url;";

                        if (detayQuery != string.Empty)
                        {
                            detayQuery += " union ";
                        }
                        detayQuery += "select ps.isim, ps.yuzde, 'Proje' as tur, DATE_FORMAT(ps.baslangic_tarihi, '%d-%m-%x') as baslangic_tarihi, DATE_FORMAT(ps.bitis_tarihi, '%d-%m-%x') as bitis_tarihi, ps.flag, ps.durum, concat('proje/',ps.url) as url, concat(k.ad, ' ', k.soyad) as ad_soyad "
                        + "from proje_surec as ps "
                        + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                        + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                        + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + "";
                    }
                    var gyl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryGorev).ToListAsync();
                    var sl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(querySurec).ToListAsync();
                    var pl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryProje).ToListAsync();

                    var gylD = db.Database.SqlQuery<RaporListeleriModel>(detayQuery).ToListAsync();
                    /*var slD = db.Database.SqlQuery<RaporListeleriModel>(querySurecDetay).ToListAsync();
                    var plD = db.Database.SqlQuery<RaporListeleriModel>(queryProjeDetay).ToListAsync();*/

                    //await Task.WhenAll(gyl, sl, pl, gylD, slD, plD);
                    await Task.WhenAll(gyl, sl, pl, gylD);

                    List<RaporKullanicilaraGoreModel> gyList = gyl.Result;
                    List<RaporKullanicilaraGoreModel> srcList = sl.Result;
                    List<RaporKullanicilaraGoreModel> prjList = pl.Result;

                    List<RaporListeleriModel> gyListDetay = gylD.Result;
                    /*List<RaporListeleriModel> srcListDetay = slD.Result;
                    List<RaporListeleriModel> prjListDetay = plD.Result;*/

                    GroupedBarChartDataset grvDataset = new GroupedBarChartDataset();
                    grvDataset.label = "Görev";
                    grvDataset.backgroundColor = raporTurleri.gorevRenk;
                    GroupedBarChartDataset prjDataset = new GroupedBarChartDataset();
                    prjDataset.label = "Proje";
                    prjDataset.backgroundColor = raporTurleri.projeRenk;
                    GroupedBarChartDataset srcDataset = new GroupedBarChartDataset();
                    srcDataset.label = "Süreç";
                    srcDataset.backgroundColor = raporTurleri.surecRenk;

                    for (int i = 0; i < kullaniciList.Count; i++)
                    {
                        string kullaniciUrl = kullaniciList[i];
                        RaporKullanicilaraGoreModel grvVarMi = gyList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                        if (grvVarMi == null)
                        {
                            grvVarMi = new RaporKullanicilaraGoreModel();
                            grvVarMi.adet = 0;
                            grvVarMi.url = kullaniciUrl;
                        }
                        grvDataset.data.Add(grvVarMi.adet);

                        RaporKullanicilaraGoreModel srcVarMi = srcList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                        if (srcVarMi == null)
                        {
                            srcVarMi = new RaporKullanicilaraGoreModel();
                            srcVarMi.adet = 0;
                            srcVarMi.url = kullaniciUrl;
                        }
                        srcDataset.data.Add(srcVarMi.adet);

                        RaporKullanicilaraGoreModel prjVarMi = prjList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                        if (prjVarMi == null)
                        {
                            prjVarMi = new RaporKullanicilaraGoreModel();
                            prjVarMi.adet = 0;
                            prjVarMi.url = kullaniciUrl;
                        }
                        prjDataset.data.Add(prjVarMi.adet);

                        data.labels.Add(kullaniciUrl);
                    }

                    /*if (periyot == raporPeriyotlari.yillik)
                    {*/
                        #region yıllık
                        /*string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%b-%Y\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id.ToString() + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%m-%Y\");";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and " + durumProjeSurecStr + " and ps.tur = " + ProjeSurecTur.surec + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\");";
                        }
                        if (proje == 1)
                        {
                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and " + durumProjeSurecStr + " and ps.tur = " + ProjeSurecTur.proje + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%m-%Y\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporKullanicilaraGoreModel> gyList = gyl.Result;
                        List<RaporKullanicilaraGoreModel> srcList = sl.Result;
                        List<RaporKullanicilaraGoreModel> prjList = pl.Result;

                        DatasetModel grvDataset = new DatasetModel();
                        grvDataset.label = "Görev";
                        grvDataset.borderColor = raporTurleri.gorevRenk;
                        DatasetModel prjDataset = new DatasetModel();
                        prjDataset.label = "Proje";
                        prjDataset.borderColor = raporTurleri.projeRenk;
                        DatasetModel srcDataset = new DatasetModel();
                        srcDataset.label = "Süreç";
                        srcDataset.borderColor = raporTurleri.surecRenk;*/
                        /*for (int i = 11; i >= 0; i--)
                        {
                            string ayStr = now.AddMonths(-i).ToString("MM-yyyy");
                            string ayStrText = now.AddMonths(-i).ToString("MMM-yyyy");
                            RaporKullanicilaraGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporKullanicilaraGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporKullanicilaraGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporKullanicilaraGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporKullanicilaraGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporKullanicilaraGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }*/
                        /*if (gorev == 1)
                        {
                            data.datasets.Add(grvDataset);
                        }
                        if (surec == 1)
                        {
                            data.datasets.Add(srcDataset);
                        }
                        if (proje == 1)
                        {
                            data.datasets.Add(prjDataset);
                        }*/
                        #endregion yıllık
                    /*}
                    else if (periyot == raporPeriyotlari.aylik)
                    {*/
                        #region aylık
                        /*string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%b-%Y\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + projeTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%V-%m-%x\");";
                        }
                        if (surec == 1)
                        {
                            queryGorev += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\");";
                        }
                        if (proje == 1)
                        {
                            queryGorev += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%b-%Y\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%V-%m-%x\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporKullanicilaraGoreModel> gyList = gyl.Result;
                        List<RaporKullanicilaraGoreModel> srcList = sl.Result;
                        List<RaporKullanicilaraGoreModel> prjList = pl.Result;

                        DatasetModel grvDataset = new DatasetModel();
                        grvDataset.label = "Görev";
                        grvDataset.borderColor = raporTurleri.gorevRenk;
                        DatasetModel prjDataset = new DatasetModel();
                        prjDataset.label = "Proje";
                        prjDataset.borderColor = raporTurleri.projeRenk;
                        DatasetModel srcDataset = new DatasetModel();
                        srcDataset.label = "Süreç";
                        srcDataset.borderColor = raporTurleri.surecRenk;*/
                        /*for (int i = 3; i >= 0; i--)
                        {
                            int weekOfYear = OurFunctions.WeeksInYear(now.AddDays(-(i * 7))) - 1;
                            string ayStr = weekOfYear + "-" + now.AddDays(-(i * 7)).ToString("MM-yyyy");
                            string ayStrText = (i + 1).ToString() + " Hafta Önce";
                            RaporKullanicilaraGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporKullanicilaraGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporKullanicilaraGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporKullanicilaraGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporKullanicilaraGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporKullanicilaraGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }*/
                        /*if (gorev == 1)
                        {
                            data.datasets.Add(grvDataset);
                        }
                        if (surec == 1)
                        {
                            data.datasets.Add(srcDataset);
                        }
                        if (proje == 1)
                        {
                            data.datasets.Add(prjDataset);
                        }*/
                        #endregion aylık
                    /*}
                    else if (periyot == raporPeriyotlari.haftalik)
                    {*/
                        #region haftalık
                        /*string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(g." + groupBy + ", \"%a\") as tarihText from gorevler as g where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumProjeSurecStr + " and " + gorevTarihStr + " group by DATE_FORMAT(g." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (surec == 1)
                        {
                            queryGorev += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        if (proje == 1)
                        {

                            queryGorev += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", \"%d-%m-%x\") as tarih, DATE_FORMAT(ps." + groupBy + ", \"%a\") as tarihText from proje_surec as ps where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by DATE_FORMAT(ps." + groupBy + ", \"%d-%M-%x\");";
                        }
                        var gyl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporKullanicilaraGoreModel> gyList = gyl.Result;
                        List<RaporKullanicilaraGoreModel> srcList = sl.Result;
                        List<RaporKullanicilaraGoreModel> prjList = pl.Result;

                        DatasetModel grvDataset = new DatasetModel();
                        grvDataset.label = "Görev";
                        grvDataset.borderColor = raporTurleri.gorevRenk;
                        DatasetModel prjDataset = new DatasetModel();
                        prjDataset.label = "Proje";
                        prjDataset.borderColor = raporTurleri.projeRenk;
                        DatasetModel srcDataset = new DatasetModel();
                        srcDataset.label = "Süreç";
                        srcDataset.borderColor = raporTurleri.surecRenk;*/
                        /*for (int i = 6; i >= 0; i--)
                        {
                            string ayStr = now.AddDays(-i).ToString("dd-MM-yyyy");
                            string ayStrText = (i + 1).ToString() + " Gün Önce";
                            RaporKullanicilaraGoreModel grvVarMi = gyList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporKullanicilaraGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.tarih = ayStr;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporKullanicilaraGoreModel srcVarMi = srcList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporKullanicilaraGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.tarih = ayStr;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporKullanicilaraGoreModel prjVarMi = prjList.Where(e => e.tarih.Equals(ayStr)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporKullanicilaraGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.tarih = ayStr;
                            }
                            prjDataset.data.Add(prjVarMi.adet);

                            data.labels.Add(ayStrText);
                        }*/
                        /*if (gorev == 1)
                        {
                            data.datasets.Add(grvDataset);
                        }
                        if (surec == 1)
                        {
                            data.datasets.Add(srcDataset);
                        }
                        if (proje == 1)
                        {
                            data.datasets.Add(prjDataset);
                        }*/
                        #endregion haftalık
                    /*}
                    else
                    {*/
                        #region custom
                        /*string queryGorev = queryRapor;
                        string querySurec = queryRapor;
                        string queryProje = queryRapor;
                        int toplamGun = (int)(DateTime.Parse(bitis) - DateTime.Parse(baslangic)).TotalDays;
                        if (gorev == 1)
                        {
                            queryGorev += "select count(g.id) as adet, DATE_FORMAT(g." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(g." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                            + "from gorevler as g "
                            + "inner join kullanici_gorev as kp on kp.gorev_id = g.id and kp.flag = " + durumlar.aktif + " "
                            + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                            + "where g.firma_id = " + lgm.firma_id + " and g.flag = " + durumlar.aktif + " and " + durumGorevStr + " " + gorevTarihStr + " group by k.url;";
                        }
                        if (surec == 1)
                        {
                            querySurec += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(ps." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                            + "from proje_surec as ps "
                            + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                            + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                            + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.surec + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by k.url;";
                        }
                        if (proje == 1)
                        {

                            queryProje += "select count(ps.id) as adet, DATE_FORMAT(ps." + groupBy + ", '%d-%m-%x') as tarih, DATE_FORMAT(ps." + groupBy + ", '%a') as tarihText, concat(k.ad, ' ', k.soyad) as label, k.url "
                            + "from proje_surec as ps "
                            + "inner join kullanici_proje as kp on kp.proje_id = ps.id and kp.flag = " + durumlar.aktif + " "
                            + "inner join kullanicilar as k on kp.kullanici_id = k.id and k.flag = " + durumlar.aktif + " and " + userUrls + " "
                            + "where ps.firma_id = " + lgm.firma_id + " and ps.flag = " + durumlar.aktif + " and ps.tur = " + ProjeSurecTur.proje + " and " + durumProjeSurecStr + " " + projeTarihStr + " group by k.url;";
                        }
                        var gyl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryGorev).ToListAsync();
                        var sl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(querySurec).ToListAsync();
                        var pl = db.Database.SqlQuery<RaporKullanicilaraGoreModel>(queryProje).ToListAsync();

                        await Task.WhenAll(gyl, sl, pl);

                        List<RaporKullanicilaraGoreModel> gyList = gyl.Result;
                        List<RaporKullanicilaraGoreModel> srcList = sl.Result;
                        List<RaporKullanicilaraGoreModel> prjList = pl.Result;

                        GroupedBarChartDataset grvDataset = new GroupedBarChartDataset();
                        grvDataset.label = "Görev";
                        grvDataset.backgroundColor = raporTurleri.gorevRenk;
                        GroupedBarChartDataset prjDataset = new GroupedBarChartDataset();
                        prjDataset.label = "Proje";
                        prjDataset.backgroundColor = raporTurleri.projeRenk;
                        GroupedBarChartDataset srcDataset = new GroupedBarChartDataset();
                        srcDataset.label = "Süreç";
                        srcDataset.backgroundColor = raporTurleri.surecRenk;*/
                        /*for (int i = 0; i < kullaniciList.Count; i++)
                        {
                            string kullaniciUrl = kullaniciList[i];
                            RaporKullanicilaraGoreModel grvVarMi = gyList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                            if (grvVarMi == null)
                            {
                                grvVarMi = new RaporKullanicilaraGoreModel();
                                grvVarMi.adet = 0;
                                grvVarMi.url = kullaniciUrl;
                            }
                            grvDataset.data.Add(grvVarMi.adet);

                            RaporKullanicilaraGoreModel srcVarMi = srcList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                            if (srcVarMi == null)
                            {
                                srcVarMi = new RaporKullanicilaraGoreModel();
                                srcVarMi.adet = 0;
                                srcVarMi.url = kullaniciUrl;
                            }
                            srcDataset.data.Add(srcVarMi.adet);

                            RaporKullanicilaraGoreModel prjVarMi = prjList.Where(e => e.url.Equals(kullaniciUrl)).FirstOrDefault();
                            if (prjVarMi == null)
                            {
                                prjVarMi = new RaporKullanicilaraGoreModel();
                                prjVarMi.adet = 0;
                                prjVarMi.url = kullaniciUrl;
                            }
                            prjDataset.data.Add(prjVarMi.adet);
                        }*/
                        /*if (gorev == 1)
                        {
                            data.datasets.Add(grvDataset);
                        }
                        if (surec == 1)
                        {
                            data.datasets.Add(srcDataset);
                        }
                        if (proje == 1)
                        {
                            data.datasets.Add(prjDataset);
                        }*/
                        #endregion custom
                    //}

                    if (gorev == 1)
                    {
                        data.datasets.Add(grvDataset);
                    }
                    if (surec == 1)
                    {
                        data.datasets.Add(srcDataset);
                    }
                    if (proje == 1)
                    {
                        data.datasets.Add(prjDataset);
                    }

                    List<object> nesneler = new List<object>();
                    nesneler.Add(gyListDetay);
                    /*if (gorev == 1)
                    {
                        nesneler.Add(gyListDetay);
                    }
                    if (surec == 1)
                    {
                        nesneler.Add(srcListDetay);
                    }
                    if (proje == 1)
                    {
                        nesneler.Add(prjListDetay);
                    }*/

                    var jsonSerialiser = new JavaScriptSerializer();
                    string json = jsonSerialiser.Serialize(data);

                    RaporDonusModel rdm = new RaporDonusModel();
                    rdm.json = json;
                    rdm.liste = nesneler;

                    JsonSonuc sonuc = JsonSonuc.sonucUret(true, rdm);

                    return Json(sonuc, JsonRequestBehavior.AllowGet);
                    #endregion kullanicilara göre
                }
                return Json(JsonSonuc.sonucUret(false, "Rapor türü bulunamadı."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Raporlar
        #region mail gönder
        [AreaAuthorize("Kullanici", "")]
        public async Task<JsonResult> mailGonder()
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                UserMailModel umm = new UserMailModel();
                foreach (var property in umm.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = umm.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(umm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(umm, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(umm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(umm, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                firma_musavir fm = db.firma_musavir.Where(e => e.id == lgm.firma_id).FirstOrDefault();
                if (fm != null)
                {
                    string emailMesaj = "<div>Bu mail " + config.projeİsmi + " sistemi aracılığı ile " + fm.firma_adi + " tarafından gönderilmiştir.</div></br>"
                    + "<div>" + umm.message + "</div></br>"
                    + "<div>Bu mail " + config.projeİsmi + " sistemi aracılığı ile gönderilmiştir.</div>";
                    bool mailGonderildi = EmailFunctions.sendEmailGmail(emailMesaj, umm.subject, umm.tomail, MailHedefTur.musteri, 0, fm.firma_mail, lgm.id, fm.mail_pass, fm.mail_port, fm.mail_ssl, fm.mail_host, -2);
                    if (!mailGonderildi)
                    {
                         return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(JsonSonuc.sonucUret(false, "Firma bilgileriniz bulunamadı."), JsonRequestBehavior.AllowGet);
                }

                return Json(JsonSonuc.sonucUret(true, "Mail Gönderildi."), JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion mail gönder
        
        #region temp işlemleri
        [AreaAuthorize("Yonetici", "")]
        [HttpPost]
        public JsonResult tempGorevleriGetir(string tempGuid)
        {
            try
            {
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                return Json(JsonSonuc.sonucUret(true, tempObj.gorevList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(JsonSonuc.sonucUret(false, "Görevler getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yonetici", "")]
        [HttpPost]
        public JsonResult tempGorevMusterileriGetir(string tempGuid, string gorevId)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<gorev_musteri> gorevMusteriList = tempObj.gorevMusteriList.Where(e => e.gorev_id == intGorevId).ToList();

                List<kullanicilar> kList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToList();
                List<tempKullaniciProjeOzetModel> kpList = new List<tempKullaniciProjeOzetModel>();

                for (int i = 0; i < gorevMusteriList.Count; i++ )
                {
                    kullanicilar usr = kList.Where(e => e.id == gorevMusteriList[i].kullanici_id).FirstOrDefault();

                    bool yeniEklendi = false;

                    tempKullaniciProjeOzetModel tlpom = kpList.Where(e => e.kullaniciUrl == usr.url).FirstOrDefault();

                    if (tlpom == null)
                    {
                        tlpom = new tempKullaniciProjeOzetModel();
                        tlpom.ad = usr.ad;
                        tlpom.soyad = usr.soyad;
                        tlpom.kullaniciUrl = usr.url;
                        tlpom.kullaniciId = usr.id;
                        yeniEklendi = true;
                    }

                    if (!tlpom.musteriIdList.Contains(gorevMusteriList[i].musteri_id))
                    {
                        tlpom.musteriIdList.Add(gorevMusteriList[i].musteri_id);
                    }
                    if (!tlpom.idList.Contains(gorevMusteriList[i].id))
                    {
                        tlpom.idList.Add(gorevMusteriList[i].id);
                    }

                    if (yeniEklendi)
                    {
                        kpList.Add(tlpom);
                    }
                }

                return Json(JsonSonuc.sonucUret(true, kpList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev müşterileri getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        [AreaAuthorize("Yonetici", "")]
        [HttpPost]
        public JsonResult tempGorevGetir(string gorevId, string tempGuid)
        {
            try
            {
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                gorevler g = tempObj.gorevList.Where(e => e.id == intGorevId).FirstOrDefault();
                return Json(JsonSonuc.sonucUret(true, g), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempGorevKullanicilariGetir(string tempGuid, string gorevId)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<kullanici_gorev> gorevKullaniciList = tempObj.kullaniciGorevList.Where(e => e.gorev_id == intGorevId).ToList();

                vrlfgysdbEntities db = new vrlfgysdbEntities();
                List<kullanicilar> kList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToList();
                List<KullaniciProjeOzetModel> kpList = new List<KullaniciProjeOzetModel>();

                for (int i = 0; i < gorevKullaniciList.Count; i++)
                {
                    kullanicilar usr = kList.Where(e => e.id == gorevKullaniciList[i].kullanici_id).FirstOrDefault();

                    KullaniciProjeOzetModel tlpom = new KullaniciProjeOzetModel();
                    tlpom.ad = usr.ad;
                    tlpom.soyad = usr.soyad;
                    tlpom.id = gorevKullaniciList[i].id;
                    tlpom.kullanici_id = usr.id;
                    tlpom.url = usr.url;
                    kpList.Add(tlpom);
                }

                return Json(JsonSonuc.sonucUret(true, kpList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev kullanıcıları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempGorevYapilacaklariGetir(string gorevId, string tempGuid)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<yapilacaklar> gorevYapilacakList = tempObj.yapilacaklarList.Where(e => e.gorev_id == intGorevId).ToList();

                return Json(JsonSonuc.sonucUret(true, gorevYapilacakList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev yapılacakları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempGorevDosyalariGetir(string gorevId, string tempGuid)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<gorev_dosya> gorevDosyaList = tempObj.gorevDosyalarList.Where(e => e.gorev_id == intGorevId).ToList();
                List<dosyaOzetModel> dosyaList = new List<dosyaOzetModel>();
                foreach (gorev_dosya gd in gorevDosyaList)
                {
                    dosyalar d = tempObj.dosyalarList.Where(e => e.id == gd.dosya_id).FirstOrDefault();
                    if (d != null)
                    {
                        dosyaOzetModel dom = new dosyaOzetModel();
                        dom.id = gd.id;
                        dom.isim = d.isim;
                        dom.url = d.url;
                        dosyaList.Add(dom);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, dosyaList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev dosyaları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempProjeKullanicilariGetir(string tempGuid)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                tempKontrol(lgm, tempGuid, Session);

                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<kullanici_proje> projeKullaniciList = tempObj.projeKullaniciList;

                vrlfgysdbEntities db = new vrlfgysdbEntities();
                List<kullanicilar> kList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToList();
                List<KullaniciProjeOzetModel> kpList = new List<KullaniciProjeOzetModel>();

                for (int i = 0; i < projeKullaniciList.Count; i++)
                {
                    kullanicilar usr = kList.Where(e => e.id == projeKullaniciList[i].kullanici_id).FirstOrDefault();

                    KullaniciProjeOzetModel tlpom = new KullaniciProjeOzetModel();
                    tlpom.ad = usr.ad;
                    tlpom.soyad = usr.soyad;
                    tlpom.id = projeKullaniciList[i].id;
                    tlpom.kullanici_id = usr.id;
                    kpList.Add(tlpom);
                }

                return Json(JsonSonuc.sonucUret(true, kpList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Proje kullanıcıları getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempProjeMusterileriGetir(string tempGuid)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                tempKontrol(lgm, tempGuid, Session);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                //List<proje_musteri> projeMusteriList = tempObj.projeMusteriList;

                vrlfgysdbEntities db = new vrlfgysdbEntities();
                //List<kullanicilar> kList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id).ToList();
                List<KullaniciProjeOzetModel> kpList = new List<KullaniciProjeOzetModel>();

                List<musteriler> musteriList = db.musteriler.Where(e => e.firma_id == lgm.fm.id && e.flag == durumlar.aktif).ToList();

                if (tempObj.projeMusteriList.Count > 0)
                {
                    foreach (proje_musteri pm in tempObj.projeMusteriList)
                    {
                        musteriler m = musteriList.Where(e => e.id == pm.musteri_id).FirstOrDefault();
                        if (m != null)
                        {
                            KullaniciProjeOzetModel kpom = new KullaniciProjeOzetModel();
                            kpom.id = pm.id;
                            kpom.musteri_id = m.id;
                            kpom.ad = m.firma_adi;
                            kpList.Add(kpom);
                        }                        
                    }                    
                }                

                /*for (int i = 0; i < projeMusteriList.Count; i++)
                {
                    //kullanicilar usr = kList.Where(e => e.id == projeMusteriList[i].kullanici_id).FirstOrDefault();

                    bool yeniEklendi = false;

                    //tempKullaniciProjeOzetModel tlpom = kpList.Where(e => e.kullaniciUrl == usr.url).FirstOrDefault();

                    if (tlpom == null)
                    {
                        tlpom = new tempKullaniciProjeOzetModel();
                        //tlpom.ad = usr.ad;
                        //tlpom.soyad = usr.soyad;
                        //tlpom.kullaniciUrl = usr.url;
                        yeniEklendi = true;
                    }

                    if (!tlpom.musteriIdList.Contains(projeMusteriList[i].musteri_id))
                    {
                        tlpom.musteriIdList.Add(projeMusteriList[i].musteri_id);
                    }
                    if (!tlpom.idList.Contains(projeMusteriList[i].id))
                    {
                        tlpom.idList.Add(projeMusteriList[i].id);
                    }

                    if (yeniEklendi)
                    {
                        kpList.Add(tlpom);
                    }
                }*/

                return Json(JsonSonuc.sonucUret(true, kpList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev müşterileri getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult tempGorevBaglantilariGetir(string tempGuid, string gorevId)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int intGorevId = Convert.ToInt32(gorevId);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<gorev_baglanti> gorevBaglantiList = tempObj.gorevBaglantilari.Where(e => e.gorev_id == intGorevId).ToList();

                return Json(JsonSonuc.sonucUret(true, gorevBaglantiList), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Görev bağlantilari getirilirken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonSonuc tempGorevMusteriEkle(LoggedUserModel lgm, HttpRequestBase Request, string[] musteriList)
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);                
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                string gorevIdStr = Request["gorev_id"];
                int gorev_id = 0;
                if (gorevIdStr == null || gorevIdStr.Equals(string.Empty) || gorevIdStr.Equals("0"))
                {
                    gorevler grv = new gorevler();
                    gorev_id = 1;
                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                    {
                        gorev_id = tempObj.gorevList.Max(e => e.id) + 1;
                    }
                    grv.id = gorev_id;
                    tempObj.gorevList.Add(grv);
                }
                else
                {
                    gorev_id = Convert.ToInt32(gorevIdStr);
                }
                foreach (string mstr in musteriList)
                {
                    gorev_musteri kg = new gorev_musteri();
                    foreach (var property in kg.GetType().GetProperties())
                    {
                        try
                        {
                            var response = Request[property.Name];
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = kg.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(kg, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(kg, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(kg, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(kg, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception)
                        { }
                    }

                    int id = 1;
                    if (tempObj.gorevMusteriList != null && tempObj.gorevMusteriList.Count > 0)
                    {
                        id = tempObj.gorevMusteriList.Max(e => e.id) + 1;
                    }                   

                    kg.id = id;
                    kg.flag = durumlar.aktif;
                    kg.date = DateTime.Now;
                    kg.vid = id;
                    kg.sort = id;
                    kg.musteri_id = Convert.ToInt32(mstr);
                    kg.ekleyen = GetCurrentUser.GetUser().id;
                    kg.gorev_id = gorev_id;

                    if (kg.musteri_id == 0 || kg.kullanici_id == 0)
                    {
                        return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                    }

                    gorev_musteri dbKg = tempObj.gorevMusteriList.Where(e => e.flag == durumlar.aktif && e.gorev_id == kg.gorev_id && e.musteri_id == kg.musteri_id && e.kullanici_id == kg.kullanici_id).FirstOrDefault();
                    if (dbKg != null)
                    {
                        continue;
                    }

                    tempObj.gorevMusteriList.Add(kg); 
                }

                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, gorev_id);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempGorevKullaniciEkle(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);                
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                string gorevIdStr = Request["gorev_id"];
                int gorev_id = 0;
                if (gorevIdStr == null || gorevIdStr.Equals(string.Empty) || gorevIdStr.Equals("0"))
                {
                    gorevler grv = new gorevler();
                    gorev_id = 1;
                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                    {
                        gorev_id = tempObj.gorevList.Max(e => e.id) + 1;
                    }
                    grv.id = gorev_id;
                    tempObj.gorevList.Add(grv);
                }
                else
                {
                    gorev_id = Convert.ToInt32(gorevIdStr);
                }
                kullanici_gorev kg = new kullanici_gorev();
                foreach (var property in kg.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = kg.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(kg, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(kg, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(kg, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(kg, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                int id = 1;
                if (tempObj.kullaniciGorevList != null && tempObj.kullaniciGorevList.Count > 0)
                {
                    id = tempObj.kullaniciGorevList.Max(e => e.id) + 1;
                }

                kg.id = id;
                kg.flag = durumlar.aktif;
                kg.date = DateTime.Now;
                kg.vid = id;
                kg.sort = id;
                kg.ekleyen = GetCurrentUser.GetUser().id;
                kg.gorev_id = gorev_id;

                kullanici_gorev dbKg = tempObj.kullaniciGorevList.Where(e => e.flag == durumlar.aktif && e.gorev_id == kg.gorev_id && e.kullanici_id == kg.kullanici_id).FirstOrDefault();
                if (dbKg != null)
                {
                    return JsonSonuc.sonucUret(true, gorev_id);
                }

                tempObj.kullaniciGorevList.Add(kg);
                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, gorev_id);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempYeniGorevBag(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                string gorevIdStr = Request["gorev_id"];
                int gorev_id = 0;
                if (gorevIdStr == null || gorevIdStr.Equals(string.Empty) || gorevIdStr.Equals("0"))
                {
                    gorevler grv = new gorevler();
                    gorev_id = 1;
                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                    {
                        gorev_id = tempObj.gorevList.Max(e => e.id) + 1;
                    }
                    grv.id = gorev_id;
                    tempObj.gorevList.Add(grv);
                }
                else
                {
                    gorev_id = Convert.ToInt32(gorevIdStr);
                }
                gorev_baglanti gb = new gorev_baglanti();
                foreach (var property in gb.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = gb.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(gb, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(gb, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(gb, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(gb, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                int id = 1;
                if (tempObj.gorevBaglantilari != null && tempObj.gorevBaglantilari.Count > 0)
                {
                    id = tempObj.gorevBaglantilari.Max(e => e.id) + 1;
                }

                gb.gorev_id = gorev_id;
                gb.id = id;
                gb.flag = durumlar.aktif;
                gb.date = DateTime.Now;
                gb.vid = id;
                gb.sort = id;
                gb.ekleyen = GetCurrentUser.GetUser().id;

                gorev_baglanti dbKg = tempObj.gorevBaglantilari.Where(e => e.flag == durumlar.aktif && e.gorev_id == gb.gorev_id && e.bagli_gorev == gb.bagli_gorev).FirstOrDefault();
                if (dbKg != null)
                {
                    return JsonSonuc.sonucUret(true, gb.gorev_id);
                }

                tempObj.gorevBaglantilari.Add(gb);
                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, gb.gorev_id);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempGoreviEkle(LoggedUserModel lgm, HttpRequestBase Request, string gorev_id)
        {
            string tempGuid = Request["tempGuid"];
            tempKontrol(lgm, tempGuid, Session);            
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];

            //bool yeniEklendi = false;
            if (gorev_id == null ||  gorev_id.Equals(string.Empty))
            {
                gorev_id = "0";
            }
            int id = Convert.ToInt32(gorev_id);
            if (id == 0)
            {
                id = 1;
                if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                {
                    id = tempObj.gorevList.Max(e => e.id) + 1;
                }
                gorevler grv = new gorevler();
                grv.id = id;
                tempObj.gorevList.Add(grv);
                //yeniEklendi = true;
            }

            string grvMultiply = Request["gorev_multiply"];
            int gorev_multiply = 0;
            if (grvMultiply != null)
            {
                gorev_multiply = 1;
            }

            for (int i = 0; i < tempObj.gorevList.Count; i++)
            {
                if (tempObj.gorevList[i].id == id)
                {
                    foreach (var property in tempObj.gorevList[i].GetType().GetProperties())
                    {
                        try
                        {
                            var response = Request[property.Name];
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = tempObj.gorevList[i].GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(tempObj.gorevList[i], Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(tempObj.gorevList[i], Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(tempObj.gorevList[i], Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(tempObj.gorevList[i], Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception)
                        { }
                    }

                    tempObj.gorevList[i].gorev_multiply = gorev_multiply;
                    tempObj.gorevList[i].url = id.ToString();
                    tempObj.gorevList[i].firma_id = lgm.firma_id;
                    tempObj.gorevList[i].flag = durumlar.aktif;
                    tempObj.gorevList[i].date = DateTime.Now;
                    tempObj.gorevList[i].vid = tempObj.gorevList[i].id;
                    tempObj.gorevList[i].ekleyen = lgm.id;
                    tempObj.gorevList[i].sort = tempObj.gorevList[i].id;
                    tempObj.gorevList[i].durum = TamamlamaDurumlari.bekliyor;
                    tempObj.gorevList[i].tamamlanma_tarihi = DateTime.Now;
                    tempObj.gorevList[i].id = id;

                    if (tempGuid.Contains("Gorev_"))
                    {
                        tempObj.gorevList[i].id = Convert.ToInt32(tempObj.gorevList[i].url);
                        gorevIslemleri gorevIslm = new gorevIslemleri();
                        return gorevIslm.goreviKaydet(Convert.ToInt32(Request["proje_id"].ToString()), tempGuid, tempObj.gorevList[i], Request, Session, Server);
                    }
                    else
                    {
                        Session[tempGuid] = tempObj;
                        if (tempObj.gorevList[i].gorev_multiply == GorevMultiplyDurum.multiply)
                        {
                            string gorevIsmi = tempObj.gorevList[i].isim;
                            List<kullanici_gorev> kullaniciList = tempObj.kullaniciGorevList.Where(e => e.gorev_id == tempObj.gorevList[i].id).ToList();
                            int kullanici_id = kullaniciList[0].kullanici_id;
                            kullanicilar usr1 = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.id == kullanici_id).FirstOrDefault();
                            tempObj.gorevList[i].isim = gorevIsmi + "(" + usr1.ad + " " + usr1.soyad + ")";
                            for (int j = 1; j < kullaniciList.Count; j++)
                            {
                                kullanici_id = kullaniciList[j].kullanici_id;
                                kullanicilar usr = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.id == kullanici_id).FirstOrDefault();
                                if (usr == null)
                                {
		                            continue;
                                }
                                #region görevi klonla
                                gorevler yeniGorev = new gorevler();
                                CloneObject.CopyTo(tempObj.gorevList[i], yeniGorev);
                                int idYeniGorev = 1;
                                if (tempObj.gorevList.Count != 0)
                                {
                                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                                    {
                                        idYeniGorev = tempObj.gorevList.Max(e => e.id) + 1;
                                    }
                                }
                                yeniGorev.id = idYeniGorev;
                                yeniGorev.vid = idYeniGorev;
                                yeniGorev.url = idYeniGorev.ToString();
                                yeniGorev.isim = gorevIsmi + "(" + usr.ad + " " + usr.soyad + ")";
                                tempObj.gorevList.Add(yeniGorev);
                                #endregion görevi klonla
                                #region kullanıcıları düzenle
                                for (int k = 0; k < tempObj.kullaniciGorevList.Count; k++)
                                {
                                    if (tempObj.kullaniciGorevList[k].id == kullaniciList[j].id)
                                    {
                                        tempObj.kullaniciGorevList[k].gorev_id = yeniGorev.id;
                                    }
                                }
                                #endregion kullanıcıları düzenle
                                #region müşterileri düzenle
                                for (int m = 0; m < tempObj.gorevMusteriList.Count; m++)
                                {
                                    if (tempObj.gorevMusteriList[m].gorev_id == tempObj.gorevList[i].id && tempObj.gorevMusteriList[m].kullanici_id == kullaniciList[j].kullanici_id)
                                    {
                                        tempObj.gorevMusteriList[m].gorev_id = yeniGorev.id;
                                    }
                                }
                                #endregion müşterileri düzenle
                                #region yapılacakları düzenle
                                List<yapilacaklar> yapilacakList = tempObj.yapilacaklarList.Where(e => e.gorev_id == tempObj.gorevList[i].id).ToList();
                                foreach (yapilacaklar yplck in yapilacakList)
                                {
                                    yapilacaklar yeniYplck = new yapilacaklar();
                                    CloneObject.CopyTo(yplck, yeniYplck);
                                    int idYplck = 1;
                                    if (tempObj != null && tempObj.yapilacaklarList.Count > 0)
                                    {
                                        idYplck = tempObj.yapilacaklarList.Max(e => e.id) + 1;
                                    }
                                    yeniYplck.id = idYplck;
                                    yeniYplck.vid = idYplck;
                                    yeniYplck.url = idYplck.ToString();
                                    yeniYplck.gorev_id = yeniGorev.id;
                                    tempObj.yapilacaklarList.Add(yeniYplck);
                                }
                                #endregion yapılacakları düzenle
                                #region dosyaları düzenle
                                List<gorev_dosya> dosyaList = tempObj.gorevDosyalarList.Where(e => e.gorev_id == tempObj.gorevList[i].id).ToList();
                                foreach (gorev_dosya gd in dosyaList)
                                {
                                    dosyalar eskiDosya = tempObj.dosyalarList.Where(e => e.id == gd.dosya_id).FirstOrDefault();
                                    dosyalar yeniDosya = new dosyalar();
                                    CloneObject.CopyTo(eskiDosya, yeniDosya);
                                    int idDosya = 1;
                                    if (tempObj != null && tempObj.gorevDosyalarList.Count > 0)
                                    {
                                        idDosya = tempObj.gorevDosyalarList.Max(e => e.id) + 1;
                                    }
                                    yeniDosya.id = idDosya;
                                    yeniDosya.vid = idDosya;
                                    yeniDosya.sort = idDosya;
                                    string pathDosya = "~/public/upload/dosyalar/temp";
                                    string ext = Path.GetExtension(eskiDosya.url);
                                    yeniDosya.url = "d_" + yeniDosya.vid.ToString() + "_" + tempGuid + ext;
                                    System.IO.File.Copy(Server.MapPath(pathDosya + "/" + eskiDosya.url), Server.MapPath(pathDosya + "/" + yeniDosya.url), true);
                                    tempObj.dosyalarList.Add(yeniDosya);

                                    gorev_dosya yeniGd = new gorev_dosya();
                                    CloneObject.CopyTo(gd, yeniGd);
                                    int idBag = 1;
                                    if (tempObj != null && tempObj.gorevDosyalarList.Count > 0)
                                    {
                                        idBag = tempObj.gorevDosyalarList.Max(e => e.id) + 1;
                                    }
                                    yeniGd.id = idBag;
                                    yeniGd.vid = idBag;
                                    yeniGd.sort = idBag;
                                    yeniGd.gorev_id = yeniGorev.id;
                                    yeniGd.dosya_id = yeniDosya.id;
                                    tempObj.gorevDosyalarList.Add(yeniGd);
                                }
                                #endregion dosyaları düzenle
                                #region bağları düzenle
                                List<gorev_baglanti> bagList = tempObj.gorevBaglantilari.Where(e => e.gorev_id == tempObj.gorevList[i].id).ToList();
                                foreach (gorev_baglanti bag in bagList)
                                {
                                    gorev_baglanti yeniBag = new gorev_baglanti();
                                    CloneObject.CopyTo(bag, yeniBag);
                                    int idBag = 1;
                                    if (tempObj != null && tempObj.gorevBaglantilari.Count > 0)
                                    {
                                        idBag = tempObj.gorevBaglantilari.Max(e => e.id) + 1;
                                    }
                                    yeniBag.id = idBag;
                                    yeniBag.vid = idBag;
                                    yeniBag.gorev_id = yeniGorev.id;
                                    tempObj.gorevBaglantilari.Add(yeniBag);
                                }
                                #endregion bağları düzenle
                            }
                        }
                    }
                    break;
                }
            }
            
            return JsonSonuc.sonucUret(true, "Görev Eklendi.");
        }
        public bool tempKontrol(LoggedUserModel lgm, string tempGuid, HttpSessionStateBase session)
        {
            if (session[tempGuid] == null)
            {
                TempAddingObject tempObj = new TempAddingObject();
                tempObj.tempGuid = tempGuid;
                tempObj.projeSurec = new proje_surec();
                tempObj.projeSurec.firma_id = lgm.firma_id;
                tempObj.gorevList = new List<gorevler>();
                tempObj.projeKullaniciList = new List<kullanici_proje>();
                tempObj.projeMusteriList = new List<proje_musteri>();
                tempObj.gorevMusteriList = new List<gorev_musteri>();
                tempObj.kullaniciGorevList = new List<kullanici_gorev>();
                tempObj.gorevDosyalarList = new List<gorev_dosya>();
                tempObj.dosyalarList = new List<dosyalar>();
                tempObj.yapilacaklarList = new List<yapilacaklar>();
                Session[tempObj.tempGuid] = tempObj;
            }
            
            return true;
        }
        
        public JsonSonuc tempYeniYapilacak(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);                
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                string gorevIdStr = Request["gorev_id"];
                int gorev_id = 0;
                if (gorevIdStr == null || gorevIdStr.Equals(string.Empty) || gorevIdStr.Equals("0"))
                {
                    gorevler grv = new gorevler();
                    gorev_id = 1;
                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                    {
                        gorev_id = tempObj.gorevList.Max(e => e.id) + 1;
                    }
                    grv.id = gorev_id;
                    tempObj.gorevList.Add(grv);
                }
                else
                {
                    gorev_id = Convert.ToInt32(gorevIdStr);
                }
                yapilacaklar yplck = new yapilacaklar();
                foreach (var property in yplck.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = yplck.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(yplck, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(yplck, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(yplck, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(yplck, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }
                yplck.firma_id = lgm.firma_id;

                int id = 1;
                if (tempObj != null && tempObj.yapilacaklarList.Count > 0)
                {
                    id = tempObj.yapilacaklarList.Max(e => e.id) + 1;
                }

                yplck.id = id;
                yplck.gorev_id = gorev_id;
                yplck.url = id.ToString();
                yplck.flag = durumlar.aktif;
                yplck.date = DateTime.Now;
                yplck.vid = id;
                yplck.ekleyen = GetCurrentUser.GetUser().id;
                yplck.sort = id;
                yplck.durum = YapilacaklarDurum.beklemede;

                tempObj.yapilacaklarList.Add(yplck);
                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, yplck.gorev_id);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempYeniGorevDosyasi(HttpRequestBase Request, HttpServerUtilityBase server)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            string tempGuid = Request["tempGuid"];
            tempKontrol(lgm, tempGuid, Session);            
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            try
            {                                
                string gorevIdStr = Request["gorev_id"];
                int gorev_id = 0;
                if (gorevIdStr == null || gorevIdStr.Equals(string.Empty) || gorevIdStr.Equals("0"))
                {
                    gorevler grv = new gorevler();
                    gorev_id = 1;
                    if (tempObj.gorevList != null && tempObj.gorevList.Count > 0)
                    {
                        gorev_id = tempObj.gorevList.Max(e => e.id) + 1;
                    }
                    grv.id = gorev_id;
                    tempObj.gorevList.Add(grv);
                }
                else
                {
                    gorev_id = Convert.ToInt32(gorevIdStr);
                }
                dosyalar d = new dosyalar();
                foreach (var property in d.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = d.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(d, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(d, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(d, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(d, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                int id = 1;
                if (tempObj != null && tempObj.dosyalarList.Count > 0)
                {
                    id = tempObj.dosyalarList.Max(e => e.id) + 1;
                }
                d.id = id;
                d.aciklama = "";
                d.flag = durumlar.aktif;
                d.date = DateTime.Now;
                d.vid = id;
                d.sort = id;
                d.ekleyen = GetCurrentUser.GetUser().id;

                string pathDosya = "~/public/upload/dosyalar/temp";
                if (!Directory.Exists(server.MapPath(pathDosya)))
                {
                    Directory.CreateDirectory(server.MapPath(pathDosya));
                }

                HttpFileCollectionBase hfc = Request.Files;

                if (hfc.Count != 0)
                {
                    string ext = ".png";
                    HttpPostedFileBase hpf_img = hfc[0];

                    if (hpf_img.ContentLength > 0)
                    {

                        string fileName = "";
                        if (Request.Browser.Browser == "IE")
                        {
                            fileName = Path.GetFileName(hpf_img.FileName);
                        }
                        else
                        {
                            fileName = hpf_img.FileName;
                        }

                        ext = Path.GetExtension(fileName);
                        if ((ext == null || ext == string.Empty) || !(ext.Equals(".jpg") || ext.Equals(".jpeg") || ext.Equals(".png") || ext.Equals(".bmp") || ext.Equals(".docx") || ext.Equals(".doc") || ext.Equals(".txt") || ext.Equals(".pptx") || ext.Equals(".pdf") || ext.Equals(".xlsx") || ext.Equals(".pub")))
                        {
                            return JsonSonuc.sonucUret(false, "Sisteme sadece resim ve yazı içerikleri yükleyebilirsiniz. Desteklenen uzantılar: \".jpg, .jpeg, .png, .bmp, .docx, .doc, .txt, .pptx, .pdf, .xlsx, .pub\".");
                        }

                        string strFileName = StringFormatter.OnlyEnglishChar(d.isim);
                        string createdUrl = strFileName;
                        string tempUrl = createdUrl;
                        bool bulundu = false;
                        int i = 1;
                        dosyalar pg = new dosyalar();
                        do
                        {
                            pg = db.dosyalar.Where(e => e.url.Equals(tempUrl + ext)).FirstOrDefault();
                            if (pg != null)
                            {
                                tempUrl = strFileName + i.ToString();
                            }
                            else
                            {
                                createdUrl = tempUrl;
                                bulundu = true;
                            }
                            i++;
                        } while (!bulundu);
                        strFileName = createdUrl;

                        string createdFileName = strFileName;
                        string fullPathWithFileName = pathDosya + "/" + createdFileName + ext;
                        hpf_img.SaveAs(server.MapPath(fullPathWithFileName));
                        d.url = createdFileName + ext;

                        tempObj.dosyalarList.Add(d);
                    }
                    else
                    {
                        //return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                        return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                    }
                }
                else
                {
                    //return Json(FormReturnTypes.basarisiz, JsonRequestBehavior.AllowGet);
                    return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                }

                gorev_dosya gd = new gorev_dosya();
                foreach (var property in gd.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = gd.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(gd, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(gd, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(gd, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(gd, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                int gdId = 1;
                if (tempObj != null && tempObj.gorevDosyalarList.Count > 0)
                {
                    gdId = tempObj.gorevDosyalarList.Max(e => e.id) + 1;
                }

                gd.id = gdId;
                gd.flag = durumlar.aktif;
                gd.date = DateTime.Now;
                gd.vid = gdId;
                gd.sort = gdId;
                gd.ekleyen = GetCurrentUser.GetUser().id;
                gd.dosya_id = d.id;
                gd.gorev_id = gorev_id;

                tempObj.gorevDosyalarList.Add(gd);
                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, gorev_id);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempYeniProjeMusterisi(HttpRequestBase Request, string[] musteriList)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);                
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];

                foreach (string mstr in musteriList)
                {
                    proje_musteri kontrol = tempObj.projeMusteriList.Where(e => e.musteri_id.Equals(Convert.ToInt32(mstr))).FirstOrDefault();
                    if (kontrol != null)
                    {
                        continue;
                    }
                    proje_musteri pm = new proje_musteri();

                    int id = 1;
                    if (pm.id == 0)
                    {
                        if (tempObj != null && tempObj.projeMusteriList.Count > 0)
                        {
                            id = tempObj.projeMusteriList.Max(e => e.id) + 1;
                        }
                        pm.id = id;
                    }

                    pm.musteri_id = Convert.ToInt32(mstr);
                    pm.flag = durumlar.aktif;
                    pm.date = DateTime.Now;
                    pm.vid = id;
                    pm.sort = id;
                    pm.ekleyen = GetCurrentUser.GetUser().id;

                    tempObj.projeMusteriList.Add(pm);
                    Session[tempGuid] = tempObj;
                }

                return JsonSonuc.sonucUret(true, "Müşteri Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc tempYeniProjeKullanicisi(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string tempGuid = Request["tempGuid"];
                tempKontrol(lgm, tempGuid, Session);                
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];

                kullanici_proje kp = new kullanici_proje();
                foreach (var property in kp.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = kp.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(kp, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(kp, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(kp, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(kp, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                int id = 1;
                if (kp.id == 0)
                {
                    if (tempObj != null && tempObj.projeKullaniciList.Count > 0)
                    {
                        id = tempObj.projeKullaniciList.Max(e => e.id) + 1;
                    }
                    kp.id = id;
                }

                kp.flag = durumlar.aktif;
                kp.date = DateTime.Now;
                kp.vid = id;
                kp.sort = id;
                kp.ekleyen = lgm.id;

                kullanici_proje dbKp = tempObj.projeKullaniciList.Where(e => e.flag == durumlar.aktif && e.kullanici_id == kp.kullanici_id).FirstOrDefault();
                if (dbKp != null)
                {
                    return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
                }

                tempObj.projeKullaniciList.Add(kp);
                Session[tempGuid] = tempObj;

                return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }

        public JsonSonuc tempProjeSurecKaydet(HttpRequestBase Request, int projeSurecTur)
        {
            try
            {
                string tempGuid = Request["tempGuid"];
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];

                LoggedUserModel lgm = GetCurrentUser.GetUser();
                #region projenin kaydedilmesi
                int vid = 1;
                if (db.proje_surec.Count() != 0)
                {
                    vid = db.proje_surec.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.proje_surec.Count() != 0)
                {
                    sort = db.proje_surec.Max(e => e.sort) + 1;
                }

                proje_surec prj = new proje_surec();
                foreach (var property in prj.GetType().GetProperties())
                {
                    try
                    {
                        var response = Request[property.Name];
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = prj.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(prj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(prj, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(prj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(prj, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(prj.isim);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                proje_surec pg = new proje_surec();
                do
                {
                    pg = db.proje_surec.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                prj.url = createdUrl;
                prj.firma_id = lgm.firma_id;

                prj.flag = durumlar.aktif;
                prj.date = DateTime.Now;
                prj.vid = vid;
                prj.ekleyen = GetCurrentUser.GetUser().id;
                prj.sort = sort;
                //prj.donem_sayisi = 0;
                prj.parent_vid = 0;
                prj.durum = TamamlamaDurumlari.bekliyor;
                //prj.periyot_suresi = 0;                
                prj.tamamlanma_tarihi = DateTime.Now;
                prj.tur = projeSurecTur;
                prj.parent_vid = vid;

                if (ProjeSurecTur.proje == projeSurecTur)
                {
                    prj.periyot_turu = 0;
                    prj.mevcut_donem = 0;
                }
                else if (ProjeSurecTur.proje == projeSurecTur)
                {
                    prj.periyot_turu = 0;
                    prj.mevcut_donem = 0;
                }

                string isimControl = "select * from proje_surec where tur = " + ProjeSurecTur.proje + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + prj.isim + "' and firma_id = " + prj.firma_id;
                ProjeSurecModel isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                if (isimKontrolPs != null)
                {
                    return JsonSonuc.sonucUret(false, "proje_isim_hatasi");
                }

                bool kullaniciKontrol = projeIslemleri.firmaProjeKontrol(prj.firma_id, prj.id).Result;
                if (!kullaniciKontrol)
                {
                    return JsonSonuc.sonucUret(false, "proje_sayisi_hatasi");
                }

                db.proje_surec.Add(prj);
                db.SaveChanges();
                #endregion projenin kaydedilmesi
                proje_surec dbProjeSurec = db.proje_surec.Where(e => e.vid == prj.vid).FirstOrDefault();
                #region proje kullanicilari
                foreach (kullanici_proje kp in tempObj.projeKullaniciList)
                {
                    int vidKullaniciProje = 1;
                    if (db.kullanici_proje.Count() != 0)
                    {
                        vidKullaniciProje = db.kullanici_proje.Max(e => e.vid) + 1;
                    }
                    int sortKullaniciProje = 1;
                    if (db.kullanici_proje.Count() != 0)
                    {
                        sortKullaniciProje = db.kullanici_proje.Max(e => e.sort) + 1;
                    }

                    kp.vid = vidKullaniciProje;
                    kp.sort = sortKullaniciProje;
                    kp.proje_id = dbProjeSurec.id;

                    db.kullanici_proje.Add(kp);
                    db.SaveChanges();
                }
                #endregion proje kullanicilari
                #region proje müşterileri
                foreach (proje_musteri pm in tempObj.projeMusteriList)
                {
                    int vidKullaniciProje = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        vidKullaniciProje = db.proje_musteri.Max(e => e.vid) + 1;
                    }
                    int sortKullaniciProje = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        sortKullaniciProje = db.proje_musteri.Max(e => e.sort) + 1;
                    }

                    pm.vid = vidKullaniciProje;
                    pm.sort = sortKullaniciProje;
                    pm.proje_id = dbProjeSurec.id;

                    db.proje_musteri.Add(pm);
                    db.SaveChanges();
                }
                #endregion proje müşterileri
                #region görevlerin kaydedilmesi
                foreach (gorevler grv in tempObj.gorevList)
                {
                    if (grv.isim != null && !grv.isim.Equals(string.Empty))
                    {
                        gorevIslemleri gorevIslm = new gorevIslemleri();
                        JsonSonuc sonuc = gorevIslm.goreviKaydet(dbProjeSurec.id, tempGuid, grv, Request, Session, Server);
                        if (!sonuc.IsSuccess)
                        {
                             return JsonSonuc.sonucUret(false, "Proje/Süreç görevleri kaydedilirken bir hata oluştu.");
                        }
                    }
                }
                #endregion görevlerin kaydedilmesi

                if (projeSurecTur == ProjeSurecTur.surec)
                {
                    surecIslemleri surecIsl = new surecIslemleri();
                    surecIsl.tekrarlananSurecKontrolu();    
                }
                
                return JsonSonuc.sonucUret(true, dbProjeSurec.url);
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "Proje keydedilirken bir hata oluştu.");
            }
        }
        

        public JsonSonuc tempProjeKullaniciSil(int proje_kullanici_id, string tempGuid)
        {
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            kullanici_proje kp = tempObj.projeKullaniciList.Where(e => e.id == proje_kullanici_id).FirstOrDefault();
            tempObj.projeKullaniciList.Remove(kp);
            Session[tempGuid] = tempObj;
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public JsonSonuc tempProjeMusterisiSil(string url, string tempGuid)
        {
            try
            {
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                List<proje_musteri> pmList = tempObj.projeMusteriList.ToList();
                for (int i = 0; i < pmList.Count; i++)
                {
                    tempObj.projeMusteriList.Remove(pmList[i]);
                }
                Session[tempGuid] = tempObj;
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Müşteri silindi.");
        }
        public JsonSonuc tempGorevSil(string id, string tempGuid)
        {
            try
            {
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                gorevler grv = tempObj.gorevList.Where(e => e.id.Equals(Convert.ToInt32(id))).FirstOrDefault();
                if (grv != null)
                {
                    tempObj.gorevList.Remove(grv);
                    Session[tempGuid] = tempObj;
                }                
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Müşteri silindi.");
        }

        public JsonSonuc tempGorevKullaniciSil(int gorev_kullanici_id, string tempGuid)
        {
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            kullanici_gorev kg = tempObj.kullaniciGorevList.Where(e => e.id == gorev_kullanici_id).FirstOrDefault();
            tempObj.kullaniciGorevList.Remove(kg);
            Session[tempGuid] = tempObj;
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public JsonSonuc tempGorevMusterisiSil(string url, string gorev_url, string tempGuid)
        {
            try
            {
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                kullanicilar usr = db.kullanicilar.Where(e => e.url.Equals(url)).FirstOrDefault();
                gorevler grv = tempObj.gorevList.Where(e => e.id == Convert.ToInt32(gorev_url)).FirstOrDefault();
                List<gorev_musteri> gmList = tempObj.gorevMusteriList.Where(e => e.kullanici_id == usr.id && e.gorev_id == grv.id).ToList();
                for (int i = 0; i < gmList.Count; i++)
                {
                    tempObj.gorevMusteriList.Remove(gmList[i]);
                }
                Session[tempGuid] = tempObj;
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Müşteri silindi.");
        }
        public JsonSonuc tempGorevYapilacakSil(int yapilacak_id, string tempGuid)
        {
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            yapilacaklar yplck = tempObj.yapilacaklarList.Where(e => e.id == yapilacak_id).FirstOrDefault();
            tempObj.yapilacaklarList.Remove(yplck);
            Session[tempGuid] = tempObj;
            return JsonSonuc.sonucUret(true, "Yapılacak silindi.");
        }
        public JsonSonuc tempGorevDosyaSil(int gd_id, string tempGuid)
        {
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            gorev_dosya gd = tempObj.gorevDosyalarList.Where(e => e.id == gd_id).FirstOrDefault();
            dosyalar d = tempObj.dosyalarList.Where(e => e.id == gd.dosya_id).FirstOrDefault();
            tempObj.gorevDosyalarList.Remove(gd);
            tempObj.dosyalarList.Remove(d);
            Session[tempGuid] = tempObj;
            return JsonSonuc.sonucUret(true, "Dosya silindi.");
        }
        public JsonSonuc tempGorevBagSil(int id, string tempGuid)
        {
            TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
            gorev_baglanti gb = tempObj.gorevBaglantilari.Where(e => e.id == id).FirstOrDefault();
            tempObj.gorevBaglantilari.Remove(gb);
            Session[tempGuid] = tempObj;
            return JsonSonuc.sonucUret(true, "Bağlantı silindi.");
        }

        public JsonSonuc tempGorevMusterisKullaniciGorevlendir(int gorevid, string hedef_kullanici, string kaynak_kullanici, string tempGuid)
        {
            try
            {
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];
                kullanicilar kaynakKullanici = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.url.Equals(kaynak_kullanici)).FirstOrDefault();
                kullanicilar hedefKullanici = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.url.Equals(hedef_kullanici)).FirstOrDefault();
                List<gorev_musteri> gorevMusteriList = tempObj.gorevMusteriList.Where(e => e.gorev_id == gorevid && e.kullanici_id == kaynakKullanici.id).ToList();

                for (int i = 0; i < tempObj.gorevMusteriList.Count; i++)
                {
                    if (tempObj.gorevMusteriList[i].gorev_id == gorevid && tempObj.gorevMusteriList[i].kullanici_id == kaynakKullanici.id)
                    {
                        tempObj.gorevMusteriList[i].kullanici_id = hedefKullanici.id;
                    }
                }
                Session[tempGuid] = tempObj;
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı yetkilendirildi.");
        }
        #endregion temp işlemleri

        #region Yardım Sayfaları
        public ActionResult Yardim()
        {
            string yardimQuery = "select * from yardim where flag = " + durumlar.aktif;
            List<YardimModel> yardimList = db.Database.SqlQuery<YardimModel>(yardimQuery).ToList();
            return View(yardimList);
        }
        #endregion Yardım Sayfaları

        #region Excel Export Table
        [AreaAuthorize("Kullanici", "")]
        [HttpPost]
        public JsonResult exportTable(string tableName, string baslangicTarihi, string bitisTarihi, int durum, string order, string descc, int tur)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                string excelUrl = "";                
                if (tableName.Equals("proje_surec") && tur == ProjeSurecTur.proje)
                {
                    Task<JsonResult> sonuc = ProjelerFiltre(durum, baslangicTarihi, bitisTarihi, order, descc);
                    List<ProjeSurecModel> dataList = (List<ProjeSurecModel>)sonuc.Result.Data;
                    string fileName = "Proje_" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + "_" + lgm.id + ".xlsx";
                    excelUrl = ExcelTabloExport.Export(ListToDatatable.ToDataTable<ProjeSurecModel>(dataList), System.Web.HttpContext.Current, fileName);
                }
                else if (tableName.Equals("proje_surec") && tur == ProjeSurecTur.surec)
                {

                }
                else if (tableName.Equals("gorevler"))
                {

                }
                return Json(JsonSonuc.sonucUret(true, excelUrl), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "Excel dosyası oluşturulurken bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Excel Export Table

        #region mail ve sms fonksiyonları
        [AreaAuthorize("Yonetici", "")]
        public ActionResult Mailler()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<object> nesneler = new List<object>();
            string query = "select count(m.id) as count, m.* from mailler as m "
                + "where m.mail_grup_id > 0 and m.firma_id = " + lgm.firma_id + " group by mail_grup_id  order by m.date desc;";
            List<MaillerCountModel> mailList = db.Database.SqlQuery<MaillerCountModel>(query).ToList();
            nesneler.Add(mailList);
            return View(nesneler);
        }
        [AreaAuthorize("Yonetici", "")]
        public async Task<ActionResult> TopluMailGonder(string id)
        {
            try
            {
                if (id == null)
                {
                    id = "-200";
                }
                int intId = Convert.ToInt32(id);
                List<object> nesneler = new List<object>();
                string query = "select m.id, m.firma_adi as text from musteriler as m "
                + "where m.flag = " + durumlar.aktif + " order by m.firma_id;";
                string query2 = "select hedef_id from mailler as m where m.mail_grup_id = " + id;
                var k = db.Database.SqlQuery<IdTextPair>(query).ToListAsync();
                var m = db.mailler.Where(e => e.flag == durumlar.aktif && e.mail_grup_id == intId).FirstOrDefaultAsync();
                var k2 = db.Database.SqlQuery<int>(query2).ToListAsync();
                await Task.WhenAll(k, m, k2);
                List<IdTextPair> musteriList = k.Result;
                mailler mail = m.Result;
                List<int> kullaniciIdList = k2.Result;
                nesneler.Add(musteriList);
                nesneler.Add(mail);
                nesneler.Add(kullaniciIdList);
                return View(nesneler);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Mailler");
            }
        }
        public JsonResult mailGonderToplu(string[] musteriList)
        {
            try
            {
                string icerik = Request.Unvalidated["icerik"];
                string konu = Request["konu"];
                int groupId = EmailFunctions.getGroupId();
                foreach (string str in musteriList)
                {
                    int userId = Convert.ToInt32(str);
                    musteriler mstr = db.musteriler.Where(e => e.id == userId).FirstOrDefault();
                    if (mstr != null)
                    {
                        bool mailSonuc = EmailFunctions.sendEmailGmail(icerik, konu, mstr.email, MailHedefTur.musteri, mstr.id, EmailFunctions.mailAdresi, 0, "", "", "", "", groupId);
                    }
                }
                return Json(JsonSonuc.sonucUret(true, "Mail Gönderildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }

        [AreaAuthorize("Yonetici", "")]
        public async Task<ActionResult> Smsler()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            List<object> nesneler = new List<object>();
            string query = "select count(s.id) as count, s.* from smsler as s  "
                + "where s.sms_grup_id > 0 and s.firma_id = " + lgm.firma_id + " group by sms_grup_id;";
            List<SmslerCountModel> mailList = db.Database.SqlQuery<SmslerCountModel>(query).ToList();
            nesneler.Add(mailList);

            nesneler.Add(SendSms.smsHakkiSorgula(lgm.fm.musteri_no));

            return View(nesneler);
        }
        [AreaAuthorize("Yonetici", "")]
        public async Task<ActionResult> TopluSmsGonder(string id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                if (id == null)
                {
                    id = "-200";
                }
                int intId = Convert.ToInt32(id);
                List<object> nesneler = new List<object>();
                string query = "select m.id, m.firma_adi as text from musteriler as m "
                + "where m.flag = " + durumlar.aktif + " order by m.firma_id;";
                string query2 = "select hedef_id from smsler as s where s.sms_grup_id = " + id;
                var k = db.Database.SqlQuery<IdTextPair>(query).ToListAsync();
                var s = db.smsler.Where(e => e.flag == durumlar.aktif && e.sms_grup_id == intId).FirstOrDefaultAsync();
                var k2 = db.Database.SqlQuery<int>(query2).ToListAsync();
                await Task.WhenAll(k, s, k2);
                List<IdTextPair> musteriList = k.Result;
                smsler sms = s.Result;
                List<int> kullaniciIdList = k2.Result;
                nesneler.Add(musteriList);
                nesneler.Add(sms);
                nesneler.Add(kullaniciIdList);
                nesneler.Add(SendSms.smsHakkiSorgula(lgm.fm.musteri_no));
                return View(nesneler);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Smsler");
            }
        }
        [AreaAuthorize("Yonetici", "")]
        public JsonResult smsGonderToplu(string[] musteriList)
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
                List<musteriler> userList = new List<musteriler>();
                foreach (string str in musteriList)
                {
                    int mstrId = Convert.ToInt32(str);
                    musteriler mstr = db.musteriler.Where(e => e.id == mstrId).FirstOrDefault();
                    if (mstr != null)
                    {
                        numaraList.Add(mstr.gsm);
                        userList.Add(mstr);
                    }
                }
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                SendSms sms = new SendSms();
                firma_musavir fm = db.firma_musavir.Where(e => e.id == lgm.firma_id).FirstOrDefault();
                bool sonuc = sms.SendSMS(numaraList.ToArray(), icerik, fm.sms_header, lgm.fm.musteri_no);
                if (sonuc == false)
                {
                    return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
                }
                foreach (musteriler mstr in userList)
                {
                    SendSms.smsKaydet(icerik, durumlar.aktif, MailHedefTur.musteri, mstr.id, mstr.gsm, lgm.id, groupId);
                }
                return Json(JsonSonuc.sonucUret(true, "Sms Gönderildi."), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz."), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion mail ve sms fonksiyonları
    }
}