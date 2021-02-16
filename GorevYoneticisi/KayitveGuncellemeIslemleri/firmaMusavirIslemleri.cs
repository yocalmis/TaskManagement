using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class firmaMusavirIslemleri
    {
        public string yeniFirmaMusavir(HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.firma_musavir.Count() != 0)
                {
                    vid = db.firma_musavir.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.firma_musavir.Count() != 0)
                {
                    sort = db.firma_musavir.Max(e => e.sort) + 1;
                }

                firma_musavir fm = new firma_musavir();
                foreach (var property in fm.GetType().GetProperties())
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
                            PropertyInfo propertyS = fm.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(fm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(fm, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(fm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(fm, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(fm.firma_adi);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                firma_musavir pg = new firma_musavir();
                do
                {
                    pg = db.firma_musavir.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                fm.url = createdUrl;

                fm.flag = durumlar.aktif;
                fm.date = DateTime.Now;
                fm.vid = vid;
                fm.sort = sort;
                fm.isim = "";
                fm.ekleyen = GetCurrentUser.GetUser().id;

                db.firma_musavir.Add(fm);
                db.SaveChanges();

                return fm.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string firmaMusavirDuzenle(string url, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                firma_musavir dbfm = db.firma_musavir.Where(e => e.url.Equals(url)).FirstOrDefault();
                string dbUrlTemp = dbfm.url;

                //firma_musavir fm = new firma_musavir();
                foreach (var property in dbfm.GetType().GetProperties())
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
                            PropertyInfo propertyS = dbfm.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(dbfm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(dbfm, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                 dbfm.url = dbUrlTemp;

                db.Entry(dbfm).State = EntityState.Modified;
                db.SaveChanges();

                if (dbfm.id == GetCurrentUser.GetUser().firma_id)
                {
                    kullaniciIslemleri ki = new kullaniciIslemleri();
                    ki.resetLoginInfo();
                }

                return dbfm.url;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("email_unique"))
                {
                    return "email_unique";
                }
                else
                {
                    return "";
                }
            }
        }
    }
}