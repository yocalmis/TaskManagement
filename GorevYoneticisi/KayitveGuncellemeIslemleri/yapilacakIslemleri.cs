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
    public class yapilacakIslemleri
    {
        public JsonSonuc yeniYapilacak(int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

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


                yplck.firma_id = firma_id;
                yplck.tamamlanma_tarihi = DateTime.Now;
                yplck.flag = durumlar.aktif;
                yplck.date = DateTime.Now;
                yplck.ekleyen = GetCurrentUser.GetUser().id;
                yplck.durum = YapilacaklarDurum.beklemede;

                return yapilacakEkle(yplck);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc yapilacakDuzenle(string url, int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                yapilacaklar dbPrj = db.yapilacaklar.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi).FirstOrDefault();

                if (dbPrj == null || url == null || url.Equals(""))
                {
                    return yeniYapilacak(firma_id, Request);
                }
                else if (!(dbPrj.flag != durumlar.silindi))
                {
                    return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                }

                string urlTemp = dbPrj.url;

                foreach (var property in dbPrj.GetType().GetProperties())
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
                            PropertyInfo propertyS = dbPrj.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(dbPrj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(dbPrj, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                dbPrj.url = urlTemp;

                db.Entry(dbPrj).State = EntityState.Modified;
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, dbPrj.url);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc silYapilacak(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();               
                yapilacaklar yplck = db.yapilacaklar.Where(e => e.url.Equals(url)).FirstOrDefault();
                int eskiYuzde = yplck.gorevler.yuzde;
                yplck.flag = durumlar.silindi;
                db.Entry(yplck).State = EntityState.Modified;
                db.SaveChanges();
                gorevIslemleri gis = new gorevIslemleri();
                JsonSonuc sonuc = gis.gorevYuzdesiDuzenle(yplck.gorev_id);
                gorevIslemleri.logEkle(yplck.gorevler, "Görev tamamlama yüzdesi " + eskiYuzde + "'den " + sonuc.Message + "e getirildi. \"" + yplck.isim + "\" işlemi görevden silindi.", GetCurrentUser.GetUser());

                return JsonSonuc.sonucUret(true, "Yapılacaklar listesi düzenlendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc yapilacakPasiflestir(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                yapilacaklar yplck = db.yapilacaklar.Where(e => e.url.Equals(url)).FirstOrDefault();
                int eskiYuzde = yplck.gorevler.yuzde;
                yplck.durum = YapilacaklarDurum.pasif;
                db.Entry(yplck).State = EntityState.Modified;
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, "Yapılacaklar listesi düzenlendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc yapilacakAktiflestir(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                yapilacaklar yplck = db.yapilacaklar.Where(e => e.url.Equals(url)).FirstOrDefault();
                int eskiYuzde = yplck.gorevler.yuzde;
                yplck.durum = YapilacaklarDurum.beklemede;
                db.Entry(yplck).State = EntityState.Modified;
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, "Yapılacaklar listesi düzenlendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc yapilacaklariGetir(string gorev_url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                //gorevler gorev = db.gorevler.Where(e => e.flag == durumlar.aktif && e.url.Equals(gorev_url)).FirstOrDefault();

                string queryGorevCount = "select y.* from yapilacaklar as y "
                     + "inner join gorevler as g on g.id = y.gorev_id and g.flag = "+ durumlar.aktif + " and g.url = '" + gorev_url + "' "
                     + "where y.flag = " + durumlar.aktif;
                List<GorevlerModel> gorevList = db.Database.SqlQuery<GorevlerModel>(queryGorevCount).ToList();

                JsonSonuc sonuc = JsonSonuc.sonucUret(true, gorevList);
                return sonuc;
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "Yapılacaklar getirilirken bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc yapilacakDurum(string url, int durum)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                yapilacaklar yplck = db.yapilacaklar.Where(e => e.url.Equals(url)).FirstOrDefault();
                yplck.durum = durum;
                db.Entry(yplck).State = EntityState.Modified;
                db.SaveChanges();
                gorevIslemleri gis = new gorevIslemleri();
                int eskiYuzde = yplck.gorevler.yuzde;
                JsonSonuc sonuc = gis.gorevYuzdesiDuzenle(yplck.gorev_id);

                string islemText = "tamamlandı olarak işaretlendi.";
                if (yplck.durum == YapilacaklarDurum.beklemede)
                {
                    islemText = "tamamlanmadı olarak işaretlendi.";
                }

                gorevIslemleri.logEkle(yplck.gorevler, "Görev tamamlama yüzdesi " + eskiYuzde + "'den " + sonuc.Message + "e getirildi. \"" + yplck.isim + "\" işlemi " + islemText, GetCurrentUser.GetUser());

                return sonuc;
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }

        public JsonSonuc yapilacakEkle(yapilacaklar yplck)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.yapilacaklar.Count() != 0)
                {
                    vid = db.yapilacaklar.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.yapilacaklar.Count() != 0)
                {
                    sort = db.yapilacaklar.Max(e => e.sort) + 1;
                }

                string strImageName = StringFormatter.OnlyEnglishChar(yplck.isim);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                yapilacaklar pg = new yapilacaklar();
                do
                {
                    pg = db.yapilacaklar.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                yplck.url = createdUrl;
                yplck.vid = vid;
                yplck.sort = sort;

                db.yapilacaklar.Add(yplck);
                db.SaveChanges();

                int eskiYuzde = db.gorevler.Where(e => e.id == yplck.gorev_id).FirstOrDefault().yuzde;
                gorevIslemleri gis = new gorevIslemleri();
                JsonSonuc sonuc = gis.gorevYuzdesiDuzenle(yplck.gorev_id);
                
                gorevIslemleri.logEkle(yplck.gorevler, "Görev tamamlama yüzdesi " + eskiYuzde + "'den " + sonuc.Message + "e getirildi. \"" + yplck.isim + "\" işlemi göreve eklendi.", GetCurrentUser.GetUser());

                return JsonSonuc.sonucUret(true, yplck.url);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }

        
    }
}