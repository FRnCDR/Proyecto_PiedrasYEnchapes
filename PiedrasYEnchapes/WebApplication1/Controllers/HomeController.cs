using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Login()
        {
             return View();
        }


        [HttpGet]
        public ActionResult Registro()
        {
            return View();

        }

        [HttpGet]
        public ActionResult RecuperarAcceso()
        {
            return View();

        }
    }
}