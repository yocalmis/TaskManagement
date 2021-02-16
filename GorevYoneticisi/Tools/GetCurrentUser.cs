using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace GorevYoneticisi.Tools
{
    public class GetCurrentUser
    {
        public static LoggedUserModel GetUser()
        {
            try
            {
                LoggedUserModel usr = null;
                HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    // Get the forms authentication ticket.
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var identity = new System.Security.Principal.GenericIdentity(authTicket.Name, "Forms");
                    //var principal = new System.Security.Principal.IPrincipal(identity);

                    // Get the custom user data encrypted in the ticket.
                    string userData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;

                    // Deserialize the json data and set it on the custom principal.
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    usr = (LoggedUserModel)serializer.Deserialize(userData, typeof(LoggedUserModel));
                }
                return usr;
            }
            catch (Exception ex)
            {
                //System.Web.HttpContext.Current.Response.Redirect(Tools.config.url + "admin/login/logoff");
                return null;
            }
        }
    }
}