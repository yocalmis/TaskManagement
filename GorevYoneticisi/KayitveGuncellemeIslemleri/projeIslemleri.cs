using GorevYoneticisi.Models;
using GorevYoneticisi.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace GorevYoneticisi.KayitveGuncellemeIslemleri
{
    public class projeIslemleri
    {
        public string yeniProje(int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                int vid = 1;
                if (db.proje_surec.Count() != 0)
                {
                    vid = db.proje_surec.Max(e => e.vid) + 1;
                }
                int sort = 1;
                if (db.proje_surec.Count() != 0)
                {
                    sort = db.proje_surec.Max(e => e.sort) + 1;
                }

                proje_surec prj = new proje_surec();
                foreach (var property in prj.GetType().GetProperties())
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
                            PropertyInfo propertyS = prj.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(prj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(prj, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(prj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(prj, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(prj.isim);
                string createdUrl = strImageName;
                string tempUrl = createdUrl;
                bool bulundu = false;
                int i = 0;
                proje_surec pg = new proje_surec();
                do
                {
                    pg = db.proje_surec.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
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
                prj.url = createdUrl;
                prj.firma_id = firma_id;

                prj.flag = durumlar.aktif;
                prj.date = DateTime.Now;
                prj.vid = vid;
                prj.ekleyen = GetCurrentUser.GetUser().id;
                prj.sort = sort;
                //prj.donem_sayisi = 0;
                prj.parent_vid = 0;
                prj.durum = TamamlamaDurumlari.bekliyor;
                //prj.periyot_suresi = 0;
                prj.periyot_turu = 0;
                prj.mevcut_donem = 0;
                prj.tur = ProjeSurecTur.proje;
                prj.tamamlanma_tarihi = DateTime.Now;

                string isimControl = "select * from proje_surec where tur = " + ProjeSurecTur.proje + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + prj.isim + "' and firma_id = " + prj.firma_id;
                ProjeSurecModel isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                if (isimKontrolPs != null)
                {
                    return "proje_isim_hatasi";
                }
                
                bool kullaniciKontrol = firmaProjeKontrol(prj.firma_id, prj.id).Result;
                if (!kullaniciKontrol)
                {
                    return "proje_sayisi_hatasi";
                }

                db.proje_surec.Add(prj);
                db.SaveChanges();

                /*int musteri_id = Convert.ToInt32(Request["musteri_id"].ToString());
                if (musteri_id != 0)
                {
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == prj.vid).FirstOrDefault();
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault();
                    if (pm == null)
                    {
                        pm = new proje_musteri();
                        pm.date = DateTime.Now;
                        pm.flag = durumlar.aktif;
                        pm.musteri_id = musteri_id;
                        pm.proje_id = dbPs.id;
                        int vidPm = 1;
                        if (db.proje_musteri.Count() != 0)
                        {
                            vidPm = db.proje_musteri.Max(e => e.vid) + 1;
                        }
                        int sortPm = 1;
                        if (db.proje_musteri.Count() != 0)
                        {
                            sortPm = db.proje_musteri.Max(e => e.sort) + 1;
                        }
                        pm.sort = sortPm;
                        pm.vid = vidPm;
                        db.proje_musteri.Add(pm);
                        db.SaveChanges();
                    }
                    else if (pm != null && pm.musteri_id != musteri_id)
                    {
                        pm.musteri_id = musteri_id;
                        db.Entry(pm).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else if (musteri_id == 0)
                {
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == prj.vid).FirstOrDefault();
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault(); if (pm != null && pm.musteri_id != musteri_id)
                        if (pm != null)
                        {
                            pm.flag = durumlar.silindi;
                            db.Entry(pm).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                }*/

                return prj.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string projeDuzenle(string url, int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                proje_surec dbPrj = db.proje_surec.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi).FirstOrDefault();

                if (dbPrj == null || url == null || url.Equals(""))
                {
                    return yeniProje(firma_id, Request);
                }
                else if (!(dbPrj.flag != durumlar.silindi))
                {
                    return "";
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

                string isimControl = "select * from proje_surec where id != " + dbPrj.id.ToString() + " and tur = " + ProjeSurecTur.proje + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + dbPrj.isim + "' and firma_id = " + dbPrj.firma_id;
                ProjeSurecModel isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                if (isimKontrolPs != null)
                {
                    return "proje_isim_hatasi";
                }

                bool kullaniciKontrol = firmaProjeKontrol(dbPrj.firma_id, dbPrj.id).Result;
                if (!kullaniciKontrol)
                {
                    return "proje_sayisi_hatasi";
                }

                db.Entry(dbPrj).State = EntityState.Modified;
                db.SaveChanges();

                /*#region proje_musteri
                int musteri_id = Convert.ToInt32(Request["musteri_id"].ToString());
                if (musteri_id != 0)
                {
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == dbPrj.vid).FirstOrDefault();
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault();
                    if (pm == null)
                    {
                        pm = new proje_musteri();
                        pm.date = DateTime.Now;
                        pm.flag = durumlar.aktif;
                        pm.musteri_id = musteri_id;
                        pm.proje_id = dbPs.id;
                        int vidPm = 1;
                        if (db.proje_musteri.Count() != 0)
                        {
                            vidPm = db.proje_musteri.Max(e => e.vid) + 1;
                        }
                        int sortPm = 1;
                        if (db.proje_musteri.Count() != 0)
                        {
                            sortPm = db.proje_musteri.Max(e => e.sort) + 1;
                        }
                        pm.sort = sortPm;
                        pm.vid = vidPm;
                        db.proje_musteri.Add(pm);
                        db.SaveChanges();
                    }
                    else if (pm != null && pm.musteri_id != musteri_id)
                    {
                        pm.musteri_id = musteri_id;
                        db.Entry(pm).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else if (musteri_id == 0)
                {
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == dbPrj.vid).FirstOrDefault();
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault();
                    if (pm != null)
                    {
                        pm.flag = durumlar.silindi;
                        db.Entry(pm).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                #endregion proje_musteri*/

                return dbPrj.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static async Task<bool> firmaProjeKontrol(int firma_id, int musteri_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            if (firma_id == 0)
            {
                return true;
            }
            var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
            string queryMusteriCount = "select count(id) from proje_surec where tur = " + ProjeSurecTur.proje + " and flag != " + durumlar.silindi.ToString() + " and firma_id = " + firma_id.ToString() + " and id != " + musteri_id;
            var kc = db.Database.SqlQuery<int>(queryMusteriCount).FirstOrDefaultAsync();

            await Task.WhenAll(f, kc);

            firma_musavir fm = f.Result;
            int projeCount = kc.Result;

            if (fm == null)
            {
                return false;
            }
            else if (fm.proje_sayisi > projeCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public JsonSonuc silProje(string url, int firma_id)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                proje_surec prj = null;
                if (firma_id != 0)
                {
                    prj = db.proje_surec.Where(e => e.url.Equals(url) && e.firma_id == firma_id).FirstOrDefault();
                }
                else
                {
                    prj = db.proje_surec.Where(e => e.url.Equals(url)).FirstOrDefault();
                }                
                if (prj == null)
                {
                    return JsonSonuc.sonucUret(false, "Proje bulunamadı.");
                }
                //prj.flag = durumlar.silindi;
                prj.durum = TamamlamaDurumlari.pasif;
                db.Entry(prj).State = EntityState.Modified;
                db.SaveChanges();

                List<gorev_proje> projeGorevList = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.proje_id == prj.id).ToList();
                gorevIslemleri gis = new gorevIslemleri();
                foreach (gorev_proje gp in projeGorevList)
                {
                    if (gp.gorevler != null)
                    {
                        //gis.silGorev(gp.gorevler.url);
                        gp.gorevler.durum = TamamlamaDurumlari.pasif;
                        db.Entry(gp.gorevler).State = EntityState.Modified;
                    }                    
                }
                
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Proje pasif edildi.");
        }
        public JsonSonuc projeYuzdesiDuzenle(int proje_id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                proje_surec prj = db.proje_surec.Where(e => e.id == proje_id).FirstOrDefault();
                List<gorev_proje> projeGorevList = db.gorev_proje.Where(e => e.proje_id == proje_id && e.flag == durumlar.aktif).ToList();

                int toplam = 0;
                int aktifGorevler = 0;
                foreach (gorev_proje pg in projeGorevList)
                {
                    if (pg.gorevler.flag == durumlar.aktif)
                    {
                        toplam += pg.gorevler.yuzde;
                        aktifGorevler++;
                    }
                }

                if (aktifGorevler != 0)
                {
                    prj.yuzde = toplam / aktifGorevler;
                }
                else
                {
                    prj.yuzde = toplam;
                }

                prj.durum = TamamlamaDurumlari.basladi;

                db.Entry(prj).State = EntityState.Modified;
                db.SaveChanges();

                if (prj.yuzde == 100)
                {
                    List<kullanicilar> yetkiliList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.kullanici_turu <= KullaniciTurleri.firma_yetkili).ToList();
                    foreach (kullanicilar usr in yetkiliList)
                    {
                        bildirimIslemleri.yeniBildirim(usr.id, BildirimTurleri.proje, prj.id, "", prj.isim + " isimli proje tamamlandı ve onayınızı bekliyor.");
                    }
                }

                return JsonSonuc.sonucUret(true, prj.yuzde);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "Proje yüzdesini düzenlerken bir hata oluştu.");
            }
        }

        #region proje müşterisi işlemleri
        public JsonSonuc yeniProjeMusterisi(HttpRequestBase Request, string[] musteriList)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                foreach (string str in musteriList)
                {
                    int vid = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        vid = db.proje_musteri.Max(e => e.vid) + 1;
                    }
                    int sort = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        sort = db.proje_musteri.Max(e => e.sort) + 1;
                    }

                    proje_musteri pm = new proje_musteri();
                    foreach (var property in pm.GetType().GetProperties())
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
                                PropertyInfo propertyS = pm.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(pm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(pm, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(pm, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(pm, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception)
                        { }
                    }

                    pm.flag = durumlar.aktif;
                    pm.date = DateTime.Now;
                    pm.vid = vid;
                    pm.sort = sort;
                    pm.ekleyen = GetCurrentUser.GetUser().id;
                    pm.musteri_id = Convert.ToInt32(str);

                    proje_musteri dbPm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == pm.proje_id && e.musteri_id == pm.musteri_id).FirstOrDefault();
                    if (dbPm != null)
                    {
                        continue;
                        //return JsonSonuc.sonucUret(true, "Müşteri Eklendi.");
                    }

                    db.proje_musteri.Add(pm);
                    db.SaveChanges();
                }
                
                return JsonSonuc.sonucUret(true, "Müşteri Eklendi.");
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        }
        public JsonSonuc projeMusterisiSil(int proje_id)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                //kullanicilar usr = db.kullanicilar.Where(e => e.url.Equals(url)).FirstOrDefault();
                List<proje_musteri> pmList = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id.Equals(proje_id)).ToList();
                foreach (proje_musteri pm in pmList)
                {
                    pm.flag = durumlar.silindi;
                    db.Entry(pm).State = EntityState.Modified;
                }                
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı silindi.");
        }
        public JsonSonuc projeSurecMusterisiKullaniciGorevlendir(int id, string kullanici)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                kullanicilar usr = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.url.Equals(kullanici)).FirstOrDefault();
                proje_musteri pm = db.proje_musteri.Where(e => e.id.Equals(id)).FirstOrDefault();
                if (usr == null)
                {
                    pm.kullanici_id = 0;
                }
                else
                {
                    pm.kullanici_id = usr.id;
                }
                db.Entry(pm).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Kullanıcı yetkilendirildi.");
        }
        public static List<MusteriProjeOzetModel> getProjectSurecMusterilerOzet(int proje_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select pm.id, m.ad, m.soyad, m.firma_adi, k.url as kUrl, m.id as musteri_id from musteriler as m "
                + "inner join proje_musteri as pm on pm.musteri_id = m.id "
                + "left join kullanicilar as k on k.flag = " + durumlar.aktif + " and pm.kullanici_id = k.id "
                + "where m.flag = 1 and pm.flag = 1 and pm.proje_id = " + proje_id.ToString() + ";";
            List<MusteriProjeOzetModel> mpList = db.Database.SqlQuery<MusteriProjeOzetModel>(pkQuery).ToList();
            return mpList;
        }
        #endregion proje müşterisi işlemleri

        public static List<GorevlerModel> getProjeGorevleri(int proje_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string pkQuery = "select g.* from gorevler as g "
                + "inner join gorev_proje as gp on gp.flag = " + durumlar.aktif + " and g.id = gp.gorev_id and gp.proje_id = " + proje_id.ToString() + " "
                + "where g.flag = " + durumlar.aktif + " order by g.isim";
            List<GorevlerModel> mpList = db.Database.SqlQuery<GorevlerModel>(pkQuery).ToList();
            return mpList;
        }

        public JsonSonuc projeSurecTamamlandi(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                proje_surec ps = db.proje_surec.Where(e => e.url.Equals(url)).FirstOrDefault();
                if (ps == null)
                {
                    return JsonSonuc.sonucUret(false, "Proje/Süreç bulunamadı.");
                }
                if (ps.yuzde != 100)
                {
                    return JsonSonuc.sonucUret(false, "Proje/Süreç yüzdesi %100 değil.");
                }
                ps.durum = TamamlamaDurumlari.tamamlandi;
                ps.onaylayan_yetkili = lgm.id;
                ps.tamamlanma_tarihi = DateTime.Now;

                List<gorev_proje> projeGorevList = db.gorev_proje.Where(e => e.proje_id == ps.id && e.flag == durumlar.aktif && e.gorevler.flag == durumlar.aktif).ToList();

                foreach (gorev_proje gp in projeGorevList)
                {
                    gp.gorevler.durum = TamamlamaDurumlari.tamamlandi;
                    db.Entry(gp).State = EntityState.Modified;
                }

                db.Entry(ps).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Proje/Süreç durumu güncellendi.");
        }
        public JsonSonuc projeSurecAktiflestir(string url)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                proje_surec ps = db.proje_surec.Where(e => e.url.Equals(url)).FirstOrDefault();
                if (ps == null)
                {
                    return JsonSonuc.sonucUret(false, "Proje/Süreç bulunamadı.");
                }
                ps.durum = TamamlamaDurumlari.basladi;

                List<gorev_proje> projeGorevList = db.gorev_proje.Where(e => e.proje_id == ps.id && e.flag == durumlar.aktif && e.gorevler.flag == durumlar.aktif).ToList();

                foreach (gorev_proje gp in projeGorevList)
                {
                    gp.gorevler.durum = TamamlamaDurumlari.basladi;
                    db.Entry(gp).State = EntityState.Modified;
                }

                db.Entry(ps).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception)
            {
                return JsonSonuc.sonucUret(false, "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
            return JsonSonuc.sonucUret(true, "Proje/Süreç durumu güncellendi.");
        }
    }
}