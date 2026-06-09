using System.Web.Mvc;

namespace WebApplication1.Filtros
{
    public class ValidarPerfilAdmin : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var perfil = filterContext.HttpContext.Session["IdPerfil"];

            if (perfil == null || perfil.ToString() != "1")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    }
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}