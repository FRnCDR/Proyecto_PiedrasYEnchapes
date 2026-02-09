using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Services;
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
            using (var context = new DATABASE_PYEEntities())
            {
                int resultado = context.LoginUsuario(
                    usuario.CorreoElectronico,
                    usuario.Contrasenna
                ).FirstOrDefault() ?? 0; 

                if (resultado == 1)
                {
                    Session["UsuarioCorreo"] = usuario.CorreoElectronico;
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Mensaje = "Correo o contraseña incorrectos.";
                return View(usuario);
            }
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
            using (var context = new DATABASE_PYEEntities())
            {
                // buscar usuario por correo (tbUsuario del EF)
                var userDb = context.tbUsuario
                    .FirstOrDefault(u => u.CorreoElectronico == usuario.CorreoElectronico);

                if (userDb != null)
                {
                    // 1) generar contraseña temporal
                    var passTemp = RecuperacionService.GenerarContrasenna();

                    // 2) actualizar en BD (texto plano como tu guía)
                    userDb.Contrasenna = passTemp;

                    var filas = context.SaveChanges();

                    if (filas > 0)
                    {
                        // 3) enviar correo
                        RecuperacionService.EnviarCorreoRecuperacion(
                            "Recuperación de acceso - Piedras Decorativas",
                            passTemp,
                            userDb.CorreoElectronico
                        );

                        ViewBag.MensajeOK = "Te enviamos una contraseña temporal a tu correo.";
                        return View(); // o RedirectToAction("Login","Home");
                    }
                }

                ViewBag.Mensaje = "No se encontró un usuario con ese correo o no se pudo restablecer.";
                return View(usuario);
            }
        }








        // ----------------- CONSULTA CÉDULA  -----------------
        [HttpGet]
        public async Task<ActionResult> ConsultarCedula(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { ok = false, mensaje = "Cédula vacía" }, JsonRequestBehavior.AllowGet);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var http = new HttpClient())
            {
                var resp = await http.GetAsync("https://apis.gometa.org/cedulas/" + id);
                if (!resp.IsSuccessStatusCode)
                    return Json(new { ok = false, mensaje = "No se pudo consultar la cédula" }, JsonRequestBehavior.AllowGet);

                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
        }


        [HttpGet]
        public ActionResult CerrarSesion()
        {
            Session.Clear();
            return RedirectToAction("Login", "Home");

        }
    }
}