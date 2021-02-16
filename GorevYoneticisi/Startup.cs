using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GorevYoneticisi.Startup))]
namespace GorevYoneticisi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
