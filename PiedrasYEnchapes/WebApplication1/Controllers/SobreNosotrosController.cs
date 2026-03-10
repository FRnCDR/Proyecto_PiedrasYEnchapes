using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Filtros;


namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class SobreNosotrosController : Controller
    {
        [HttpGet]
        public ActionResult VerNosotros()
        {
            return View();
        }
    }
}