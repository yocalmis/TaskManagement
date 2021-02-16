using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GorevYoneticisi.Models
{
    public class TempAddingObject
    {
        public string tempGuid;
        public proje_surec projeSurec;
        public List<kullanici_proje> projeKullaniciList = new List<kullanici_proje>();
        public List<proje_musteri> projeMusteriList = new List<proje_musteri>();

        public List<gorevler> gorevList = new List<gorevler>();
        public List<kullanici_gorev> kullaniciGorevList = new List<kullanici_gorev>();
        public List<gorev_musteri> gorevMusteriList = new List<gorev_musteri>();
        public List<yapilacaklar> yapilacaklarList = new List<yapilacaklar>();

        public List<dosyalar> dosyalarList = new List<dosyalar>();
        public List<gorev_dosya> gorevDosyalarList = new List<gorev_dosya>();
        public List<gorev_baglanti> gorevBaglantilari = new List<gorev_baglanti>();
    }
}