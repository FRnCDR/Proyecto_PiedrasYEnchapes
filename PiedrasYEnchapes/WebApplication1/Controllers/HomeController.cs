using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Filtros;
using WebApplication1.Helpers;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    
    public class HomeController : Controller
    {
        [HttpGet]
        [ValidarSesion]
        public ActionResult Index()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var productos = context.tbProductos
                    .Where(p => p.Estado == true) // si manejas estado
                    .OrderByDescending(p => p.ProductoID)
                    .Take(9)
                    .ToList();

                return View(productos);
            }
        }
        [HttpGet]
        public ActionResult Login()
        {
             return View();
        }


        [HttpPost]
        public ActionResult Login(Usuario usuario)
        {
            // Identificamos el intento por correo + IP para limitar la fuerza bruta.
            var ip = Request.UserHostAddress;

            // Si la cuenta/IP está bloqueada por demasiados intentos fallidos, no seguimos.
            if (LoginThrottleHelper.EstaBloqueado(usuario.CorreoElectronico, ip, out int minutosRestantes))
            {
                ViewBag.Mensaje = $"Demasiados intentos fallidos. Intente nuevamente en {minutosRestantes} minuto(s).";
                return View(usuario);
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var usuarioDb = context.tbUsuario
                    .FirstOrDefault(u => u.CorreoElectronico == usuario.CorreoElectronico);

                if (usuarioDb != null &&
                    PasswordHelper.VerifyPassword(usuario.Contrasenna, usuarioDb.Contrasenna))
                {
                    // Inicio de sesión correcto: se limpia el conteo de intentos.
                    LoginThrottleHelper.RegistrarExito(usuario.CorreoElectronico, ip);

                    Session["UsuarioCorreo"] = usuarioDb.CorreoElectronico;
                    Session["Nombre"] = usuarioDb.Nombre;
                    Session["IdUsuario"] = usuarioDb.IdUsuario;
                    Session["IdPerfil"] = usuarioDb.IdPerfil;

                    return RedirectToAction("Index", "Home");
                }

                // Credenciales incorrectas: se registra el intento fallido.
                LoginThrottleHelper.RegistrarFallo(usuario.CorreoElectronico, ip);

                // Volvemos a evaluar por si este fallo activó el bloqueo.
                if (LoginThrottleHelper.EstaBloqueado(usuario.CorreoElectronico, ip, out int minRestantes))
                {
                    ViewBag.Mensaje = $"Demasiados intentos fallidos. Intente nuevamente en {minRestantes} minuto(s).";
                    return View(usuario);
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
            PasswordHelper.HashPassword(usuario.Contrasenna)
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

                    // 2) actualizar en BD
                    userDb.Contrasenna = PasswordHelper.HashPassword(passTemp);

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