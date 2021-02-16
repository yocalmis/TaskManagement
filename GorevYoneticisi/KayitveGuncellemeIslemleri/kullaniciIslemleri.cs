using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class kullaniciIslemleri
    {
        public string yeniKullanici(string password, string password_control, string mail_permission, string sms_permission, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int mailPermission = Permissions.granted;
                if (mail_permission == null)
                {
                    mailPermission = Permissions.denied;
                }
                int smsPermission = Permissions.granted;
                if (sms_permission == null)
                {
                    smsPermission = Permissions.denied;
                }

                int vid = 1;
                if (db.kullanicilar.Count() != 0)
                {
                    vid = db.kullanicilar.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.kullanicilar.Count() != 0)
                {
                    sort = db.kullanicilar.Max(e => e.sort) + 1;
                }

                kullanicilar user = new kullanicilar();
                foreach (var property in user.GetType().GetProperties())
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
                            PropertyInfo propertyS = user.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(user, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(user, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(user, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(user, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(user.ad + " " + user.soyad);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                kullanicilar pg = new kullanicilar();
                do
                {
                    pg = db.kullanicilar.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                user.url = createdUrl;

                user.mail_permission = mailPermission;
                user.sms_permission = smsPermission;
                user.password = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
                user.flag = durumlar.emailOnayBekliyor;
                user.date = DateTime.Now;
                user.vid = vid;
                user.reset_guidexpiredate = DateTime.Now.AddDays(5);
                user.ekleyen = GetCurrentUser.GetUser().id;
                //user.kullanici_turu = KullaniciTurleri.super_admin;
                user.sort = sort;
                user.mail_host = "";
                user.mail_port = "";
                user.mail_ssl = "";
                user.mail_psw = "";

                Guid gd = new Guid();
                gd = Guid.NewGuid();

                user.reset_guid = gd.ToString();

                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int incelenecekKullaniciTurleri = lgm.kullanici_turu;
                if (lgm.kullanici_turu == KullaniciTurleri.super_admin)
                {
                    incelenecekKullaniciTurleri = 0;
                }
                if (!(user.kullanici_turu > incelenecekKullaniciTurleri))
                {
                    return "";
                }

                bool kullaniciKontrol = firmaKullaniciKontrol(user.firma_id, user.id).Result;
                if (!kullaniciKontrol)
                {
                    return "kullanici_sayisi_hatasi";
                }

                db.kullanicilar.Add(user);
                db.SaveChanges();

                string icerik = "<div>Üyeliğiniz Oluşturulmuştur.</div>"
                + "<div>Üyeliğiniz başarı ile oluşturulmuştur. Aşağıdaki onay linkine tıklayarak üyeliğinizi onaylayailrsiniz. Bizi tercih ettiğiniz için teşekkür ederiz.</div> <div><a href=\"" + config.url + "EmailOnay/" + user.reset_guid + "\">E-mail adresinizi onaylamak için tıklayınız</a></div>";
                string baslik = config.projeİsmi + " Üyeliği E-mail Onay";

                EmailFunctions.sendEmailGmail(icerik, baslik, user.email, MailHedefTur.kullanici, user.id, EmailFunctions.mailAdresi, 0, "", "", "", "", -3);

                return user.url;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("email_unique"))
                {
                    return "email_unique";
                }
                else if (e.InnerException != null && e.InnerException.ToString().Contains("username_unique"))
                {
                    return "username_unique";
                }
                else
                {
                    return "";
                }
            }
        }
        public string kullaniciDuzenle(string url, string password, string password_control, string mail_permission, string sms_permission, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int mailPermission = Permissions.granted;
                if (mail_permission == null)
                {
                    mailPermission = Permissions.denied;
                }
                int smsPermission = Permissions.granted;
                if (sms_permission == null)
                {
                    smsPermission = Permissions.denied;
                }

                LoggedUserModel lgm = GetCurrentUser.GetUser();
                int incelenecekKullaniciTurleri = lgm.kullanici_turu;
                if (lgm.kullanici_turu == KullaniciTurleri.super_admin)
                {
                    incelenecekKullaniciTurleri = 0;
                }

                kullanicilar dbUser = db.kullanicilar.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi && (e.id == lgm.id || e.kullanici_turu > incelenecekKullaniciTurleri)).FirstOrDefault();

                if (dbUser == null || url == null || url.Equals(""))
                {
                    string firmaId = Request["firma_id"].ToString();
                    if (!firmaId.Equals(lgm.firma_id.ToString()))
                    {
                        return "";
                    }
                    return yeniKullanici(password, password_control, mail_permission, sms_permission, Request);
                }
                else if (!(dbUser.flag != durumlar.silindi))
                {
                    return "";
                }

                string passwordTemp = dbUser.password;
                string urlTemp = dbUser.url;
                
                //kullanicilar user = new kullanicilar();
                foreach (var property in dbUser.GetType().GetProperties())
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
                            PropertyInfo propertyS = dbUser.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(dbUser, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(dbUser, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                if (!password.Trim().Equals(""))
                {
                    dbUser.password = HashWithSha.ComputeHash(password, "SHA512", Encoding.ASCII.GetBytes(password));
                }
                else
                {
                    dbUser.password = passwordTemp;
                }
                dbUser.url = urlTemp;

                if (!(dbUser.id == lgm.id || dbUser.kullanici_turu > incelenecekKullaniciTurleri))
                {
                    return "";
                }

                bool kullaniciKontrol = firmaKullaniciKontrol(dbUser.firma_id, dbUser.id).Result;
                if (!kullaniciKontrol)
                {
                    return "kullanici_sayisi_hatasi";
                }

                dbUser.mail_permission = mailPermission;
                dbUser.sms_permission = smsPermission;

                db.Entry(dbUser).State = EntityState.Modified;
                db.SaveChanges();

                if (dbUser.id == GetCurrentUser.GetUser().id)
                {
                    kullaniciIslemleri ki = new kullaniciIslemleri();
                    ki.resetLoginInfo();
                }

                return dbUser.url;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("email_unique"))
                {
                    return "email_unique";
                }
                else if (e.InnerException != null && e.InnerException.ToString().Contains("username_unique"))
                {
                    return "username_unique";
                }
                else
                {
                    return "";
                }
            }
        }
        public void resetLoginInfo()
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            int id = GetCurrentUser.GetUser().id;
            FormsAuthentication.SignOut();
            kullanicilar usr = db.kullanicilar.Where(e => e.id == id).FirstOrDefault();
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
            }            
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
        public static async Task<bool> firmaKullaniciKontrol(int firma_id, int user_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            if (firma_id == 0)
            {
                return true;
            }
            /*if (user_id == 0)
            {
                var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
                string queryKullaniciCount = "select count(id) from firma_musavir where flag = " + durumlar.aktif.ToString() + " and firma_id = " + firma_id.ToString();
                var kc = db.Database.SqlQuery<int>(queryKullaniciCount).FirstOrDefaultAsync();

                await Task.WhenAll(f, kc);

                firma_musavir fm = f.Result;
                int kullaniciCount = kc.Result;

                if (fm == null)
                {
                    return false;
                }
                else if(fm.kullanici_sayisi < kullaniciCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {*/
                var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
                string queryKullaniciCount = "select count(id) from kullanicilar where (flag = " + durumlar.emailOnayBekliyor.ToString() + " or flag = " + durumlar.aktif.ToString() + ") and firma_id = " + firma_id.ToString() + " and id != " + user_id;
                var kc = db.Database.SqlQuery<int>(queryKullaniciCount).FirstOrDefaultAsync();

                await Task.WhenAll(f, kc);

                firma_musavir fm = f.Result;
                int kullaniciCount = kc.Result;

                if (fm == null)
                {
                    return false;
                }
                else if (fm.kullanici_sayisi > kullaniciCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            //}
        }
        public static List<KullaniciProjeOzetModel> getFirmaKullanicilariOzet()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select k.url, k.ad, k.soyad, 0 as id, k.id as kullanici_id from kullanicilar as k "
                + "where k.flag = 1 and k.firma_id = " + lgm.firma_id;
            List<KullaniciProjeOzetModel> kpList = db.Database.SqlQuery<KullaniciProjeOzetModel>(pkQuery).ToList();
            return kpList;
        }
    }
}