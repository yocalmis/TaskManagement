using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class gorevKullanicisiIslemleri
    {
        public JsonSonuc yeniGorevKullanicisi(HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();

                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.kullanici_gorev.Count() != 0)
                {
                    vid = db.kullanici_gorev.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.kullanici_gorev.Count() != 0)
                {
                    sort = db.kullanici_gorev.Max(e => e.sort) + 1;
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

                kg.flag = durumlar.aktif;
                kg.date = DateTime.Now;
                kg.vid = vid;
                kg.sort = sort;
                kg.ekleyen = lgm.id;

                kullanici_gorev dbKg = db.kullanici_gorev.Where(e => e.flag == durumlar.aktif && e.gorev_id == kg.gorev_id && e.kullanici_id == kg.kullanici_id).FirstOrDefault();
                if (dbKg != null)
                {
                    return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
                }

                db.kullanici_gorev.Add(kg);
                db.SaveChanges();

                gorevler grv = db.gorevler.Where(e => e.id == kg.gorev_id).FirstOrDefault();
                if (grv != null)
                {
                    bildirimIslemleri.yeniBildirim(kg.kullanici_id, BildirimTurleri.gorev, kg.gorev_id, grv.url, grv.isim + " isimli görev ilgilenmeniz için size yönlendirildi. Yönlendiren yetkili " + lgm.ad + " " + lgm.soyad + ".");
                }  

                return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc gorevKullanicisiSil(int id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                kullanici_gorev kg = db.kullanici_gorev.Where(e => e.id.Equals(id)).FirstOrDefault();
                kg.flag = durumlar.silindi;
                db.Entry(kg).State = EntityState.Modified;

                if (kg.gorevler != null)
                {
                    bildirimIslemleri.yeniBildirim(kg.kullanici_id, BildirimTurleri.gorev, kg.gorev_id, "", kg.gorevler.isim + " isimli görev sizden alındı. İşlemi yapan yetkili " + lgm.ad + " " + lgm.soyad + ".");
                } 

                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public static List<KullaniciProjeOzetModel> getGorevKullanicilarOzet(int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select k.ad, k.soyad, kp.id, k.url, k.id as kullanici_id from gorevler as g "
                + "inner join kullanici_gorev as kp on kp.gorev_id = g.id "
                + "inner join kullanicilar as k on k.id = kp.kullanici_id "
                + "where g.flag = 1 and kp.flag = 1 and k.flag = 1 and g.id = " + gorev_id.ToString();
            List<KullaniciProjeOzetModel> kpList = db.Database.SqlQuery<KullaniciProjeOzetModel>(pkQuery).ToList();
            return kpList;
        }
    }
}