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
    public class musteriKullaniciIslemleri
    {
        public JsonSonuc yenimusteriKullanicisi(HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                LoggedUserModel lgm = GetCurrentUser.GetUser();

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

                kullanici_musteri km = new kullanici_musteri();
                foreach (var property in km.GetType().GetProperties())
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
                            PropertyInfo propertyS = km.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(km, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(km, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(km, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(km, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                km.flag = durumlar.aktif;
                km.date = DateTime.Now;
                km.vid = vid;
                km.sort = sort;
                km.ekleyen = lgm.id;

                kullanici_musteri dbKm = db.kullanici_musteri.Where(e => e.flag == durumlar.aktif && e.musteri_id == km.musteri_id && e.kullanici_id == km.kullanici_id).FirstOrDefault();
                if (dbKm != null)
                {
                    return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
                }                

                db.kullanici_musteri.Add(km);
                db.SaveChanges();

                musteriler mstr = db.musteriler.Where(e => e.id == km.musteri_id).FirstOrDefault();
                if (mstr != null)
	            {
                    bildirimIslemleri.yeniBildirim(km.kullanici_id, BildirimTurleri.musteri, km.musteri_id, mstr.url, mstr.firma_adi + " firma isimli müşteri ilgilenmeniz için size yönlendirildi. Yönlendiren yetkili " + lgm.ad + " " + lgm.soyad + ".");
	            }                

                return JsonSonuc.sonucUret(true, "Kullanıcı Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc musteriKullanicisiSil(int id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                kullanici_musteri km = db.kullanici_musteri.Where(e => e.id.Equals(id)).FirstOrDefault();
                km.flag = durumlar.silindi;
                db.Entry(km).State = EntityState.Modified;
                if (km.musteriler != null)
                {
                    bildirimIslemleri.yeniBildirim(km.kullanici_id, BildirimTurleri.musteri, km.musteri_id, "", km.musteriler.firma_adi + " firma isimli müşteri sizden alındı. İşlemi yapan yetkili " + lgm.ad + " " + lgm.soyad + ".");
                }  
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public static List<KullaniciProjeOzetModel> getMusteriKullanicilarOzet(int musteri_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select k.ad, k.soyad, km.id from musteriler as m "
                + "inner join kullanici_musteri as km on km.musteri_id = m.id "
                + "inner join kullanicilar as k on k.id = km.kullanici_id "
                + "where m.flag = 1 and km.flag = 1 and k.flag = 1 and m.id = " + musteri_id.ToString();
            List<KullaniciProjeOzetModel> kmList = db.Database.SqlQuery<KullaniciProjeOzetModel>(pkQuery).ToList();
            return kmList;
        }
    }
}