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
    
    public partial class kullanici_gorev
    {
        public int id { get; set; }
        public int flag { get; set; }
        public int vid { get; set; }
        public int sort { get; set; }
        public System.DateTime date { get; set; }
        public int kullanici_id { get; set; }
        public int gorev_id { get; set; }
        public int ekleyen { get; set; }
    
        public virtual kullanicilar kullanicilar { get; set; }
        public virtual kullanicilar kullanicilar1 { get; set; }
        public virtual gorevler gorevler { get; set; }
    }
}
