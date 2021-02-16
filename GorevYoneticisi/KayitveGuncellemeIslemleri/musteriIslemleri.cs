using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class musteriIslemleri
    {
        public string yeniMusteri(int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

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

                musteriler mstr = new musteriler();
                foreach (var property in mstr.GetType().GetProperties())
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
                            PropertyInfo propertyS = mstr.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(mstr, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(mstr, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(mstr, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(mstr, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(mstr.ad + " " + mstr.soyad);
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
                mstr.url = createdUrl;
                mstr.firma_id = firma_id;

                mstr.flag = durumlar.aktif;
                mstr.date = DateTime.Now;
                mstr.vid = vid;
                mstr.ekleyen = GetCurrentUser.GetUser().id;
                mstr.sort = sort;
                mstr.firma = "";

                bool kullaniciKontrol = firmaMusteriKontrol(mstr.firma_id, mstr.id).Result;
                if (!kullaniciKontrol)
                {
                    return "musteri_sayisi_hatasi";
                }

                db.musteriler.Add(mstr);
                db.SaveChanges();

                return mstr.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string musteriDuzenle(string url, int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                musteriler dbMstr = db.musteriler.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi).FirstOrDefault();

                if (dbMstr == null || url == null || url.Equals(""))
                {
                    return yeniMusteri(firma_id, Request);
                }
                else if (!(dbMstr.flag != durumlar.silindi))
                {
                    return "";
                }

                string urlTemp = dbMstr.url;

                foreach (var property in dbMstr.GetType().GetProperties())
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
                            PropertyInfo propertyS = dbMstr.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(dbMstr, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(dbMstr, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                dbMstr.url = urlTemp;

                bool kullaniciKontrol = firmaMusteriKontrol(dbMstr.firma_id, dbMstr.id).Result;
                if (!kullaniciKontrol)
                {
                    return "musteri_sayisi_hatasi";
                }

                db.Entry(dbMstr).State = EntityState.Modified;
                db.SaveChanges();

                if (dbMstr.id == GetCurrentUser.GetUser().id)
                {
                    kullaniciIslemleri ki = new kullaniciIslemleri();
                    ki.resetLoginInfo();
                }

                return dbMstr.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static async Task<bool> firmaMusteriKontrol(int firma_id, int musteri_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            if (firma_id == 0)
            {
                return true;
            }
            var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
            string queryMusteriCount = "select count(id) from musteriler where flag != " + durumlar.silindi.ToString() + " and firma_id = " + firma_id.ToString() + " and id != " + musteri_id;
            var kc = db.Database.SqlQuery<int>(queryMusteriCount).FirstOrDefaultAsync();

            await Task.WhenAll(f, kc);

            firma_musavir fm = f.Result;
            int musteriCount = kc.Result;

            if (fm == null)
            {
                return false;
            }
            else if (fm.musteri_sayisi > musteriCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static List<MusteriProjeOzetModel> getFirmaMusterilerOzet()
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select 0 as id, m.ad, m.soyad, m.firma_adi, '' as kUrl, m.id as musteri_id from musteriler as m "
                + "where m.flag = 1 and m.firma_id = " + lgm.firma_id + ";";
            List<MusteriProjeOzetModel> mpList = db.Database.SqlQuery<MusteriProjeOzetModel>(pkQuery).ToList();
            return mpList;
        }
    }
}