using WebApplication1.EF;
using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class EmpleadoController : Controller
    {
        // Consultar todos los empleados
        public ActionResult VerEmpleados()
        {
            var resultado = ConsultarEmpleados();
            return View(resultado);
        }

        // Agregar nuevo empleado (GET)
        [HttpGet]
        public ActionResult AgregarEmpleado()
        {
            return View(new Empleado());
        }

        // Agregar nuevo empleado (POST)
        [HttpPost]
        public ActionResult AgregarEmpleado(Empleado empleado)
        {
            if (!ModelState.IsValid)
            {
                return View(empleado);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    bool correoExiste = context.tbEmpleados
                        .Any(e => e.Correo == empleado.Correo);

                    if (correoExiste)
                    {
                        ViewBag.Mensaje = "El correo ingresado ya está registrado.";
                        return View(empleado);
                    }

                    var nuevoEmpleado = new tbEmpleados
                    {
                        Nombre = empleado.Nombre,
                        Apellidos = empleado.Apellidos,
                        Correo = empleado.Correo,
                        Telefono = empleado.Telefono,
                        Estado = true,
                        FechaRegistro = DateTime.Now
                    };

                    context.tbEmpleados.Add(nuevoEmpleado);
                    var resultadoInsercion = context.SaveChanges();

                    if (resultadoInsercion > 0)
                    {
                        return RedirectToAction("VerEmpleados", "Empleado");
                    }
                }

                ViewBag.Mensaje = "No se pudo registrar el empleado.";
                return View(empleado);
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(empleado);
            }
        }

        // Actualizar un empleado (GET)
        [HttpGet]
        public ActionResult ActualizarEmpleado(int? q)
        {
            if (!q.HasValue)
            {
                return RedirectToAction("VerEmpleados", "Empleado");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbEmpleados
                    .Where(e => e.IdEmpleado == q)
                    .ToList();

                var datos = resultado.Select(e => new Empleado
                {
                    IdEmpleado = e.IdEmpleado,
                    Nombre = e.Nombre,
                    Apellidos = e.Apellidos,
                    Correo = e.Correo,
                    Telefono = e.Telefono
                }).FirstOrDefault();

                if (datos == null)
                {
                    TempData["Mensaje"] = "Empleado no encontrado.";
                    return RedirectToAction("VerEmpleados", "Empleado");
                }

                return View(datos);
            }
        }

        // Actualizar un empleado (POST)
        [HttpPost]
        public ActionResult ActualizarEmpleado(Empleado empleado)
        {
            if (!ModelState.IsValid)
            {
                return View(empleado);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var empleadoExistente = context.tbEmpleados
                        .FirstOrDefault(e => e.IdEmpleado == empleado.IdEmpleado);

                    if (empleadoExistente == null)
                    {
                        ViewBag.Mensaje = "Empleado no encontrado.";
                        return View(empleado);
                    }

                    bool correoDuplicado = context.tbEmpleados
                        .Any(e => e.Correo == empleado.Correo &&
                                  e.IdEmpleado != empleado.IdEmpleado);

                    if (correoDuplicado)
                    {
                        ViewBag.Mensaje = "El correo ya está asignado a otro empleado.";
                        return View(empleado);
                    }

                    empleadoExistente.Nombre = empleado.Nombre;
                    empleadoExistente.Apellidos = empleado.Apellidos;
                    empleadoExistente.Correo = empleado.Correo;
                    empleadoExistente.Telefono = empleado.Telefono;

                    var resultadoActualizacion = context.SaveChanges();

                    if (resultadoActualizacion > 0)
                    {
                        return RedirectToAction("VerEmpleados", "Empleado");
                    }

                    ViewBag.Mensaje = "No se pudo actualizar la información.";
                    return View(empleado);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(empleado);
            }
        }

        // Cambiar estado (activar/inactivar)
        public ActionResult CambiarEstadoEmpleado(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var empleado = context.tbEmpleados
                    .FirstOrDefault(e => e.IdEmpleado == q);

                if (empleado != null)
                {
                    empleado.Estado = !empleado.Estado; // Cambiar el estado
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerEmpleados", "Empleado");
        }

        // Consultar empleados
        private List<Empleado> ConsultarEmpleados()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbEmpleados
                    .ToList()
                    .Select(e => new Empleado
                    {
                        IdEmpleado = e.IdEmpleado,
                        Nombre = e.Nombre,
                        Apellidos = e.Apellidos,
                        Correo = e.Correo,
                        Telefono = e.Telefono,
                        Estado = e.Estado
                    })
                    .ToList();

                return resultado;
            }
        }
    }
}
