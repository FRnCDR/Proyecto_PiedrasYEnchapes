using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PiedrasYEnchapes.UI.Identity.Startup))]
namespace PiedrasYEnchapes.UI.Identity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
