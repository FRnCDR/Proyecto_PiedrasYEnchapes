using System.Web;
using System.Web.Mvc;

namespace PiedrasYEnchapes.UI.Identity
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
