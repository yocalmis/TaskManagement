using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;

namespace GorevYoneticisi.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpPost]
        public async Task<CevapModel> KullaniciLogin()
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                var context = new HttpContextWrapper(HttpContext.Current);
                HttpRequestBase request = context.Request;

                string gelenJson;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        gelenJson = readStream.ReadToEnd();
                    }
                }

                KullanicilarModelServis kullanici = JsonConvert.DeserializeObject<KullanicilarModelServis>(gelenJson);
                kullanici.password = HashWithSha.ComputeHash(kullanici.password, "SHA512", Encoding.ASCII.GetBytes(kullanici.password));

                kullanicilar dbKullanici = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.email.Equals(kullanici.email) && e.password.Equals(kullanici.password)).FirstOrDefault();
                if (dbKullanici == null)
                {
                    return CreateCevap.cevapOlustur(false, "Yanlış E-mail ya da şifre. Lütfen girdiğiniz bilgileri kontrol ederek tekrar deneyiniz.", null);
                }

                KullanicilarModelServis ym = new KullanicilarModelServis();
                foreach (var property in ym.GetType().GetProperties())
                {
                    try
                    {
                        var response = dbKullanici.GetType().GetProperty(property.Name).GetValue(dbKullanici, null).ToString();
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = ym.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(ym, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(ym, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(ym, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(ym, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }
                firma_musavir fm = dbKullanici.firma_musavir.FirstOrDefault();
                if (fm != null)
	            {
		            ym.konum_periyot = fm.konum_periyot;
	            }
                else
	            {
                    ym.konum_periyot = 1;
	            }                

                return CreateCevap.cevapOlustur(true, "", ym);
            }
            catch (Exception ex)
            {
                return CreateCevap.cevapOlustur(false, "Bir hata oluştu. Lütfen tekrar deneyiniz", null);
            }
        }
        [HttpPost]
        public async Task<CevapModel> takipNoktasiEkle()
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                var context = new HttpContextWrapper(HttpContext.Current);
                HttpRequestBase request = context.Request;

                string gelenJson;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        gelenJson = readStream.ReadToEnd();
                    }
                }

                saha_takip stkp = JsonConvert.DeserializeObject<saha_takip>(gelenJson);

                stkp.flag = durumlar.aktif;
                stkp.date = DateTime.Now;

                int vid = 1;
                if (db.saha_takip.Count() != 0)
                {
                    vid = db.saha_takip.Max(e => e.vid) + 1;
                }
                stkp.vid = vid;

                db.saha_takip.Add(stkp);
                db.SaveChanges();

                return CreateCevap.cevapOlustur(true, "", stkp);
            }
            catch (Exception ex)
            {
                return CreateCevap.cevapOlustur(false, "Bir hata oluştu. Lütfen tekrar deneyiniz", null);
            }
        }
    }
}