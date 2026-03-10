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
    public class ProveedorController : Controller
    {
        // Consultar todos los proveedores
        public ActionResult VerProveedores()
        {
            var resultado = ConsultarProveedores();
            return View(resultado);
        }

        // Agregar nuevo proveedor (GET)
        [HttpGet]
        public ActionResult AgregarProveedor()
        {
            return View(new Proveedor { Estado = true, FechaRegistro = DateTime.Now });
        }

        // Agregar nuevo proveedor (POST)
        [HttpPost]
        public ActionResult AgregarProveedor(Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "El modelo NO es válido";
                return View(proveedor);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    // Validar correo duplicado
                    bool correoExiste = context.tbProveedores
                        .Any(p => p.CorreoElectronico == proveedor.CorreoElectronico);

                    if (correoExiste)
                    {
                        ViewBag.Mensaje = "El correo ingresado ya está registrado.";
                        return View(proveedor);
                    }

                    var nuevoProveedor = new tbProveedores
                    {
                        NombreEmpresa = proveedor.NombreEmpresa,
                        Contacto = proveedor.Contacto,
                        CorreoElectronico = proveedor.CorreoElectronico,
                        Telefono = proveedor.Telefono,
                        Direccion = proveedor.Direccion,
                        Tipo = proveedor.Tipo,
                        Estado = proveedor.Estado,
                        FechaRegistro = DateTime.Now
                    };

                    context.tbProveedores.Add(nuevoProveedor);
                    var resultadoInsercion = context.SaveChanges();

                    if (resultadoInsercion > 0)
                    {
                        return RedirectToAction("VerProveedores", "Proveedor");
                    }
                }

                ViewBag.Mensaje = "No se pudo registrar el proveedor.";
                return View(proveedor);
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(proveedor);
            }
        }

        // Actualizar proveedor (GET)
        [HttpGet]
        public ActionResult ActualizarProveedor(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("VerProveedores");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var proveedor = context.tbProveedores.FirstOrDefault(p => p.ProveedorID == id.Value);

                if (proveedor == null)
                {
                    TempData["Mensaje"] = "Proveedor no encontrado.";
                    return RedirectToAction("VerProveedores");
                }

                var modelo = new Proveedor
                {
                    ProveedorID = proveedor.ProveedorID,
                    NombreEmpresa = proveedor.NombreEmpresa,
                    Contacto = proveedor.Contacto,
                    CorreoElectronico = proveedor.CorreoElectronico,
                    Telefono = proveedor.Telefono,
                    Direccion = proveedor.Direccion,
                    Tipo = proveedor.Tipo,
                    Estado = proveedor.Estado,
                    FechaRegistro = proveedor.FechaRegistro
                };

                ModelState.Clear();
                return View(modelo);
            }
        }

        // Actualizar proveedor (POST)
        [HttpPost]
        public ActionResult ActualizarProveedor(Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return View(proveedor);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var proveedorExistente = context.tbProveedores
                        .FirstOrDefault(p => p.ProveedorID == proveedor.ProveedorID);

                    if (proveedorExistente == null)
                    {
                        ViewBag.Mensaje = "Proveedor no encontrado.";
                        return View(proveedor);
                    }

                    // Validar correo duplicado
                    bool correoDuplicado = context.tbProveedores
                        .Any(p => p.CorreoElectronico == proveedor.CorreoElectronico
                                  && p.ProveedorID != proveedor.ProveedorID);

                    if (correoDuplicado)
                    {
                        ViewBag.Mensaje = "El correo ya está asignado a otro proveedor.";
                        return View(proveedor);
                    }

                    proveedorExistente.NombreEmpresa = proveedor.NombreEmpresa;
                    proveedorExistente.Contacto = proveedor.Contacto;
                    proveedorExistente.CorreoElectronico = proveedor.CorreoElectronico;
                    proveedorExistente.Telefono = proveedor.Telefono;
                    proveedorExistente.Direccion = proveedor.Direccion;
                    proveedorExistente.Tipo = proveedor.Tipo;
                    proveedorExistente.Estado = proveedor.Estado;

                    int resultado = context.SaveChanges();

                    if (resultado > 0)
                    {
                        TempData["Mensaje"] = "Proveedor actualizado correctamente.";
                        return RedirectToAction("VerProveedores");
                    }

                    ViewBag.Mensaje = "No se realizaron cambios.";
                    return View(proveedor);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(proveedor);
            }
        }

        // Cambiar estado (activar/inactivar)
        public ActionResult CambiarEstadoProveedor(int id)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var proveedor = context.tbProveedores.FirstOrDefault(p => p.ProveedorID == id);

                if (proveedor != null)
                {
                    proveedor.Estado = !proveedor.Estado;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerProveedores");
        }

        // Consultar proveedores
        private List<Proveedor> ConsultarProveedores()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbProveedores
                    .Select(p => new Proveedor
                    {
                        ProveedorID = p.ProveedorID,
                        NombreEmpresa = p.NombreEmpresa,
                        Contacto = p.Contacto,
                        CorreoElectronico = p.CorreoElectronico,
                        Telefono = p.Telefono,
                        Direccion = p.Direccion,
                        Tipo = p.Tipo,
                        Estado = p.Estado,
                        FechaRegistro = p.FechaRegistro
                    })
                    .ToList();

                return resultado;
            }
        }
    }
}