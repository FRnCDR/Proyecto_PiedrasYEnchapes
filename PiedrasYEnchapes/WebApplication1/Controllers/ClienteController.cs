using WebApplication1.EF;
using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Filtros;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class ClienteController : Controller
    {
        // Consultar todos los clientes
        public ActionResult VerClientes()
        {
            var resultado = ConsultarClientes();
            return View(resultado);
        }

        // Agregar nuevo cliente (GET)
        [HttpGet]
        public ActionResult AgregarCliente()
        {
            return View(new Cliente());
        }

        [HttpPost]
        public ActionResult AgregarCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "El modelo NO es válido";
                return View(cliente);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    // Validar correo duplicado
                    bool correoExiste = context.tbClientes
                        .Any(c => c.CorreoElectronico == cliente.CorreoElectronico);

                    if (correoExiste)
                    {
                        ViewBag.Mensaje = "El correo ingresado ya está registrado.";
                        return View(cliente);
                    }

                    // Validar identificación duplicada
                    bool identificacionExiste = context.tbClientes
                        .Any(c => c.Identificacion == cliente.Identificacion);

                    if (identificacionExiste)
                    {
                        ViewBag.Mensaje = "La identificación ya está registrada.";
                        return View(cliente);
                    }

                    var nuevoCliente = new tbClientes
                    {
                        Identificacion = cliente.Identificacion,
                        Nombre = cliente.Nombre,
                        Apellidos = cliente.Apellidos,
                        CorreoElectronico = cliente.CorreoElectronico,
                        Telefono = cliente.Telefono,
                        Direccion = cliente.Direccion,
                        Estado = cliente.Estado, 
                        FechaRegistro = DateTime.Now
                    };

                    context.tbClientes.Add(nuevoCliente);
                    var resultadoInsercion = context.SaveChanges();

                    if (resultadoInsercion > 0)
                    {
                        return RedirectToAction("VerClientes", "Cliente");
                    }
                }

                ViewBag.Mensaje = "No se pudo registrar el cliente.";
                return View(cliente);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        ViewBag.Mensaje += $"Propiedad: {ve.PropertyName} - Error: {ve.ErrorMessage}<br/>";
                    }
                }

                return View(cliente);
            }
        }

        [HttpGet]
        public ActionResult ActualizarCliente(int? q)
        {
            if (!q.HasValue)
            {
                return RedirectToAction("VerClientes", "Cliente");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var cliente = context.tbClientes
                    .FirstOrDefault(c => c.IdCliente == q.Value);

                if (cliente == null)
                {
                    TempData["Mensaje"] = "Cliente no encontrado.";
                    return RedirectToAction("VerClientes", "Cliente");
                }

                var datos = new Cliente
                {
                    IdCliente = cliente.IdCliente,
                    Identificacion = cliente.Identificacion,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    CorreoElectronico = cliente.CorreoElectronico,
                    Telefono = cliente.Telefono,
                    Direccion =  cliente.Direccion,
                    FechaRegistro = cliente.FechaRegistro,
                    Estado = cliente.Estado
                };

                ModelState.Clear();

                return View(datos);
            }
        }

        // Actualizar cliente (POST)
        [HttpPost]
        public ActionResult ActualizarCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var clienteExistente = context.tbClientes
                        .FirstOrDefault(c => c.IdCliente == cliente.IdCliente);

                    if (clienteExistente == null)
                    {
                        ViewBag.Mensaje = "Cliente no encontrado.";
                        return View(cliente);
                    }

                    // Validar correo duplicado
                    bool correoDuplicado = context.tbClientes
                        .Any(c => c.CorreoElectronico == cliente.CorreoElectronico
                                  && c.IdCliente != cliente.IdCliente);

                    if (correoDuplicado)
                    {
                        ViewBag.Mensaje = "El correo ya está asignado a otro cliente.";
                        return View(cliente);
                    }


                    clienteExistente.Nombre = cliente.Nombre;
                    clienteExistente.Apellidos = cliente.Apellidos;
                    clienteExistente.CorreoElectronico = cliente.CorreoElectronico;
                    clienteExistente.Telefono = cliente.Telefono;
                    clienteExistente.Direccion = cliente.Direccion;
                    clienteExistente.Estado = cliente.Estado;

                    int resultado = context.SaveChanges();

                    if (resultado > 0)
                    {
                        TempData["Mensaje"] = "Cliente actualizado correctamente.";
                        return RedirectToAction("VerClientes", "Cliente");
                    }

                    ViewBag.Mensaje = "No se realizaron cambios.";
                    return View(cliente);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(cliente);
            }
        }

        // Cambiar estado (activar/inactivar)
        public ActionResult CambiarEstadoCliente(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var cliente = context.tbClientes
                    .FirstOrDefault(c => c.IdCliente == q);

                if (cliente != null)
                {
                    cliente.Estado = !cliente.Estado;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerClientes", "Cliente");
        }

        // Consultar clientes
        private List<Cliente> ConsultarClientes()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbClientes
                    .Select(c => new Cliente
                    {
                        IdCliente = c.IdCliente,
                        Identificacion = c.Identificacion,
                        Nombre = c.Nombre,
                        Apellidos = c.Apellidos,
                        CorreoElectronico = c.CorreoElectronico,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        FechaRegistro = c.FechaRegistro,
                        Estado = c.Estado
                    })
                    .ToList();

                return resultado;
            }
        }
    }
}