using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PiedrasYEnchapes.UI.Startup))]
namespace PiedrasYEnchapes.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
