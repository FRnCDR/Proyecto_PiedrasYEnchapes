using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class InventarioController : Controller
    {
        [HttpGet]
        public ActionResult VerInventario()
        {
            return View();
        }
    }
}