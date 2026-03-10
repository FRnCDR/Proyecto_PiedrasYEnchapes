
using System.Web;
using System.Web.Mvc;


namespace WebApplication1.Filtros
{
    public class ValidarSesionAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sesion = HttpContext.Current.Session["UsuarioCorreo"];

            if (sesion == null)
            {
                filterContext.Result = new RedirectResult("~/Home/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
