using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class mesajIslemleri
    {
        public JsonSonuc yeniMesaj(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();

                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.kullanici_proje.Count() != 0)
                {
                    vid = db.kullanici_proje.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.kullanici_proje.Count() != 0)
                {
                    sort = db.kullanici_proje.Max(e => e.sort) + 1;
                }

                mesajlar msj = new mesajlar();
                foreach (var property in msj.GetType().GetProperties())
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
                            PropertyInfo propertyS = msj.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(msj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(msj, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(msj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(msj, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(OurFunctions.ourSubString(msj.mesaj, 20));
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                mesajlar pg = new mesajlar();
                do
                {
                    pg = db.mesajlar.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                msj.url = createdUrl;

                msj.flag = durumlar.bekliyor;
                msj.date = DateTime.Now;
                msj.vid = vid;
                msj.sort = sort;
                msj.gonderen_id = lgm.id;
                msj.firma_id = lgm.firma_id;

                if (msj.parent_url.Equals(string.Empty))
                {
                    msj.parent_url = msj.url;
                }
                
                db.mesajlar.Add(msj);
                db.SaveChanges();


                mesajlar dbMsj = db.mesajlar.Where(e => e.vid == msj.vid).FirstOrDefault();
                if (dbMsj != null)
                {
                    bildirimIslemleri.yeniBildirim(dbMsj.alan_id, BildirimTurleri.mesaj, dbMsj.id, dbMsj.url, lgm.ad + " " + lgm.soyad + " isimli isimli kullanıcıdan yeni bir mesajınız var.");
                } 

                return JsonSonuc.sonucUret(true, "Mesaj Gönderildi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public static JsonSonuc okunduIsaretle(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                string queryGorevCount = "update mesajlar set flag = " + durumlar.aktif + " where parent_url = '" + url + "'";
                db.Database.ExecuteSqlCommand(queryGorevCount);

                return JsonSonuc.sonucUret(true, "Mesaj Gönderildi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
    }
}