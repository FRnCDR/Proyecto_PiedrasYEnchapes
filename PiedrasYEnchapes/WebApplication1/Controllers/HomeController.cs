using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;

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

        [HttpPost]
        public ActionResult Login(Usuario usuario)
        {
            return View();
        }


        [HttpGet]
        public ActionResult Registro()
        {
            return View();

        }

        [HttpPost]
        public ActionResult Registro(Usuario usuario)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                        var resultado = context.CrearUsuarios(
            usuario.Identificacion,
            usuario.Nombre,
            usuario.CorreoElectronico,
            usuario.Contrasenna
                ).FirstOrDefault();

                if (resultado == 1)
                    return RedirectToAction("Login", "Home");


                ViewBag.Mensaje = "Ya existe un usuario con esa identificación o correo.";
                return View(usuario);
            }
        }




        [HttpGet]
        public ActionResult RecuperarAcceso()
        {
            return View();

        }


        [HttpPost]
        public ActionResult RecuperarAcceso(Usuario usuario)
        {
            return View();
        }
    }
}