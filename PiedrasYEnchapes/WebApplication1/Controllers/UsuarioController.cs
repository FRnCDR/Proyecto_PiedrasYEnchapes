using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UsuarioController : Controller
    {
        [HttpGet]
        public ActionResult VerPerfil()
        {
            // Si no lo estás usando, puedes borrarlo.
            // Si lo vas a usar luego, puedes replicar la misma lógica de EditarPerfil pero solo lectura.
            return View();
        }

        // GET: Usuario/EditarPerfil
        [HttpGet]
        public ActionResult EditarPerfil()
        {
            // ✅ URL del endpoint que ya tienes en HomeController
            ViewBag.UrlConsultarCedula = Url.Action("ConsultarCedula", "Home");

            // Mensajes del POST
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.TipoMensaje = TempData["TipoMensaje"];

            using (var context = new DATABASE_PYEEntities())
            {
                // ✅ En tu Login guardas esto:
                // Session["UsuarioCorreo"] = usuario.CorreoElectronico;
                var correoSesion = Session["UsuarioCorreo"]?.ToString();

                if (string.IsNullOrWhiteSpace(correoSesion))
                {
                    TempData["Mensaje"] = "No hay sesión activa. Inicia sesión nuevamente.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Login", "Home");
                }

                var u = context.tbUsuario.FirstOrDefault(x => x.CorreoElectronico == correoSesion);

                if (u == null)
                {
                    TempData["Mensaje"] = "No se encontró el usuario en la base de datos.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Login", "Home");
                }

                // Mapear a tu modelo de la vista
                var model = new Usuario
                {
                    Identificacion = u.Identificacion,
                    Nombre = u.Nombre,
                    CorreoElectronico = u.CorreoElectronico,
                    Contrasenna = "" // nunca se manda la contraseña real
                };

                return View(model);
            }
        }

        // POST: Usuario/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPerfil(Usuario usuario)
        {
            if (usuario == null)
            {
                TempData["Mensaje"] = "Datos inválidos.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("EditarPerfil");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var correoSesion = Session["UsuarioCorreo"]?.ToString();

                if (string.IsNullOrWhiteSpace(correoSesion))
                {
                    TempData["Mensaje"] = "No hay sesión activa. Inicia sesión nuevamente.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Login", "Home");
                }

                var u = context.tbUsuario.FirstOrDefault(x => x.CorreoElectronico == correoSesion);

                if (u == null)
                {
                    TempData["Mensaje"] = "No se encontró el usuario.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("EditarPerfil");
                }

                // ✅ Actualizar datos
                u.Identificacion = usuario.Identificacion;
                u.Nombre = usuario.Nombre;
                u.CorreoElectronico = usuario.CorreoElectronico;

                // Si el correo cambió, actualiza la sesión también
                if (!string.Equals(correoSesion, usuario.CorreoElectronico, StringComparison.OrdinalIgnoreCase))
                {
                    Session["UsuarioCorreo"] = usuario.CorreoElectronico;
                }

                // ✅ Contraseña: solo si el usuario escribió una nueva
                if (!string.IsNullOrWhiteSpace(usuario.Contrasenna))
                {
                    // Si en tu sistema guardas hash, aquí deberías hashearla.
                    u.Contrasenna = usuario.Contrasenna;
                }

                var filas = context.SaveChanges();

                if (filas > 0)
                {
                    TempData["Mensaje"] = "Cambios guardados correctamente.";
                    TempData["TipoMensaje"] = "success";
                }
                else
                {
                    TempData["Mensaje"] = "No se detectaron cambios para guardar.";
                    TempData["TipoMensaje"] = "warning";
                }
            }

            return RedirectToAction("EditarPerfil");
        }
    }
}