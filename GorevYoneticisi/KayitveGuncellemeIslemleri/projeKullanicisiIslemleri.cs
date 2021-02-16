using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class projeKullanicisiIslemleri
    {
        public JsonSonuc yeniProjeKullanicisi(HttpRequestBase Request)
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

                kp.flag = durumlar.aktif;
                kp.date = DateTime.Now;
                kp.vid = vid;
                kp.sort = sort;
                kp.ekleyen = lgm.id;

                kullanici_proje dbKp = db.kullanici_proje.Where(e => e.flag == durumlar.aktif && e.proje_id == kp.proje_id && e.kullanici_id == kp.kullanici_id).FirstOrDefault();
                if (dbKp != null)
                {
                    return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
                }

                db.kullanici_proje.Add(kp);
                db.SaveChanges();

                proje_surec ps = db.proje_surec.Where(e => e.id == kp.proje_id).FirstOrDefault();
                if (ps != null)
                {
                    if (ps.tur == ProjeSurecTur.proje)
                    {
                        bildirimIslemleri.yeniBildirim(kp.kullanici_id, BildirimTurleri.proje, kp.proje_id, ps.url, ps.isim + " isimli proje ilgilenmeniz için size yönlendirildi. Yönlendiren yetkili " + lgm.ad + " " + lgm.soyad + ".");                        
                    }
                    else if (ps.tur == ProjeSurecTur.surec)
                    {
                        bildirimIslemleri.yeniBildirim(kp.kullanici_id, BildirimTurleri.surec, kp.proje_id, ps.url, ps.isim + " isimli süreç ilgilenmeniz için size yönlendirildi. Yönlendiren yetkili " + lgm.ad + " " + lgm.soyad + ".");
                    }                    
                }  

                return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc projeKullanicisiSil(int id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();

                vrlfgysdbEntities db = new vrlfgysdbEntities();
                kullanici_proje kp = db.kullanici_proje.Where(e => e.id.Equals(id)).FirstOrDefault();
                kp.flag = durumlar.silindi;
                db.Entry(kp).State = EntityState.Modified;

                if (kp.proje_surec != null)
                {
                    if (kp.proje_surec.tur == ProjeSurecTur.proje)
                    {
                        bildirimIslemleri.yeniBildirim(kp.kullanici_id, BildirimTurleri.proje, kp.proje_id, "", kp.proje_surec.isim + " isimli proje sizden alındı. İşlemi yapan yetkili " + lgm.ad + " " + lgm.soyad + ".");
                    }
                    else if (kp.proje_surec.tur == ProjeSurecTur.surec)
                    {
                        bildirimIslemleri.yeniBildirim(kp.kullanici_id, BildirimTurleri.surec, kp.proje_id, "", kp.proje_surec.isim + " isimli süreç sizden alındı. İşlemi yapan yetkili " + lgm.ad + " " + lgm.soyad + ".");
                    }
                } 
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public static List<KullaniciProjeOzetModel> getProjectSurecKullanicilarOzet(int proje_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select k.url, k.ad, k.soyad, kp.id, k.id as kullanici_id from proje_surec as pc "
                + "inner join kullanici_proje as kp on kp.proje_id = pc.id "
                + "inner join kullanicilar as k on k.id = kp.kullanici_id "
                + "where pc.flag = 1 and kp.flag = 1 and k.flag = 1 and pc.id = " + proje_id.ToString();
            List<KullaniciProjeOzetModel> kpList = db.Database.SqlQuery<KullaniciProjeOzetModel>(pkQuery).ToList();
            return kpList;
        }
    }
}