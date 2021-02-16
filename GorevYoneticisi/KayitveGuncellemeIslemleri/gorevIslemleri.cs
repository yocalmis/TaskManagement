using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class gorevIslemleri
    {
        /*public string yeniGorev(int firma_id, HttpRequestBase Request)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();

                vrlfgysdbEntities db = new vrlfgysdbEntities();

                string grvMultiply = Request["gorev_multiply"];
                int gorev_multiply = 0;
                if (grvMultiply != null)
                {
                    gorev_multiply = 1;
                }

                int vid = 1;
                if (db.gorevler.Count() != 0)
                {
                    vid = db.gorevler.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.gorevler.Count() != 0)
                {
                    sort = db.gorevler.Max(e => e.sort) + 1;
                }

                gorevler grv = new gorevler();
                foreach (var property in grv.GetType().GetProperties())
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
                            PropertyInfo propertyS = grv.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(grv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(grv, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(grv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(grv, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(grv.isim);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                gorevler pg = new gorevler();
                do
                {
                    pg = db.gorevler.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                grv.url = createdUrl;
                grv.firma_id = firma_id;

                grv.gorev_multiply = gorev_multiply;
                grv.flag = durumlar.aktif;
                grv.date = DateTime.Now;
                grv.vid = vid;
                grv.ekleyen = lgm.id;
                grv.sort = sort;
                grv.durum = TamamlamaDurumlari.bekliyor;
                grv.tamamlanma_tarihi = DateTime.Now;

                bool kullaniciKontrol = firmaGorevKontrol(grv.firma_id, grv.id).Result;
                if (!kullaniciKontrol)
                {
                    return "gorev_sayisi_hatasi";
                }

                int proje_id = Convert.ToInt32(Request["proje_id"].ToString());

                goreviEkle(grv, proje_id);

                return grv.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }*/
                
        public string gorevDuzenle(string url, int firma_id, HttpRequestBase Request, HttpServerUtilityBase Server)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                gorevler dbGrv = db.gorevler.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi).FirstOrDefault();

                string grvMultiply = Request["gorev_multiply"];
                int gorev_multiply = 0;
                if (grvMultiply != null)
                {
                    gorev_multiply = 1;
                }

                if (dbGrv == null || url == null || url.Equals(""))
                {
                    //return yeniGorev(firma_id, Request);
                }
                else if (!(dbGrv.flag != durumlar.silindi))
                {
                    return "";
                }

                gorev_proje gp1 = dbGrv.gorev_proje.Where(e => e.flag == durumlar.aktif).ElementAtOrDefault(0);
                int tempProjeId = 0;
                if (gp1 != null)
                {
                    tempProjeId = gp1.proje_id;
                }

                string urlTemp = dbGrv.url;

                foreach (var property in dbGrv.GetType().GetProperties())
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
                            PropertyInfo propertyS = dbGrv.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(dbGrv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else
                            {
                                propertyS.SetValue(dbGrv, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                dbGrv.url = urlTemp;
                dbGrv.gorev_multiply = gorev_multiply;

                if (dbGrv.id != 0)
                {
                    bool kullaniciKontrol = firmaGorevKontrol(dbGrv.firma_id, dbGrv.id).Result;
                    if (!kullaniciKontrol)
                    {
                        return "gorev_sayisi_hatasi";
                    } 
                }                

                db.Entry(dbGrv).State = EntityState.Modified;
                db.SaveChanges();

                #region proje gorev
                int proje_id = Convert.ToInt32(Request["proje_id"].ToString());
                gorev_proje gp = null;
                if (proje_id != 0)
                {
                    gp = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).FirstOrDefault();
                    if (gp == null)
                    {
                        gp = new gorev_proje();
                        gp.date = DateTime.Now;
                        gp.flag = durumlar.aktif;
                        gp.proje_id = proje_id;
                        gp.gorev_id = dbGrv.id;
                        int vidPm = 1;
                        if (db.gorev_proje.Count() != 0)
                        {
                            vidPm = db.gorev_proje.Max(e => e.vid) + 1;
                        }
                        int sortPm = 1;
                        if (db.gorev_proje.Count() != 0)
                        {
                            sortPm = db.gorev_proje.Max(e => e.sort) + 1;
                        }
                        gp.sort = sortPm;
                        gp.vid = vidPm;
                        db.gorev_proje.Add(gp);
                        db.SaveChanges();
                    }
                    else if (gp != null && gp.proje_id != proje_id)
                    {
                        gp.proje_id = proje_id;
                        db.Entry(gp).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else if (proje_id == 0)
                {
                    gp = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).FirstOrDefault();
                    if (gp != null)
                    {
                        gp.flag = durumlar.silindi;
                        db.Entry(gp).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                #endregion proje gorev

                gorevYuzdesiDuzenle(dbGrv.id);
                if (tempProjeId != 0 && proje_id != tempProjeId)
                {
                    projeIslemleri pis = new projeIslemleri();
                    pis.projeYuzdesiDuzenle(tempProjeId);
                }

                logEkle(dbGrv, "Görev bilgileri düzenlendi.", GetCurrentUser.GetUser());

                if (dbGrv.gorev_multiply == GorevMultiplyDurum.multiply)
                {
                    List<kullanici_gorev> kullaniciGorevList = db.kullanici_gorev.Where(e => e.gorev_id == dbGrv.id).ToList();                    
                    if (kullaniciGorevList.Count > 1)
                    {
                        multiplyGorev(dbGrv, Request, Server);
                    }
                }

                return dbGrv.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static async Task<bool> firmaGorevKontrol(int firma_id, int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            if (firma_id == 0)
            {
                return true;
            }
            var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
            string queryGorevCount = "select count(id) from gorevler where flag != " + durumlar.silindi.ToString() + " and firma_id = " + firma_id.ToString() + " and id != " + gorev_id;
            var kc = db.Database.SqlQuery<int>(queryGorevCount).FirstOrDefaultAsync();

            await Task.WhenAll(f, kc);

            firma_musavir fm = f.Result;
            int gorevCount = kc.Result;

            if (fm == null)
            {
                return false;
            }
            else if (fm.gorev_sayisi > gorevCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public JsonSonuc gorevYuzdesiDuzenle(int gorev_id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                gorevler gorev = db.gorevler.Where(e => e.id == gorev_id).FirstOrDefault();
                int eskiYuzde = gorev.yuzde;
                List<yapilacaklar> yapilacakList = db.yapilacaklar.Where(e => e.gorev_id == gorev.id && e.flag == durumlar.aktif && e.durum != YapilacaklarDurum.pasif).ToList();

                int tamamlananYapilacaklar = yapilacakList.Where(e => e.durum == YapilacaklarDurum.yapildi).Count();
                int bekleyenYapilacaklar = yapilacakList.Where(e => e.durum == YapilacaklarDurum.beklemede).Count();

                gorev.yuzde = (tamamlananYapilacaklar * 100) / yapilacakList.Count;
                
                gorev.durum = TamamlamaDurumlari.basladi;               

                db.Entry(gorev).State = EntityState.Modified;
                db.SaveChanges();

                if (gorev.yuzde == 100)
                {
                    List<kullanicilar> yetkiliList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.kullanici_turu <= KullaniciTurleri.firma_yetkili).ToList();
                    foreach (kullanicilar usr in yetkiliList)
                    {
                        bildirimIslemleri.yeniBildirim(usr.id, BildirimTurleri.gorev, gorev.id, "", gorev.isim + " isimli görev tamamlandı ve onayınızı bekliyor.");
                    }                    
                }

                projeIslemleri pis = new projeIslemleri();
                surecIslemleri sis = new surecIslemleri();
                if (gorev.gorev_proje != null)
                {
                    gorev_proje gp = gorev.gorev_proje.Where(e => e.flag == durumlar.aktif).ElementAtOrDefault(0);
                    if (gp != null)
                    {
                        if (gp.proje_surec.tur == ProjeSurecTur.proje)
	                    {
                            pis.projeYuzdesiDuzenle(gp.proje_id);
	                    }
                        else if (gp.proje_surec.tur == ProjeSurecTur.surec)
                        {
                            sis.surecYuzdesiDuzenle(gp.proje_id);
                        }                        
                    }
                }

                /*if (eskiYuzde != gorev.yuzde)
                {
                    logEkle(gorev, "Görev tamamlama yüzdesi " + eskiYuzde + "'den " + gorev.yuzde + "e getirildi.", GetCurrentUser.GetUser());
                }*/                

                return JsonSonuc.sonucUret(true, gorev.yuzde);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "Görev yüzdesini düzenlerken bir hata oluştu.");
            }
        }
        public JsonSonuc silGorev(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                gorevler grv = db.gorevler.Where(e => e.url.Equals(url)).FirstOrDefault();
                if (grv == null)
                {
                    return JsonSonuc.sonucUret(false, "Görev bulunamadı.");
                }
                grv.durum = TamamlamaDurumlari.pasif;
                db.Entry(grv).State = EntityState.Modified;
                db.SaveChanges();

                List<yapilacaklar> yapilacakList = db.yapilacaklar.Where(e => e.flag != durumlar.silindi && e.gorev_id == grv.id).ToList();
                yapilacakIslemleri yis = new yapilacakIslemleri();
                foreach (yapilacaklar yplck in yapilacakList)
                {
                    yis.yapilacakPasiflestir(yplck.url);
                }
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Görev silindi.");
        }
        public JsonSonuc gorevTamamlandi(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                gorevler grv = db.gorevler.Where(e => e.url.Equals(url)).FirstOrDefault();
                if (grv == null)
                {
                    return JsonSonuc.sonucUret(false, "Görev bulunamadı.");
                }
                if (grv.yuzde != 100)
                {
                    return JsonSonuc.sonucUret(false, "Görev yüzdesi %100 değil.");
                }
                grv.durum = TamamlamaDurumlari.tamamlandi;
                grv.onaylayan_yetkili = lgm.id;
                grv.tamamlanma_tarihi = DateTime.Now;

                List<gorev_baglanti> gbList = db.gorev_baglanti.Where(e => e.bagli_gorev == grv.id).ToList();

                db.Entry(grv).State = EntityState.Modified;

                foreach (gorev_baglanti gb in gbList)
                {
                    gorevler grvBagli = db.gorevler.Where(e => e.id == gb.gorev_id).FirstOrDefault();
                    grvBagli.durum = TamamlamaDurumlari.bekliyor;
                    db.Entry(grvBagli).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Görev durumu güncellendi.");
        }
        public JsonSonuc gorevAktiflestir(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                gorevler grv = db.gorevler.Where(e => e.url.Equals(url)).FirstOrDefault();
                grv.durum = TamamlamaDurumlari.basladi;
                db.Entry(grv).State = EntityState.Modified;
                db.SaveChanges();

                List<yapilacaklar> yapilacakList = db.yapilacaklar.Where(e => e.flag != durumlar.silindi && e.gorev_id == grv.id).ToList();
                yapilacakIslemleri yis = new yapilacakIslemleri();
                foreach (yapilacaklar yplck in yapilacakList)
                {
                    yis.yapilacakAktiflestir(yplck.url);
                }
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Görev durumu güncellendi.");
        }

        public JsonSonuc yeniGorevMusterisi(HttpRequestBase Request, string[] musteriList)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                foreach (string mstr in musteriList)
                {
                    int vid = 1;
                    if (db.gorev_musteri.Count() != 0)
                    {
                        vid = db.gorev_musteri.Max(e => e.vid) + 1;
                    }
                    int sort = 1;
                    if (db.gorev_musteri.Count() != 0)
                    {
                        sort = db.gorev_musteri.Max(e => e.sort) + 1;
                    }

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

                    kg.musteri_id = Convert.ToInt32(mstr);
                    kg.flag = durumlar.aktif;
                    kg.date = DateTime.Now;
                    kg.vid = vid;
                    kg.sort = sort;
                    kg.ekleyen = GetCurrentUser.GetUser().id;

                    gorev_musteri dbKg = db.gorev_musteri.Where(e => e.flag == durumlar.aktif && e.gorev_id == kg.gorev_id && e.musteri_id == kg.musteri_id && kg.kullanici_id == e.kullanici_id).FirstOrDefault();
                    if (dbKg != null)
                    {
                        continue;
                    }

                    db.gorev_musteri.Add(kg);
                    db.SaveChanges();
                }

                return JsonSonuc.sonucUret(true, "Müşteri Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc gorevMusterisiSil(string url, string gorev_url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                kullanicilar usr = db.kullanicilar.Where(e => e.url.Equals(url)).FirstOrDefault();
                gorevler grv = db.gorevler.Where(e => e.url.Equals(gorev_url)).FirstOrDefault();
                List<gorev_musteri> gmList = db.gorev_musteri.Where(e => e.flag == durumlar.aktif && e.kullanici_id == usr.id && e.gorev_id == grv.id).ToList();
                //gorev_musteri kg = db.gorev_musteri.Where(e => e.id.Equals(id)).FirstOrDefault();
                //kg.flag = durumlar.silindi;
                foreach (gorev_musteri gm in gmList)
                {
                    gm.flag = durumlar.silindi;
                    db.Entry(gm).State = EntityState.Modified; 
                }                
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Müşteri silindi.");
        }
        public JsonSonuc gorevMusterisKullaniciGorevlendir(int gorevid, string hedef_kullanici, string kaynak_kullanici)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                if (kaynak_kullanici == null || kaynak_kullanici.Equals("null") || kaynak_kullanici.Equals(string.Empty))
                {
                    kaynak_kullanici = "";
                }
                kullanicilar kaynakKullanici = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.url.Equals(kaynak_kullanici)).FirstOrDefault();
                int kaynakKullaniciId = 0;
                if (kaynakKullanici != null)
                {
                    kaynakKullaniciId = kaynakKullanici.id;
                }
                kullanicilar hedefKullanici = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.url.Equals(hedef_kullanici)).FirstOrDefault();
                List<gorev_musteri> gorevMusteriList = db.gorev_musteri.Where(e => e.flag == durumlar.aktif && e.gorev_id == gorevid && e.kullanici_id == kaynakKullaniciId).ToList();                

                foreach (gorev_musteri gm in gorevMusteriList)
                {
                    gm.kullanici_id = hedefKullanici.id;
                    db.Entry(gm).State = EntityState.Modified; 
                }
               
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı yetkilendirildi.");
        }
        public static List<MusteriProjeOzetModel> getGorevMusterilerOzet(int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select m.id, m.ad as ad, m.soyad, m.firma_adi, k.url as kUrl, k.id as kId from gorevler as g "
                + "inner join gorev_musteri as gm on gm.gorev_id = g.id "
                + "inner join musteriler as m on gm.musteri_id = m.id "
                + "left join kullanicilar as k on k.flag = " + durumlar.aktif + " and k.id = gm.kullanici_id "
                + "where g.flag = 1 and gm.flag = 1 and m.flag = 1 and g.id = " + gorev_id.ToString();
            List<MusteriProjeOzetModel> kpList = db.Database.SqlQuery<MusteriProjeOzetModel>(pkQuery).ToList();
            return kpList;
        }

        public JsonSonuc yeniGorevDosyasi(HttpRequestBase Request, HttpServerUtilityBase server)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.dosyalar.Count() != 0)
                {
                    vid = db.dosyalar.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.dosyalar.Count() != 0)
                {
                    sort = db.dosyalar.Max(e => e.sort) + 1;
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

                d.aciklama = "";
                d.flag = durumlar.aktif;
                d.date = DateTime.Now;
                d.ekleyen = GetCurrentUser.GetUser().id;                

                string pathDosya = "~/public/upload/dosyalar";
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
                            return JsonSonuc.sonucUret(false, "Sisteme sadece resim ve yazı içerikleri yükleyebilirsiniz. Desteklenen uzantılar: \".jpg, .jpeg, .png, .bmp, .docx, .doc, .txt, .pptx, .pdf, .xlsx, .pub\"."); ;
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
                        /*string temp = createdFileName;
                        bool bulunduDosya = false;
                        int k = 0;
                        do
                        {
                            if (System.IO.File.Exists(server.MapPath(fullPathWithFileName)))
                            {
                                temp = temp + k.ToString();
                                fullPathWithFileName = pathDosya + temp + ext;
                            }
                            else
                            {
                                createdFileName = temp;
                                fullPathWithFileName = pathDosya + "/" + createdFileName + ext;
                                bulunduDosya = true;
                            }
                            k++;
                        } while (!bulunduDosya);*/
                        hpf_img.SaveAs(server.MapPath(fullPathWithFileName));
                        d.url = createdFileName + ext;
                        return gorevDosyaEkle(d, Convert.ToInt32(Request["gorev_id"].ToString()), Request, server);
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

                /*dosyalar dbDosya = db.dosyalar.Where(e => e.vid == d.vid).FirstOrDefault();
                if (dbDosya == null)
                {
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

                gd.flag = durumlar.aktif;
                gd.date = DateTime.Now;
                gd.vid = vid;
                gd.sort = sort;
                gd.ekleyen = GetCurrentUser.GetUser().id;
                gd.dosya_id = dbDosya.id;

                db.gorev_dosya.Add(gd);
                db.SaveChanges();

                gorevler gorev = db.gorevler.Where(e => e.id == gd.gorev_id).FirstOrDefault();
                if (gorev != null)
	            {
		             logEkle(gorev, "Göreve " + d.isim + " dosyası eklendi.", GetCurrentUser.GetUser());
	            }

                return JsonSonuc.sonucUret(true, "Dosya Eklendi.");*/
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc gorevDosyasiSil(int id)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                gorev_dosya gd = db.gorev_dosya.Where(e => e.id.Equals(id)).FirstOrDefault();
                gd.flag = durumlar.silindi;
                db.Entry(gd).State = EntityState.Modified;

                gorevler gorev = db.gorevler.Where(e => e.id == gd.gorev_id).FirstOrDefault();
                if (gorev != null)
                {
                    logEkle(gorev, "Görevden " + gd.dosyalar.isim + " dosyası silindi.", GetCurrentUser.GetUser());
                }

                db.SaveChanges();
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Dosya silindi.");
        }
        public static List<dosyaOzetModel> getGorevDosyalarOzet(int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select d.isim, d.url, gd.id from gorevler as g  "
                + "inner join gorev_dosya as gd on gd.gorev_id = g.id "
                + "inner join dosyalar as d on gd.dosya_id = d.id "
                + "where g.flag = 1 and gd.flag = 1 and d.flag = 1 and g.id = " + gorev_id.ToString();
            List<dosyaOzetModel> kpList = db.Database.SqlQuery<dosyaOzetModel>(pkQuery).ToList();
            return kpList;
        }

        public static bool logEkle(gorevler grv, string aciklama, LoggedUserModel lgm)
        {
            try 
	        {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                gorev_loglari gl = new gorev_loglari();

                int vidLog = 1;
                if (db.gorev_loglari.Count() != 0)
                {
                    vidLog = db.gorev_loglari.Max(e => e.vid) + 1;
                }
                int sortLog = 1;
                if (db.gorev_loglari.Count() != 0)
                {
                    sortLog = db.gorev_loglari.Max(e => e.sort) + 1;
                }

                gl.aciklama = aciklama;
                gl.date = DateTime.Now;
                gl.flag = durumlar.aktif;
                gl.gorev_id = grv.id;
                gl.gorevin_eski_durumu = 0;
                gl.gorevin_yeni_durumu = 0;
                gl.islem = "";
                gl.kullanici_id = lgm.id;
                gl.sort = sortLog;

                string strImageNameLog = StringFormatter.OnlyEnglishChar(grv.isim);
                string createdUrlLog = strImageNameLog;
                string tempUrlLog = createdUrlLog;
                bool bulunduLog = false;
                int iLog = 0;
                gorev_loglari pgLog = new gorev_loglari();
                do
                {
                    pgLog = db.gorev_loglari.Where(e => e.url.Equals(tempUrlLog)).FirstOrDefault();
                    if (pgLog != null)
                    {
                        tempUrlLog = createdUrlLog + iLog.ToString();
                    }
                    else
                    {
                        createdUrlLog = tempUrlLog;
                        bulunduLog = true;
                    }
                    iLog++;
                } while (!bulunduLog);
                gl.url = createdUrlLog;

                gl.vid = vidLog;
                db.gorev_loglari.Add(gl);
                db.SaveChanges();

                return true;
            }
	        catch (Exception)
	        {
		        return false;
	        }
        }
        public static List<GorevLogOzet> getGorevLoglariOzet(int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select DATE_FORMAT(gl.date, '%d.%m.%Y %H:%i') as date, gl.aciklama, k.ad, k.soyad from gorev_loglari as gl  "
                + "inner join gorevler as g on gl.gorev_id = g.id "
                + "inner join kullanicilar as k on k.id = gl.kullanici_id "
                + "where g.flag = 1 and gl.flag = 1 and g.id = " + gorev_id.ToString() + " order by gl.vid desc;";
            List<GorevLogOzet> kpList = db.Database.SqlQuery<GorevLogOzet>(pkQuery).ToList();
            return kpList;
        }

        #region görev bağlama
        public JsonSonuc yeniGorevBag(HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.gorev_baglanti.Count() != 0)
                {
                    vid = db.gorev_baglanti.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.gorev_baglanti.Count() != 0)
                {
                    sort = db.gorev_baglanti.Max(e => e.sort) + 1;
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

                gb.flag = durumlar.aktif;
                gb.date = DateTime.Now;
                gb.vid = vid;
                gb.sort = sort;
                gb.ekleyen = GetCurrentUser.GetUser().id;

                gorev_baglanti dbGb = db.gorev_baglanti.Where(e => e.flag == durumlar.aktif && e.gorev_id == gb.gorev_id && e.bagli_gorev == gb.bagli_gorev).FirstOrDefault();
                if (dbGb != null)
                {
                    return JsonSonuc.sonucUret(true, "Görev bağlandı.");
                }

                db.gorev_baglanti.Add(gb);
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, "Görev bağlandı.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc gorevBaglantisiSil(int id)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                gorev_baglanti kg = db.gorev_baglanti.Where(e => e.id.Equals(id)).FirstOrDefault();
                kg.flag = durumlar.silindi;
                db.Entry(kg).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Bağlantı silindi.");
        }
        public static List<GorevBaglantiOzetModel> getGorevBaglantilar(int gorev_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select gb.*, g.isim from gorev_baglanti as gb "
                + "inner join gorevler as g on g.flag = " + durumlar.aktif + " and gb.bagli_gorev = g.id "
                + "where gb.flag = " + durumlar.aktif + " and gb.gorev_id = " + gorev_id.ToString();
            List<GorevBaglantiOzetModel> kpList = db.Database.SqlQuery<GorevBaglantiOzetModel>(pkQuery).ToList();
            return kpList;
        }
        #endregion görev bağlama


        public JsonSonuc goreviKaydet(int proje_id, string tempGuid, gorevler grv, HttpRequestBase Request, HttpSessionStateBase Session, HttpServerUtilityBase Server)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                LoggedUserModel lgm = GetCurrentUser.GetUser();
                TempAddingObject tempObj = (TempAddingObject)Session[tempGuid];

                int eskiGorevId = grv.id;
                #region görev bilgileri kaydı
                /*int vidGorev = 1;
                if (db.gorevler.Count() != 0)
                {
                    vidGorev = db.gorevler.Max(e => e.vid) + 1;
                }
                int sortGorev = 1;
                if (db.gorevler.Count() != 0)
                {
                    sortGorev = db.gorevler.Max(e => e.sort) + 1;
                }
                string strImageNameGorev = StringFormatter.OnlyEnglishChar(grv.isim);
                string createdUrlGorev = strImageNameGorev;
                string tempUrlGorev = createdUrlGorev;
                bool bulunduGorev = false;
                int iGorev = 0;
                gorevler pgGorev = new gorevler();
                do
                {
                    pgGorev = db.gorevler.Where(e => e.url.Equals(tempUrlGorev)).FirstOrDefault();
                    if (pgGorev != null)
                    {
                        tempUrlGorev = tempUrlGorev + iGorev.ToString();
                    }
                    else
                    {
                        createdUrlGorev = tempUrlGorev;
                        bulunduGorev = true;
                    }
                    iGorev++;
                } while (!bulunduGorev);
                grv.url = createdUrlGorev;
                grv.firma_id = lgm.firma_id;

                grv.flag = durumlar.aktif;
                grv.date = DateTime.Now;
                grv.vid = vidGorev;
                grv.ekleyen = lgm.id;
                grv.sort = sortGorev;
                grv.durum = TamamlamaDurumlari.bekliyor;
                grv.tamamlanma_tarihi = DateTime.Now;

                db.gorevler.Add(grv);
                db.SaveChanges();*/
                gorevler yeniGrv = new gorevler();
                CloneObject.CopyTo(grv, yeniGrv);
                string grvUrl = goreviEkle(yeniGrv, proje_id);
                #endregion görev bilgileri kaydı
                gorevler dbGrv = db.gorevler.Where(e => e.url == grvUrl).FirstOrDefault();
                //gorevler dbGrv = db.gorevler.Where(e => e.vid == grv.vid).FirstOrDefault();
                foreach (gorev_baglanti gbag in tempObj.gorevBaglantilari)
                {
                    if (gbag.bagli_gorev == eskiGorevId)
                    {
                        gbag.bagli_gorev = dbGrv.id;
                    }
                }
                /*if (proje_id != 0)
                {
                    #region gorev proje kaydı
                    gorev_proje pm = new gorev_proje();
                    pm = new gorev_proje();
                    pm.date = DateTime.Now;
                    pm.flag = durumlar.aktif;
                    pm.proje_id = proje_id;
                    pm.gorev_id = dbGrv.id;
                    int vidPm = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        vidPm = db.gorev_proje.Max(e => e.vid) + 1;
                    }
                    int sortPm = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        sortPm = db.gorev_proje.Max(e => e.sort) + 1;
                    }
                    pm.sort = sortPm;
                    pm.vid = vidPm;
                    db.gorev_proje.Add(pm);
                    db.SaveChanges();
                    #endregion gorev proje kaydı
                }*/

                if (dbGrv != null)
                {
                    #region gorev Kullanicilari ekle
                    List<kullanici_gorev> gorevKullaniciList = tempObj.kullaniciGorevList.Where(e => e.gorev_id == eskiGorevId).ToList();
                    foreach (kullanici_gorev kg in gorevKullaniciList)
                    {
                        int vidKullaniciGorev = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            vidKullaniciGorev = db.kullanici_gorev.Max(e => e.vid) + 1;
                        }
                        int sortKullaniciGorev = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            sortKullaniciGorev = db.kullanici_gorev.Max(e => e.sort) + 1;
                        }

                        kg.gorev_id = dbGrv.id;
                        kg.flag = durumlar.aktif;
                        kg.date = DateTime.Now;
                        kg.vid = vidKullaniciGorev;
                        kg.sort = sortKullaniciGorev;
                        kg.ekleyen = lgm.id;

                        db.kullanici_gorev.Add(kg);
                        db.SaveChanges();
                    }
                    #endregion gorev Kullanicilari ekle
                    #region gorev Musterileri ekle
                    List<gorev_musteri> grvMusteriList = tempObj.gorevMusteriList.Where(e => e.gorev_id == eskiGorevId).ToList();
                    foreach (gorev_musteri gm in grvMusteriList)
                    {
                        int vidKullaniciGorev = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            vidKullaniciGorev = db.kullanici_gorev.Max(e => e.vid) + 1;
                        }
                        int sortKullaniciGorev = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            sortKullaniciGorev = db.kullanici_gorev.Max(e => e.sort) + 1;
                        }

                        gm.gorev_id = dbGrv.id;
                        gm.flag = durumlar.aktif;
                        gm.date = DateTime.Now;
                        gm.vid = vidKullaniciGorev;
                        gm.sort = sortKullaniciGorev;
                        gm.ekleyen = GetCurrentUser.GetUser().id;

                        db.gorev_musteri.Add(gm);
                        db.SaveChanges();
                    }
                    #endregion gorev Musterileri ekle
                    #region gorev Yapilacaklari ekle
                    List<yapilacaklar> yapilacakList = tempObj.yapilacaklarList.Where(e => e.gorev_id == eskiGorevId).ToList();
                    foreach (yapilacaklar yplck in yapilacakList)
                    {
                        yplck.firma_id = lgm.firma_id;

                        yplck.tamamlanma_tarihi = DateTime.Now;
                        yplck.gorev_id = dbGrv.id;
                        yplck.flag = durumlar.aktif;
                        yplck.date = DateTime.Now;
                        yplck.ekleyen = GetCurrentUser.GetUser().id;
                        yplck.durum = YapilacaklarDurum.beklemede;

                        yapilacakIslemleri yis = new yapilacakIslemleri();
                        yis.yapilacakEkle(yplck);
                    }
                    #endregion gorev Yapilacaklari ekle
                    #region gorev Dosyaları ekle
                    List<gorev_dosya> gorevDosyaList = tempObj.gorevDosyalarList.Where(e => e.gorev_id == eskiGorevId).ToList();
                    foreach (gorev_dosya gd in gorevDosyaList)
                    {
                        int vidDosya = 1;
                        if (db.dosyalar.Count() != 0)
                        {
                            vidDosya = db.dosyalar.Max(e => e.vid) + 1;
                        }
                        int sortDosya = 1;
                        if (db.dosyalar.Count() != 0)
                        {
                            sortDosya = db.dosyalar.Max(e => e.sort) + 1;
                        }

                        dosyalar dsy = tempObj.dosyalarList.Where(e => e.id == gd.dosya_id).FirstOrDefault();

                        dsy.aciklama = "";
                        dsy.flag = durumlar.aktif;
                        dsy.date = DateTime.Now;
                        dsy.vid = vidDosya;
                        dsy.sort = sortDosya;
                        dsy.ekleyen = GetCurrentUser.GetUser().id;

                        string ext = ".png";
                        string pathDosyaTemp = "~/public/upload/dosyalar/temp";
                        if (!System.IO.File.Exists(Server.MapPath(pathDosyaTemp + "/" + dsy.url)))
                        {
                            continue;
                        }

                        string fileName = Path.GetFileName(pathDosyaTemp + dsy.url);

                        ext = Path.GetExtension(fileName);

                        string pathDosya = "~/public/upload/dosyalar";

                        string strFileNameDosya = StringFormatter.OnlyEnglishChar(dsy.isim);
                        string createdUrlDosya = strFileNameDosya;
                        string tempUrlDosya = createdUrlDosya;
                        bool bulunduDosya = false;
                        int iDosya = 1;
                        dosyalar pgDosya = new dosyalar();
                        do
                        {
                            pgDosya = db.dosyalar.Where(e => e.url.Equals(tempUrlDosya + ext)).FirstOrDefault();
                            if (pgDosya != null)
                            {
                                tempUrlDosya = strFileNameDosya + iDosya.ToString();
                            }
                            else
                            {
                                createdUrlDosya = tempUrlDosya;
                                bulunduDosya = true;
                            }
                            iDosya++;
                        } while (!bulunduDosya);
                        strFileNameDosya = createdUrlDosya;

                        string createdFileName = strFileNameDosya;
                        string fullPathWithFileName = pathDosya + "/" + createdFileName + ext;

                        System.IO.File.Copy(Server.MapPath(pathDosyaTemp + "/" + dsy.url), Server.MapPath(fullPathWithFileName), true);
                        dsy.url = createdFileName + ext;

                        gorevDosyaEkle(dsy, dbGrv.id, Request, Server);
                    }
                    #endregion gorev Dosyaları ekle
                    #region gorev bağlantıları ekle
                    List<gorev_baglanti> baglantiList = tempObj.gorevBaglantilari.Where(e => e.gorev_id == eskiGorevId).ToList();
                    foreach (gorev_baglanti bglnt in baglantiList)
                    {
                        int vidBaglanti = 1;
                        if (db.gorev_baglanti.Count() != 0)
                        {
                            vidBaglanti = db.gorev_baglanti.Max(e => e.vid) + 1;
                        }
                        int sortBaglanti = 1;
                        if (db.gorev_baglanti.Count() != 0)
                        {
                            sortBaglanti = db.gorev_baglanti.Max(e => e.sort) + 1;
                        }

                        bglnt.gorev_id = dbGrv.id;
                        bglnt.flag = durumlar.aktif;
                        bglnt.date = DateTime.Now;
                        bglnt.vid = vidBaglanti;
                        bglnt.ekleyen = GetCurrentUser.GetUser().id;
                        bglnt.sort = sortBaglanti;

                        gorevBaglantiEkle(bglnt);
                    }
                    #endregion gorev bağlantıları ekle
                }

                var ctx = ((IObjectContextAdapter)db).ObjectContext;
                ctx.Refresh(RefreshMode.StoreWins, db.gorev_dosya);
                ctx.Refresh(RefreshMode.StoreWins, db.dosyalar);

                if (dbGrv.gorev_multiply == GorevMultiplyDurum.multiply)
                {
                    JsonSonuc sonuc = multiplyGorev(dbGrv, Request, Server);
                    if (sonuc.IsSuccess == false)
                    {
                        return sonuc;
                    }
                }

                return JsonSonuc.sonucUret(true, dbGrv.url);
            }
            catch (Exception ex)
            {
                return JsonSonuc.sonucUret(false, "Görev eklenirken bir hata oluştu.");
            }

        }
        public string goreviEkle(gorevler grv, int proje_id)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();

            vrlfgysdbEntities db = new vrlfgysdbEntities();

            int vidGorev = 1;
            if (db.gorevler.Count() != 0)
            {
                vidGorev = db.gorevler.Max(e => e.vid) + 1;
            }
            int sortGorev = 1;
            if (db.gorevler.Count() != 0)
            {
                sortGorev = db.gorevler.Max(e => e.sort) + 1;
            }
            string strImageNameGorev = StringFormatter.OnlyEnglishChar(grv.isim);
            string createdUrlGorev = strImageNameGorev;
            string tempUrlGorev = createdUrlGorev;
            bool bulunduGorev = false;
            int iGorev = 0;
            gorevler pgGorev = new gorevler();
            do
            {
                pgGorev = db.gorevler.Where(e => e.url.Equals(tempUrlGorev)).FirstOrDefault();
                if (pgGorev != null)
                {
                    tempUrlGorev = tempUrlGorev + iGorev.ToString();
                }
                else
                {
                    createdUrlGorev = tempUrlGorev;
                    bulunduGorev = true;
                }
                iGorev++;
            } while (!bulunduGorev);
            grv.url = createdUrlGorev;
            grv.vid = vidGorev;
            grv.sort = sortGorev;
            grv.kullanicilar = null;
            grv.yapilacaklar = null;
            grv.gorev_baglanti = null;
            grv.gorev_baglanti1 = null;
            grv.gorev_dosya = null;
            grv.gorev_loglari = null;
            grv.gorev_musteri = null;
            grv.gorev_proje = null;
            grv.kullanici_gorev = null;

            db.gorevler.Add(grv);
            db.SaveChanges();

            gorevler dbGrv = db.gorevler.Where(e => e.vid == grv.vid).FirstOrDefault();

            #region proje gorev
            if (proje_id != 0)
            {
                //gorevler dbGrv = db.gorevler.Where(e => e.vid == grv.vid).FirstOrDefault();
                gorev_proje pm = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).FirstOrDefault();
                if (pm == null)
                {
                    pm = new gorev_proje();
                    pm.date = DateTime.Now;
                    pm.flag = durumlar.aktif;
                    pm.proje_id = proje_id;
                    pm.gorev_id = dbGrv.id;
                    int vidPm = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        vidPm = db.gorev_proje.Max(e => e.vid) + 1;
                    }
                    int sortPm = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        sortPm = db.gorev_proje.Max(e => e.sort) + 1;
                    }
                    pm.sort = sortPm;
                    pm.vid = vidPm;
                    db.gorev_proje.Add(pm);
                    db.SaveChanges();
                }
                else if (pm != null && pm.proje_id != proje_id)
                {
                    pm.proje_id = proje_id;
                    db.Entry(pm).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else if (proje_id == 0)
            {                
                gorev_proje gp = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).FirstOrDefault();
                if (gp != null)
                {
                    gp.flag = durumlar.silindi;
                    db.Entry(gp).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            #endregion proje gorev

            gorevYuzdesiDuzenle(dbGrv.id);

            gorevler dbGrv2 = db.gorevler.Where(e => e.vid == grv.vid).FirstOrDefault();
            logEkle(dbGrv2, "Görev oluşturuldu.", lgm);

            return grv.url;
        }
        public JsonSonuc multiplyGorev(gorevler dbGrv, HttpRequestBase Request, HttpServerUtilityBase Server)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();

            gorevler dbGrvYeni = new gorevler();

            List<kullanici_gorev> kullaniciGorevList = db.kullanici_gorev.Where(e => e.gorev_id == dbGrv.id).ToList();
            if (kullaniciGorevList.Count > 1)
            {
                string isimGorev = dbGrv.isim;
                int kullaniciId = kullaniciGorevList[0].kullanici_id;
                kullanicilar usr = db.kullanicilar.Where(e => e.id == kullaniciId).FirstOrDefault();
                dbGrv.isim = isimGorev + "(" + usr.ad + " " + usr.soyad + ")";

                gorev_proje dbGp = db.gorev_proje.Where(e => e.gorev_id == dbGrv.id && e.flag == durumlar.aktif).FirstOrDefault();
                List<yapilacaklar> yapilacakList = db.yapilacaklar.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).ToList();
                List<gorev_baglanti> gbList = db.gorev_baglanti.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).ToList();
                List<gorev_dosya> gorevDosyaList = db.gorev_dosya.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id).ToList();
                yapilacakIslemleri yis = new yapilacakIslemleri();
                for (int i = 1; i < kullaniciGorevList.Count; i++)
                {
                    kullaniciId = kullaniciGorevList[i].kullanici_id;
                    kullanicilar usr2 = db.kullanicilar.Where(e => e.id == kullaniciId).FirstOrDefault();
                    kullanici_gorev kg = db.kullanici_gorev.Where(e => e.flag == durumlar.aktif && e.gorev_id == dbGrv.id && e.kullanici_id == kullaniciId).FirstOrDefault();
                    List<gorev_musteri> gmList = db.gorev_musteri.Where(e => e.flag == durumlar.aktif && e.kullanici_id == kullaniciId).ToList();
                    gorevler yeniGrv1 = new gorevler();
                    CloneObject.CopyTo(dbGrv, yeniGrv1);
                    yeniGrv1.isim = isimGorev + "(" + usr2.ad + " " + usr2.soyad + ")";
                    yeniGrv1.gorev_proje = null;
                    yeniGrv1.gorev_baglanti = null;
                    yeniGrv1.gorev_baglanti1 = null;
                    yeniGrv1.gorev_dosya = null;
                    yeniGrv1.gorev_loglari = null;
                    yeniGrv1.gorev_musteri = null;
                    yeniGrv1.gorev_proje = null;
                    yeniGrv1.kullanici_gorev = null;
                    yeniGrv1.kullanicilar = null;
                    yeniGrv1.yapilacaklar = null;
                    int proje_id = 0;
                    if (dbGp != null)
                    {
                        proje_id = dbGp.proje_id;
                    }
                    string sonuc1 = goreviEkle(yeniGrv1, proje_id);

                    if (sonuc1.Equals("") || sonuc1.Equals("gorev_sayisi_hatasi"))
                    {
                        if (sonuc1.Equals("gorev_sayisi_hatasi"))
                        {
                            return JsonSonuc.sonucUret(false, "Firmanıza başka görev eklenemez. Daha fazla görev ekleyebilmek için sistem yöneticimizle irtibata geçiniz.");
                        }
                        else
                        {
                            return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                        }
                    }

                    dbGrvYeni = db.gorevler.Where(e => e.flag == durumlar.aktif && e.url.Equals(sonuc1)).FirstOrDefault();
                    if (dbGrvYeni != null)
                    {

                        foreach (yapilacaklar yplck in yapilacakList)
                        {
                            yapilacaklar yeniYplck = new yapilacaklar();
                            CloneObject.CopyTo(yplck, yeniYplck);
                            yeniYplck.gorev_id = dbGrvYeni.id;
                            yeniYplck.firma_musavir = null;
                            yeniYplck.gorevler = null;
                            yeniYplck.kullanicilar = null;
                            JsonSonuc sonuc = yis.yapilacakEkle(yeniYplck);
                            if (sonuc.IsSuccess == false)
                            {
                                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                            }
                        }
                        foreach (gorev_baglanti gb in gbList)
                        {
                            gorev_baglanti yeniGb = new gorev_baglanti();
                            CloneObject.CopyTo(gb, yeniGb);
                            yeniGb.gorevler = null;
                            yeniGb.gorevler1 = null;
                            yeniGb.kullanicilar = null;
                            yeniGb.gorev_id = dbGrvYeni.id;
                            JsonSonuc sonuc = gorevBaglantiEkle(yeniGb);
                            if (sonuc.IsSuccess == false)
                            {
                                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                            }
                        }
                        foreach (gorev_dosya gd in gorevDosyaList)
                        {
                            dosyalar d = db.dosyalar.Where(e => e.id == gd.dosya_id).FirstOrDefault();
                            dosyalar yeniD = new dosyalar();
                            CloneObject.CopyTo(d, yeniD);

                            string pathDosya = "~/public/upload/dosyalar";
                            string ext = Path.GetExtension(d.url);
                            string strFileNameDosya = StringFormatter.OnlyEnglishChar(d.isim);
                            string createdUrlDosya = strFileNameDosya;
                            string tempUrlDosya = createdUrlDosya;
                            bool bulunduDosya = false;
                            int iDosya = 1;
                            dosyalar pgDosya = new dosyalar();
                            do
                            {
                                pgDosya = db.dosyalar.Where(e => e.url.Equals(tempUrlDosya + ext)).FirstOrDefault();
                                if (pgDosya != null)
                                {
                                    tempUrlDosya = strFileNameDosya + iDosya.ToString();
                                }
                                else
                                {
                                    createdUrlDosya = tempUrlDosya;
                                    bulunduDosya = true;
                                }
                                iDosya++;
                            } while (!bulunduDosya);
                            strFileNameDosya = createdUrlDosya;
                            string createdFileName = strFileNameDosya;
                            string fullPathWithFileName = pathDosya + "/" + createdFileName + ext;

                            System.IO.File.Copy(Server.MapPath(pathDosya + "/" + d.url), Server.MapPath(fullPathWithFileName), true);
                            yeniD.url = createdFileName + ext;
                            yeniD.gorev_dosya = null;
                            yeniD.kullanicilar = null;
                            JsonSonuc sonuc = gorevDosyaEkle(yeniD, dbGrvYeni.id, Request, Server);
                            if (sonuc.IsSuccess == false)
                            {
                                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                            }
                        }
                        foreach (gorev_musteri gm in gmList)
                        {
                            gm.gorev_id = dbGrvYeni.id;
                            db.Entry(kg).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        kg.gorev_id = dbGrvYeni.id;
                        db.Entry(kg).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            return JsonSonuc.sonucUret(true, dbGrvYeni.url);
        }

        public JsonSonuc gorevBaglantiEkle(gorev_baglanti gb)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.gorev_baglanti.Count() != 0)
                {
                    vid = db.gorev_baglanti.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.gorev_baglanti.Count() != 0)
                {
                    sort = db.gorev_baglanti.Max(e => e.sort) + 1;
                }
                gb.vid = vid;
                gb.sort = sort;

                db.gorev_baglanti.Add(gb);

                gorevler grv = db.gorevler.Where(e => e.flag == durumlar.aktif && e.id == gb.gorev_id).FirstOrDefault();
                if (grv.durum == TamamlamaDurumlari.tamamlandi)
                {
                    grv.durum = TamamlamaDurumlari.bekliyor;
                }
                else
                {
                    grv.durum = TamamlamaDurumlari.oncekiGorevBekleniyor;
                }
                
                db.Entry(grv).State = EntityState.Modified;
                db.SaveChanges();

                return JsonSonuc.sonucUret(true, gb.vid);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc gorevDosyaEkle(dosyalar d, int gorev_id, HttpRequestBase Request, HttpServerUtilityBase Server)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.dosyalar.Count() != 0)
                {
                    vid = db.dosyalar.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.dosyalar.Count() != 0)
                {
                    sort = db.dosyalar.Max(e => e.sort) + 1;
                }

                d.vid = vid;
                d.sort = sort;
                d.gorev_dosya = null;
                d.kullanicilar = null;

                db.dosyalar.Add(d);
                db.SaveChanges();

                dosyalar dbDosya = db.dosyalar.Where(e => e.vid == d.vid).FirstOrDefault();
                if (dbDosya == null)
                {
                    return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
                }

                gorev_dosya gd = new gorev_dosya();

                gd.gorev_id = gorev_id;
                gd.flag = durumlar.aktif;
                gd.date = DateTime.Now;
                gd.vid = vid;
                gd.sort = sort;
                gd.ekleyen = GetCurrentUser.GetUser().id;
                gd.dosya_id = dbDosya.id;
                gd.dosyalar = null;
                gd.gorevler = null;
                gd.kullanicilar = null;

                db.gorev_dosya.Add(gd);
                db.SaveChanges();

                gorevler gorev = db.gorevler.Where(e => e.id == gd.gorev_id).FirstOrDefault();
                if (gorev != null)
                {
                    logEkle(gorev, "Göreve " + d.isim + " dosyası eklendi.", GetCurrentUser.GetUser());
                }

                return JsonSonuc.sonucUret(true, "Dosya Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
    }
}