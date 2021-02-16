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
    public class surecIslemleri
    {
        public string yeniSurec(int firma_id, HttpRequestBase Request)
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

                proje_surec src = new proje_surec();
                foreach (var property in src.GetType().GetProperties())
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
                            PropertyInfo propertyS = src.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(src, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(src, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(src, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(src, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(src.isim);
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
                        tempUrl = createdUrl + i.ToString();
                    }
                    else
                    {
                        createdUrl = tempUrl;
                        bulundu = true;
                    }
                    i++;
                } while (!bulundu);
                src.url = createdUrl;
                src.firma_id = firma_id;

                src.parent_vid = vid;
                src.flag = durumlar.aktif;
                src.date = DateTime.Now;
                src.vid = vid;
                src.ekleyen = GetCurrentUser.GetUser().id;
                src.sort = sort;
                src.mevcut_donem = 1;//ilk dönem bu her dönem artışında artacak
                src.durum = TamamlamaDurumlari.bekliyor;
                src.tur = ProjeSurecTur.surec;
                src.tamamlanma_tarihi = DateTime.Now;

                string isimControl = "select * from proje_surec where tur = " + ProjeSurecTur.surec + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + src.isim + "' and firma_id = " + src.firma_id + " and date_format(baslangic_tarihi, '%Y-%m-%d') = '" + src.baslangic_tarihi.ToString("yyyy-MM-dd") + "'";
                ProjeSurecModel isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                if (isimKontrolPs != null)
                {
                    return "surec_isim_hatasi";
                }

                bool kullaniciKontrol = firmaSurecKontrol(src.firma_id, src.id).Result;
                if (!kullaniciKontrol)
                {
                    return "surec_sayisi_hatasi";
                }

                db.proje_surec.Add(src);
                db.SaveChanges();

                /*int musteri_id = Convert.ToInt32(Request["musteri_id"].ToString());
                if (musteri_id != 0)
                {
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == src.vid).FirstOrDefault();
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
                    proje_surec dbPs = db.proje_surec.Where(e => e.vid == src.vid).FirstOrDefault();
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault();
                    if(pm != null)
                    {
                        pm.flag = durumlar.silindi;
                        db.Entry(pm).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }*/

                return src.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public void tekrarlananSurecKontrolu()
        {           

            vrlfgysdbEntities db = new vrlfgysdbEntities();
            string querySurecler = "select * from (select * from proje_surec where tur = " + ProjeSurecTur.surec + " order by mevcut_donem desc) as orderedTable group by parent_vid";
            List<ProjeSurecModel> surecList = db.Database.SqlQuery<ProjeSurecModel>(querySurecler).ToList();

            DateTime now = DateTime.Now;

            bool degisiklikYapildi = false;

            foreach (ProjeSurecModel src in surecList)
            {
                degisiklikYapildi = false;
                //TimeSpan dateDifference = src.baslangic_tarihi - now;
                var dateSpan = DateTimeFunctions.DateTimeSpan.CompareDates(src.baslangic_tarihi, now);
                if (src.flag != durumlar.aktif || src.durum == TamamlamaDurumlari.pasif)
                {
                    continue;
                }
                if (src.periyot_turu == SurecPeriyotTurleri.gun && dateSpan.Days >= 1)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.hafta && dateSpan.Days >= 7)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay && dateSpan.Months >= 1)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay3 && dateSpan.Months >= 3)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay6 && dateSpan.Months >= 6)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.yil && dateSpan.Years >= 1)
                {
                    degisiklikYapildi = true;
                    tekrarlananSurec(src);
                }
            }
            if (degisiklikYapildi)
            {
                tekrarlananSurecKontrolu();
            }
        }
        public async Task<string> tekrarlananSurec(ProjeSurecModel surec)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
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
                
                proje_surec src = new proje_surec();
                foreach (var property in src.GetType().GetProperties())
                {
                    try
                    {
                        var response = surec.GetType().GetProperty(property.Name).GetValue(surec).ToString();
                        if (response == null && property.PropertyType != typeof(int))
                        {
                            if (response == null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            PropertyInfo propertyS = src.GetType().GetProperty(property.Name);
                            if (property.PropertyType == typeof(decimal))
                            {
                                propertyS.SetValue(src, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                if (response == null)
                                {
                                    propertyS.SetValue(src, Convert.ChangeType(0, property.PropertyType), null);
                                }
                                else
                                {
                                    propertyS.SetValue(src, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }

                            }
                            else
                            {
                                propertyS.SetValue(src, Convert.ChangeType(response, property.PropertyType), null);
                            }
                        }
                    }
                    catch (Exception e)
                    { }
                }

                string strImageName = StringFormatter.OnlyEnglishChar(src.isim);
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
                        tempUrl = createdUrl + i.ToString();
                    }
                    else
                    {
                        createdUrl = tempUrl;
                        bulundu = true;
                    }
                    i++;
                } while (!bulundu);
                src.url = createdUrl;

                if (src.periyot_turu == SurecPeriyotTurleri.gun)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddDays(1);
                    src.bitis_tarihi = surec.bitis_tarihi.AddDays(1);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.hafta)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddDays(7);
                    src.bitis_tarihi = surec.bitis_tarihi.AddDays(7);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddMonths(1);
                    src.bitis_tarihi = surec.bitis_tarihi.AddMonths(1);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay3)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddMonths(3);
                    src.bitis_tarihi = surec.bitis_tarihi.AddMonths(3);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.ay6)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddMonths(6);
                    src.bitis_tarihi = surec.bitis_tarihi.AddMonths(6);
                }
                else if (src.periyot_turu == SurecPeriyotTurleri.yil)
                {
                    src.baslangic_tarihi = surec.baslangic_tarihi.AddYears(1);
                    src.bitis_tarihi = surec.bitis_tarihi.AddYears(1);
                }

                src.yuzde = 0;
                src.parent_vid = surec.parent_vid;
                src.vid = vid;
                src.ekleyen = lgm.id;
                src.sort = sort;
                src.durum = TamamlamaDurumlari.bekliyor;
                src.mevcut_donem = surec.mevcut_donem + 1;//ilk dönem bu her dönem artışında artacak

                ProjeSurecModel isimKontrolPs = null;
                int isimIndex = 1;
                string tempIsim = src.isim;
                do
                {
                    string isimControl = "select * from proje_surec where tur = " + ProjeSurecTur.surec + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + src.isim + "' and firma_id = " + src.firma_id + " and date_format(baslangic_tarihi, '%Y-%m-%d') = '" + src.baslangic_tarihi.ToString("yyyy-MM-dd") + "'";
                    isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                    if (isimKontrolPs != null)
                    {
                        src.isim = tempIsim + "-" + isimIndex.ToString();
                        isimIndex++;
                    }
                } while (isimKontrolPs != null);

                bool kullaniciKontrol = firmaSurecKontrol(src.firma_id, src.id).Result;
                if (!kullaniciKontrol && src.mevcut_donem == 1)
                {
                    return "surec_sayisi_hatasi";
                }

                db.proje_surec.Add(src);
                db.SaveChanges();

                proje_surec dbSurec = db.proje_surec.Where(e => e.vid == vid).FirstOrDefault();

                var kp = db.kullanici_proje.Where(e => e.flag == durumlar.aktif && e.proje_id == surec.id).ToListAsync();
                var pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == surec.id).ToListAsync();
                var gp = db.gorev_proje.Where(e => e.flag == durumlar.aktif && e.proje_id == surec.id).ToListAsync();

                await Task.WhenAll(kp, pm, gp);

                List<kullanici_proje> kullaniciProjeList = kp.Result;
                List<proje_musteri> projeMusteriList = pm.Result;
                List<gorev_proje> gorevProjeList = gp.Result;

                #region kullanıcıları kopyala
                foreach (kullanici_proje kullaniciPrj in kullaniciProjeList)
                {
                    if (kullaniciPrj.kullanicilar.flag == durumlar.silindi)
                    {
                        continue;
                    }
                    kullanici_proje yeniKPrj = new kullanici_proje();
                    foreach (var property in yeniKPrj.GetType().GetProperties())
                    {
                        try
                        {
                            var response = kullaniciPrj.GetType().GetProperty(property.Name).GetValue(kullaniciPrj).ToString();
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = yeniKPrj.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(yeniKPrj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(yeniKPrj, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniKPrj, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(yeniKPrj, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                    }
                    int vidKprj = 1;
                    if (db.kullanici_proje.Count() != 0)
                    {
                        vidKprj = db.kullanici_proje.Max(e => e.vid) + 1;
                    }
                    int sortKprj = 1;
                    if (db.kullanici_proje.Count() != 0)
                    {
                        sortKprj = db.kullanici_proje.Max(e => e.sort) + 1;
                    }
                    yeniKPrj.id = 0;
                    yeniKPrj.vid = vidKprj;
                    yeniKPrj.sort = sortKprj;
                    yeniKPrj.proje_id = dbSurec.id;
                    yeniKPrj.date = DateTime.Now;

                    db.kullanici_proje.Add(yeniKPrj);
                    db.SaveChanges();

                }
                #endregion kullanıcıları kopyala
                #region musterileri kopyala
                foreach (proje_musteri prjMusteri in projeMusteriList)
                {
                    if (prjMusteri.musteriler.flag == durumlar.silindi)
                    {
                        continue;
                    }
                    proje_musteri yeniPrjM = new proje_musteri();
                    foreach (var property in yeniPrjM.GetType().GetProperties())
                    {
                        try
                        {
                            var response = prjMusteri.GetType().GetProperty(property.Name).GetValue(prjMusteri).ToString();
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = yeniPrjM.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(yeniPrjM, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(yeniPrjM, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniPrjM, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(yeniPrjM, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                    }
                    int vidKprj = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        vidKprj = db.proje_musteri.Max(e => e.vid) + 1;
                    }
                    int sortKprj = 1;
                    if (db.proje_musteri.Count() != 0)
                    {
                        sortKprj = db.proje_musteri.Max(e => e.sort) + 1;
                    }

                    yeniPrjM.id = 0;
                    yeniPrjM.vid = vidKprj;
                    yeniPrjM.sort = sortKprj;
                    yeniPrjM.proje_id = dbSurec.id;
                    yeniPrjM.date = DateTime.Now;

                    db.proje_musteri.Add(yeniPrjM);
                    db.SaveChanges();
                }
                #endregion musterileri kopyala

                List<gorevBaglantiEskiYeniInts> gorevLogList = new List<gorevBaglantiEskiYeniInts>();
                foreach (gorev_proje gorevPrj in gorevProjeList)
                {
                    if (gorevPrj.gorevler.flag == durumlar.silindi)
                    {
                        continue;
                    }
                    gorevler eskiGorev = gorevPrj.gorevler;

                    #region gorevin eklenmesi
                    gorevler yeniGrv = new gorevler();
                    foreach (var property in yeniGrv.GetType().GetProperties())
                    {
                        try
                        {
                            var response = eskiGorev.GetType().GetProperty(property.Name).GetValue(eskiGorev).ToString();
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = yeniGrv.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(yeniGrv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(yeniGrv, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniGrv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(yeniGrv, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                    }

                    string strImageNameGrv = StringFormatter.OnlyEnglishChar(yeniGrv.isim);
                    string createdUrlGrv = strImageNameGrv;
                    string tempUrlGrv = createdUrlGrv;
                    bool bulunduGrv = false;
                    int iGrv = 0;
                    gorevler pgGrv = new gorevler();
                    do
                    {
                        pgGrv = db.gorevler.Where(e => e.url.Equals(tempUrlGrv)).FirstOrDefault();
                        if (pgGrv != null)
                        {
                            tempUrlGrv = createdUrlGrv + iGrv.ToString();
                        }
                        else
                        {
                            createdUrlGrv = tempUrlGrv;
                            bulunduGrv = true;
                        }
                        iGrv++;
                    } while (!bulunduGrv);
                    yeniGrv.url = createdUrlGrv;

                    int vidGrv = 1;
                    if (db.gorevler.Count() != 0)
                    {
                        vidGrv = db.gorevler.Max(e => e.vid) + 1;
                    }
                    int sortGrv = 1;
                    if (db.gorevler.Count() != 0)
                    {
                        sortGrv = db.gorevler.Max(e => e.sort) + 1;
                    }

                    yeniGrv.id = 0;
                    yeniGrv.vid = vidGrv;
                    yeniGrv.sort = sortGrv;
                    yeniGrv.date = DateTime.Now;
                    yeniGrv.yuzde = 0;
                    yeniGrv.durum = TamamlamaDurumlari.bekliyor;

                    if (dbSurec.periyot_turu == SurecPeriyotTurleri.gun)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddDays(1);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddDays(1);
                    }
                    else if (dbSurec.periyot_turu == SurecPeriyotTurleri.hafta)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddDays(7);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddDays(7);
                    }
                    else if (dbSurec.periyot_turu == SurecPeriyotTurleri.ay)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddMonths(1);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddMonths(1);
                    }
                    else if (dbSurec.periyot_turu == SurecPeriyotTurleri.ay3)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddMonths(3);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddMonths(3);
                    }
                    else if (dbSurec.periyot_turu == SurecPeriyotTurleri.ay6)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddMonths(6);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddMonths(6);
                    }
                    else if (dbSurec.periyot_turu == SurecPeriyotTurleri.yil)
                    {
                        yeniGrv.baslangic_tarihi = eskiGorev.baslangic_tarihi.AddYears(1);
                        yeniGrv.bitis_tarihi = eskiGorev.bitis_tarihi.AddYears(1);
                    }

                    db.gorevler.Add(yeniGrv);
                    db.SaveChanges();

                    gorevler dbGrv2 = db.gorevler.Where(e => e.vid == yeniGrv.vid).FirstOrDefault();
                    gorevIslemleri.logEkle(dbGrv2, "Görev oluşturuldu.", lgm);
                    #endregion gorevin eklenmesi

                    gorevler dbGrv = db.gorevler.Where(e => e.vid == vidGrv).FirstOrDefault();

                    #region gorev_proje eklenmesi
                    gorev_proje yeniGrvP = new gorev_proje();
                    foreach (var property in yeniGrvP.GetType().GetProperties())
                    {
                        try
                        {
                            var response = gorevPrj.GetType().GetProperty(property.Name).GetValue(gorevPrj).ToString();
                            if (response == null && property.PropertyType != typeof(int))
                            {
                                if (response == null)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                PropertyInfo propertyS = yeniGrvP.GetType().GetProperty(property.Name);
                                if (property.PropertyType == typeof(decimal))
                                {
                                    propertyS.SetValue(yeniGrvP, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                }
                                else if (property.PropertyType == typeof(int))
                                {
                                    if (response == null)
                                    {
                                        propertyS.SetValue(yeniGrvP, Convert.ChangeType(0, property.PropertyType), null);
                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniGrvP, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }

                                }
                                else
                                {
                                    propertyS.SetValue(yeniGrvP, Convert.ChangeType(response, property.PropertyType), null);
                                }
                            }
                        }
                        catch (Exception e)
                        { }
                    }
                    int vidKprj = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        vidKprj = db.gorev_proje.Max(e => e.vid) + 1;
                    }
                    int sortKprj = 1;
                    if (db.gorev_proje.Count() != 0)
                    {
                        sortKprj = db.gorev_proje.Max(e => e.sort) + 1;
                    }

                    yeniGrvP.vid = vidKprj;
                    yeniGrvP.sort = sortKprj;
                    yeniGrvP.proje_id = dbSurec.id;
                    yeniGrvP.gorev_id = dbGrv.id;
                    yeniGrvP.date = DateTime.Now;

                    db.gorev_proje.Add(yeniGrvP);
                    db.SaveChanges();
                    #endregion gorev_proje eklenmesi

                    #region yapılacaklar eklenmesi
                    List<yapilacaklar> yapilacakList = eskiGorev.yapilacaklar.Where(e => e.flag == durumlar.aktif).ToList();
                    foreach (yapilacaklar yplck in yapilacakList)
                    {
                        yapilacaklar yeniYplck = new yapilacaklar();
                        foreach (var property in yeniYplck.GetType().GetProperties())
                        {
                            try
                            {
                                var response = yplck.GetType().GetProperty(property.Name).GetValue(yplck).ToString();
                                if (response == null && property.PropertyType != typeof(int))
                                {
                                    if (response == null)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    PropertyInfo propertyS = yeniYplck.GetType().GetProperty(property.Name);
                                    if (property.PropertyType == typeof(decimal))
                                    {
                                        propertyS.SetValue(yeniYplck, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }
                                    else if (property.PropertyType == typeof(int))
                                    {
                                        if (response == null)
                                        {
                                            propertyS.SetValue(yeniYplck, Convert.ChangeType(0, property.PropertyType), null);
                                        }
                                        else
                                        {
                                            propertyS.SetValue(yeniYplck, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                        }

                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniYplck, Convert.ChangeType(response, property.PropertyType), null);
                                    }
                                }
                            }
                            catch (Exception e)
                            { }
                        }
                        int vidYplck = 1;
                        if (db.yapilacaklar.Count() != 0)
                        {
                            vidYplck = db.yapilacaklar.Max(e => e.vid) + 1;
                        }
                        int sortYplck = 1;
                        if (db.yapilacaklar.Count() != 0)
                        {
                            sortYplck = db.yapilacaklar.Max(e => e.sort) + 1;
                        }

                        string strImageNameYplck = StringFormatter.OnlyEnglishChar(yeniYplck.isim);
                        string createdUrlYplck = strImageNameYplck;
                        string tempUrlYplck = createdUrl;
                        bool bulunduYplck = false;
                        int iYplck = 0;
                        yapilacaklar pgYplck = new yapilacaklar();
                        do
                        {
                            pgYplck = db.yapilacaklar.Where(e => e.url.Equals(tempUrlYplck)).FirstOrDefault();
                            if (pgYplck != null)
                            {
                                tempUrlYplck = createdUrlYplck + iYplck.ToString();
                            }
                            else
                            {
                                createdUrlYplck = tempUrlYplck;
                                bulunduYplck = true;
                            }
                            iYplck++;
                        } while (!bulunduYplck);
                        yeniYplck.url = createdUrlYplck;

                        yeniYplck.durum = YapilacaklarDurum.beklemede;
                        yeniYplck.gerceklestiren_id = 0;
                        yeniYplck.id = 0;
                        yeniYplck.vid = vidYplck;
                        yeniYplck.sort = sortYplck;
                        yeniYplck.gorev_id = dbGrv.id;
                        yeniYplck.date = DateTime.Now;

                        db.yapilacaklar.Add(yeniYplck);
                        db.SaveChanges();
                    }
                    #endregion yapılacaklar eklenmesi

                    #region gorev kullanıcıları eklenmesi
                    List<kullanici_gorev> gorevKullaniciList = eskiGorev.kullanici_gorev.Where(e => e.flag == durumlar.aktif).ToList();
                    foreach (kullanici_gorev kgrv in gorevKullaniciList)
                    {
                        kullanici_gorev yeniKgrv = new kullanici_gorev();
                        foreach (var property in yeniKgrv.GetType().GetProperties())
                        {
                            try
                            {
                                var response = kgrv.GetType().GetProperty(property.Name).GetValue(kgrv).ToString();
                                if (response == null && property.PropertyType != typeof(int))
                                {
                                    if (response == null)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    PropertyInfo propertyS = yeniKgrv.GetType().GetProperty(property.Name);
                                    if (property.PropertyType == typeof(decimal))
                                    {
                                        propertyS.SetValue(yeniKgrv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }
                                    else if (property.PropertyType == typeof(int))
                                    {
                                        if (response == null)
                                        {
                                            propertyS.SetValue(yeniKgrv, Convert.ChangeType(0, property.PropertyType), null);
                                        }
                                        else
                                        {
                                            propertyS.SetValue(yeniKgrv, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                        }

                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniKgrv, Convert.ChangeType(response, property.PropertyType), null);
                                    }
                                }
                            }
                            catch (Exception e)
                            { }
                        }
                        int vidYplck = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            vidYplck = db.kullanici_gorev.Max(e => e.vid) + 1;
                        }
                        int sortYplck = 1;
                        if (db.kullanici_gorev.Count() != 0)
                        {
                            sortYplck = db.kullanici_gorev.Max(e => e.sort) + 1;
                        }

                        yeniKgrv.id = 0;
                        yeniKgrv.vid = vidYplck;
                        yeniKgrv.sort = sortYplck;
                        yeniKgrv.gorev_id = dbGrv.id;
                        yeniKgrv.date = DateTime.Now;

                        db.kullanici_gorev.Add(yeniKgrv);
                        db.SaveChanges();
                    }
                    #endregion gorev kullanıcıları eklenmesi

                    #region gorev müşterileri eklenmesi
                    List<gorev_musteri> gorevMusteriList = eskiGorev.gorev_musteri.Where(e => e.flag == durumlar.aktif).ToList();
                    foreach (gorev_musteri grvM in gorevMusteriList)
                    {
                        gorev_musteri yeniGrvM = new gorev_musteri();
                        foreach (var property in yeniGrvM.GetType().GetProperties())
                        {
                            try
                            {
                                var response = grvM.GetType().GetProperty(property.Name).GetValue(grvM).ToString();
                                if (response == null && property.PropertyType != typeof(int))
                                {
                                    if (response == null)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    PropertyInfo propertyS = yeniGrvM.GetType().GetProperty(property.Name);
                                    if (property.PropertyType == typeof(decimal))
                                    {
                                        propertyS.SetValue(yeniGrvM, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }
                                    else if (property.PropertyType == typeof(int))
                                    {
                                        if (response == null)
                                        {
                                            propertyS.SetValue(yeniGrvM, Convert.ChangeType(0, property.PropertyType), null);
                                        }
                                        else
                                        {
                                            propertyS.SetValue(yeniGrvM, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                        }

                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniGrvM, Convert.ChangeType(response, property.PropertyType), null);
                                    }
                                }
                            }
                            catch (Exception e)
                            { }
                        }
                        int vidYplck = 1;
                        if (db.gorev_musteri.Count() != 0)
                        {
                            vidYplck = db.gorev_musteri.Max(e => e.vid) + 1;
                        }
                        int sortYplck = 1;
                        if (db.gorev_musteri.Count() != 0)
                        {
                            sortYplck = db.gorev_musteri.Max(e => e.sort) + 1;
                        }

                        yeniGrvM.id = 0;
                        yeniGrvM.vid = vidYplck;
                        yeniGrvM.sort = sortYplck;
                        yeniGrvM.gorev_id = dbGrv.id;
                        yeniGrvM.date = DateTime.Now;

                        db.gorev_musteri.Add(yeniGrvM);
                        db.SaveChanges();
                    }
                    #endregion gorev müşterileri eklenmesi

                    #region gorev baglantilari
		            gorevBaglantiEskiYeniInts gorevBagEskiYeniInt = new gorevBaglantiEskiYeniInts();
                    gorevBagEskiYeniInt.gorev_eski_id = eskiGorev.id;
                    gorevBagEskiYeniInt.gorev_yeni_id = dbGrv.id;
                    gorevLogList.Add(gorevBagEskiYeniInt);

                    List<gorev_baglanti> eskiBaglantiList = eskiGorev.gorev_baglanti1.Where(e => e.flag == durumlar.aktif).ToList();
                    foreach (gorev_baglanti bag in eskiBaglantiList)
                    {
                        gorev_baglanti yeniBag = new gorev_baglanti();
                        foreach (var property in yeniBag.GetType().GetProperties())
                        {
                            try
                            {
                                var response = bag.GetType().GetProperty(property.Name).GetValue(bag).ToString();
                                if (response == null && property.PropertyType != typeof(int))
                                {
                                    if (response == null)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    PropertyInfo propertyS = yeniBag.GetType().GetProperty(property.Name);
                                    if (property.PropertyType == typeof(decimal))
                                    {
                                        propertyS.SetValue(yeniBag, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                    }
                                    else if (property.PropertyType == typeof(int))
                                    {
                                        if (response == null)
                                        {
                                            propertyS.SetValue(yeniBag, Convert.ChangeType(0, property.PropertyType), null);
                                        }
                                        else
                                        {
                                            propertyS.SetValue(yeniBag, Convert.ChangeType(Decimal.Parse(response.Replace('.', ',')), property.PropertyType), null);
                                        }

                                    }
                                    else
                                    {
                                        propertyS.SetValue(yeniBag, Convert.ChangeType(response, property.PropertyType), null);
                                    }
                                }
                            }
                            catch (Exception e)
                            { }
                        }
                        int vidYplck = 1;
                        if (db.gorev_baglanti.Count() != 0)
                        {
                            vidYplck = db.gorev_baglanti.Max(e => e.vid) + 1;
                        }
                        int sortYplck = 1;
                        if (db.gorev_baglanti.Count() != 0)
                        {
                            sortYplck = db.gorev_baglanti.Max(e => e.sort) + 1;
                        }

                        yeniBag.id = 0;
                        yeniBag.vid = vidYplck;
                        yeniBag.sort = sortYplck;
                        yeniBag.gorev_id = dbGrv.id;
                        yeniBag.date = DateTime.Now;

                        gorevBaglantiEskiYeniInts gorevBagBilgisi = gorevLogList.Where(e => e.gorev_eski_id == yeniBag.bagli_gorev).FirstOrDefault();
                        if (gorevBagBilgisi == null)
	                    {
		 
	                    }
                        yeniBag.bagli_gorev = gorevBagBilgisi.gorev_yeni_id;
                        gorevIslemleri gis = new gorevIslemleri();
                        gis.gorevBaglantiEkle(yeniBag);
                    }
                    #endregion gorev baglantilari
                }

                return src.url;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public string surecDuzenle(string url, int firma_id, HttpRequestBase Request)
        {
            try
            {
                vrlfgysdbEntities db = new vrlfgysdbEntities();

                proje_surec dbPrj = db.proje_surec.Where(e => e.url.Equals(url) && e.flag != durumlar.silindi).FirstOrDefault();

                if (dbPrj == null || url == null || url.Equals(""))
                {
                    return yeniSurec(firma_id, Request);
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

                string isimControl = "select * from proje_surec where tur = " + ProjeSurecTur.surec + " and id != " + dbPrj.id + " and flag != " + durumlar.silindi.ToString() + " and isim = '" + dbPrj.isim + "' and firma_id = " + dbPrj.firma_id + " and date_format(baslangic_tarihi, '%Y-%m-%d') = '" + dbPrj.baslangic_tarihi.ToString("yyyy-MM-dd") + "'";
                ProjeSurecModel isimKontrolPs = db.Database.SqlQuery<ProjeSurecModel>(isimControl).FirstOrDefault();
                if (isimKontrolPs != null)
                {
                    return "surec_isim_hatasi";
                }

                bool kullaniciKontrol = firmaSurecKontrol(dbPrj.firma_id, dbPrj.id).Result;
                if (!kullaniciKontrol)
                {
                    return "surec_sayisi_hatasi";
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
                    proje_musteri pm = db.proje_musteri.Where(e => e.flag == durumlar.aktif && e.proje_id == dbPs.id).FirstOrDefault(); if (pm != null && pm.musteri_id != musteri_id)
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
        public static async Task<bool> firmaSurecKontrol(int firma_id, int musteri_id)
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            if (firma_id == 0)
            {
                return true;
            }
            var f = db.firma_musavir.Where(e => e.flag == durumlar.aktif && e.id == firma_id).FirstOrDefaultAsync();
            string queryMusteriCount = "select count(id) from proje_surec where tur = " + ProjeSurecTur.surec + " and flag != " + durumlar.silindi.ToString() + " and firma_id = " + firma_id.ToString() + " and id != " + musteri_id + " and mevcut_donem = 1";
            var kc = db.Database.SqlQuery<int>(queryMusteriCount).FirstOrDefaultAsync();

            await Task.WhenAll(f, kc);

            firma_musavir fm = f.Result;
            int surecCount = kc.Result;

            if (fm == null)
            {
                return false;
            }
            else if (fm.surec_sayisi > surecCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public JsonSonuc silSurec(string url, int firma_id)
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
                    return JsonSonuc.sonucUret(false, "Süreç bulunamadı.");
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
            return JsonSonuc.sonucUret(true, "Süreç pasif edildi.");
        }
        public JsonSonuc surecYuzdesiDuzenle(int proje_id)
        {
            try
            {
                LoggedUserModel lgm = GetCurrentUser.GetUser();
                vrlfgysdbEntities db = new vrlfgysdbEntities();
                proje_surec src = db.proje_surec.Where(e => e.id == proje_id).FirstOrDefault();
                List<gorev_proje> surecGorevList = db.gorev_proje.Where(e => e.proje_id == proje_id && e.flag == durumlar.aktif).ToList();

                int toplam = 0;
                int aktifGorevler = 0;
                foreach (gorev_proje pg in surecGorevList)
                {
                    if (pg.gorevler.flag == durumlar.aktif)
                    {
                        toplam += pg.gorevler.yuzde;
                        aktifGorevler++;
                    }
                }

                if (aktifGorevler != 0)
                {
                    src.yuzde = toplam / aktifGorevler;
                }
                else
                {
                    src.yuzde = toplam;
                }

                src.durum = TamamlamaDurumlari.basladi;

                db.Entry(src).State = EntityState.Modified;
                db.SaveChanges();

                if (src.yuzde == 100)
                {
                    List<kullanicilar> yetkiliList = db.kullanicilar.Where(e => e.flag == durumlar.aktif && e.firma_id == lgm.firma_id && e.kullanici_turu <= KullaniciTurleri.firma_yetkili).ToList();
                    foreach (kullanicilar usr in yetkiliList)
                    {
                        bildirimIslemleri.yeniBildirim(usr.id, BildirimTurleri.surec, src.id, "", src.isim + "(" + src.baslangic_tarihi.ToString("dd.MM.yyyy") + ") isimli süreç tamamlandı ve onayınızı bekliyor.");
                    }
                }

                return JsonSonuc.sonucUret(true, src.yuzde);
            }
            catch (Exception e)
            {
                return JsonSonuc.sonucUret(false, "Sürecin yüzdesini düzenlerken bir hata oluştu.");
            }
        }
    }
}