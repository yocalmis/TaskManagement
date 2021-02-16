using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GorevYoneticisi.Tools
{
    public class AreaAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string area;
        private readonly string hedefSayfa;
        public AreaAuthorizeAttribute(string area, string hedef)
        {
            this.area = area;
            this.hedefSayfa = hedef;
        }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            DateTime now = DateTime.Now;
            LoggedUserModel usr = GetCurrentUser.GetUser();
            if (usr != null && (usr.kullanici_turu == KullaniciTurleri.super_admin ||  usr.fm != null))
            {
                if (usr.kullanici_turu != KullaniciTurleri.super_admin && !(now >= usr.fm.baslangic_tarihi && now <= usr.fm.bitis_tarihi))
                {
                    filterContext.Result = new RedirectResult("~/Logoff");
                }
                else
                {
                    //int a = GetCurrentUser.GetUserAdmin().user_type;
                    //if (GetCurrentUser.GetUserAdmin().user_type == UserTypes.yonetici)
                    if (usr.kullanici_turu != KullaniciTurleri.super_admin && area.Equals("Admin"))
                    {
                        filterContext.Result = new RedirectResult("~/Admin/Adminlogin/Logoff");
                    }
                    else if (!(usr.kullanici_turu == KullaniciTurleri.super_admin || usr.kullanici_turu == KullaniciTurleri.firma_admin) && area.Equals("Yonetici"))
                    {
                        filterContext.Result = new RedirectResult("~/" + hedefSayfa);
                    }
                    else if (!(usr.kullanici_turu == KullaniciTurleri.super_admin || usr.kullanici_turu == KullaniciTurleri.firma_admin || usr.kullanici_turu == KullaniciTurleri.firma_yetkili) && area.Equals("Yetkili"))
                    {
                        filterContext.Result = new RedirectResult("~/" + hedefSayfa);
                    }
                }
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Logoff");
            }
            base.OnAuthorization(filterContext);
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            string url = "";
            if (area == "Admin")
            {
                if (filterContext.HttpContext.Request.Url.PathAndQuery.Equals(Tools.config.url + "Admin/AHome/Index"))
                {
                    url = Tools.config.url + "Admin/AHome/Index";
                    if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                    {
                        filterContext.Result = new RedirectResult(url);
                        return;
                    }
                }
            }
            else if (area == "Kullanici" || area.Equals("Yetkili") || area.Equals("Yonetici"))
            {
                if (filterContext.HttpContext.Request.Url.PathAndQuery.Equals(Tools.config.url))
                {
                    url = Tools.config.url;
                    if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                    {
                        filterContext.Result = new RedirectResult(url);
                        return;
                    }
                }
            }
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                string loginUrl = "";
                if (area == "Admin")
                {
                    loginUrl = Tools.config.url + "Admin/AdminLogin";
                }
                else if (area == "Kullanici" || area.Equals("Yetkili") || area.Equals("Yonetici"))
                {
                    loginUrl = Tools.config.url;
                }
                filterContext.Result = new RedirectResult(loginUrl);
            }
        }
    }
}