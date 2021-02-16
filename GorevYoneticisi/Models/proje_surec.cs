//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GorevYoneticisi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class proje_surec
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public proje_surec()
        {
            this.gorev_proje = new HashSet<gorev_proje>();
            this.kullanici_proje = new HashSet<kullanici_proje>();
            this.proje_musteri = new HashSet<proje_musteri>();
        }
    
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int firma_id { get; set; }
        public int ekleyen { get; set; }
        public int tur { get; set; }
        public System.DateTime baslangic_tarihi { get; set; }
        public System.DateTime bitis_tarihi { get; set; }
        public string isim { get; set; }
        public string aciklama { get; set; }
        public int durum { get; set; }
        public int yuzde { get; set; }
        public int periyot_turu { get; set; }
        public int parent_vid { get; set; }
        public string url { get; set; }
        public int mevcut_donem { get; set; }
        public int onaylayan_yetkili { get; set; }
        public System.DateTime tamamlanma_tarihi { get; set; }
    
        public virtual firma_musavir firma_musavir { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<gorev_proje> gorev_proje { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<kullanici_proje> kullanici_proje { get; set; }
        public virtual kullanicilar kullanicilar { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<proje_musteri> proje_musteri { get; set; }
    }
}