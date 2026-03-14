using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Filtros;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class CotizacionController : Controller
    {
        // ---------------------------------------------------------
        // VER COTIZACIONES
        // ---------------------------------------------------------
        public ActionResult VerCotizaciones()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var lista = (from c in context.tbCotizaciones
                             join cli in context.tbClientes
                             on c.IdCliente equals cli.IdCliente into clienteJoin
                             from cliente in clienteJoin.DefaultIfEmpty()
                             select new CotizacionLista
                             {
                                 CotizacionID = c.CotizacionID,
                                 IdCliente = c.IdCliente,
                                 NombreCliente = cliente != null ? cliente.Nombre + " " + cliente.Apellidos : "",
                                 FechaCotizacion = c.FechaCotizacion,
                                 Total = c.Total,
                                 Estado = c.Estado,
                                 Observaciones = c.Observaciones
                             }).ToList();

                return View(lista);
            }
        }

        // ---------------------------------------------------------
        // GET: CREAR COTIZACION
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult CrearCotizacion()
        {
            CargarClientes();
            return View(new Cotizacion());
        }

        // ---------------------------------------------------------
        // POST: CREAR COTIZACION
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearCotizacion(Cotizacion cotizacion)
        {
            try
            {
                if (!cotizacion.IdCliente.HasValue || cotizacion.IdCliente.Value <= 0)
                {
                    ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente.");
                }

                if (!ModelState.IsValid)
                {
                    CargarClientes(cotizacion.IdCliente);
                    return View(cotizacion);
                }

                using (var context = new DATABASE_PYEEntities())
                {
                    var nuevaCotizacion = new tbCotizaciones
                    {
                        IdCliente = cotizacion.IdCliente,
                        FechaCotizacion = DateTime.Now,
                        Total = 0,
                        Estado = string.IsNullOrEmpty(cotizacion.Estado) ? "Pendiente" : cotizacion.Estado,
                        Observaciones = cotizacion.Observaciones
                    };

                    context.tbCotizaciones.Add(nuevaCotizacion);
                    context.SaveChanges();

                    TempData["Mensaje"] = "Cotización creada correctamente.";
                    return RedirectToAction("AgregarDetalleCotizacion", new { q = nuevaCotizacion.CotizacionID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarClientes(cotizacion.IdCliente);
                return View(cotizacion);
            }
        }

        // ---------------------------------------------------------
        // GET: AGREGAR DETALLE A COTIZACION
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarDetalleCotizacion(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerCotizaciones");

            using (var context = new DATABASE_PYEEntities())
            {
                var entidadCotizacion = context.tbCotizaciones
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CotizacionID == q.Value);

                if (entidadCotizacion == null)
                {
                    TempData["Mensaje"] = "Cotización no encontrada.";
                    return RedirectToAction("VerCotizaciones");
                }

                var cotizacion = new Cotizacion
                {
                    CotizacionID = entidadCotizacion.CotizacionID,
                    IdCliente = entidadCotizacion.IdCliente,
                    FechaCotizacion = entidadCotizacion.FechaCotizacion,
                    Total = entidadCotizacion.Total,
                    Estado = entidadCotizacion.Estado,
                    Observaciones = entidadCotizacion.Observaciones
                };

                ViewBag.Cotizacion = cotizacion;
                ViewBag.ClienteNombre = context.tbClientes
                    .Where(c => c.IdCliente == entidadCotizacion.IdCliente)
                    .Select(c => c.Nombre + " " + c.Apellidos)
                    .FirstOrDefault();

                CargarProductosDisponibles();
                CargarDetalleCotizacion(q.Value);

                var nuevoDetalle = new DetalleCotizacion
                {
                    CotizacionID = q.Value
                };

                return View(nuevoDetalle);
            }
        }

        // ---------------------------------------------------------
        // POST: AGREGAR DETALLE A COTIZACION
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarDetalleCotizacion(DetalleCotizacion detalle)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var cotizacion = context.tbCotizaciones
                        .FirstOrDefault(c => c.CotizacionID == detalle.CotizacionID);

                    if (cotizacion == null)
                    {
                        TempData["Mensaje"] = "Cotización no encontrada.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    if (detalle.ProductoID <= 0)
                    {
                        ModelState.AddModelError("ProductoID", "Debe seleccionar un producto.");
                    }

                    if (detalle.Cantidad <= 0)
                    {
                        ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a 0.");
                    }

                    var producto = context.tbProductos
                        .FirstOrDefault(p => p.ProductoID == detalle.ProductoID);

                    if (producto == null)
                    {
                        ModelState.AddModelError("ProductoID", "Producto no encontrado.");
                    }
                    else
                    {
                        if (producto.Stock <= 0)
                        {
                            ModelState.AddModelError("ProductoID", "El producto no tiene stock disponible.");
                        }

                        if (detalle.Cantidad > producto.Stock)
                        {
                            ModelState.AddModelError("Cantidad", "La cantidad solicitada supera el stock disponible.");
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        var cotizacionModel = new Cotizacion
                        {
                            CotizacionID = cotizacion.CotizacionID,
                            IdCliente = cotizacion.IdCliente,
                            FechaCotizacion = cotizacion.FechaCotizacion,
                            Total = cotizacion.Total,
                            Estado = cotizacion.Estado,
                            Observaciones = cotizacion.Observaciones
                        };

                        ViewBag.Cotizacion = cotizacionModel;
                        ViewBag.ClienteNombre = context.tbClientes
                            .Where(c => c.IdCliente == cotizacion.IdCliente)
                            .Select(c => c.Nombre + " " + c.Apellidos)
                            .FirstOrDefault();

                        CargarProductosDisponibles(detalle.ProductoID);
                        CargarDetalleCotizacion(detalle.CotizacionID);

                        return View(detalle);
                    }

                    var detalleExistente = context.tbDetalleCotizacion
                        .FirstOrDefault(d => d.CotizacionID == detalle.CotizacionID
                                          && d.ProductoID == detalle.ProductoID);

                    if (detalleExistente != null)
                    {
                        var nuevaCantidad = detalleExistente.Cantidad + detalle.Cantidad;

                        if (nuevaCantidad > producto.Stock)
                        {
                            TempData["Mensaje"] = "La suma de cantidades supera el stock disponible.";
                            return RedirectToAction("AgregarDetalleCotizacion", new { q = detalle.CotizacionID });
                        }

                        detalleExistente.Cantidad = nuevaCantidad;
                        detalleExistente.PrecioUnitario = producto.Precio;
                        detalleExistente.Subtotal = detalleExistente.Cantidad * detalleExistente.PrecioUnitario;
                    }
                    else
                    {
                        var nuevoDetalle = new tbDetalleCotizacion
                        {
                            CotizacionID = detalle.CotizacionID,
                            ProductoID = detalle.ProductoID,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = producto.Precio,
                            Subtotal = detalle.Cantidad * producto.Precio
                        };

                        context.tbDetalleCotizacion.Add(nuevoDetalle);
                    }

                    context.SaveChanges();
                    ActualizarTotalCotizacion(detalle.CotizacionID);

                    TempData["Mensaje"] = "Producto agregado a la cotización correctamente.";
                    return RedirectToAction("AgregarDetalleCotizacion", new { q = detalle.CotizacionID });
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error: " + ex.Message;
                return RedirectToAction("AgregarDetalleCotizacion", new { q = detalle.CotizacionID });
            }
        }

        // ---------------------------------------------------------
        // VER DETALLE DE COTIZACION
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult VerDetalleCotizacion(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerCotizaciones");

            using (var context = new DATABASE_PYEEntities())
            {
                var entidadCotizacion = context.tbCotizaciones
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CotizacionID == q.Value);

                if (entidadCotizacion == null)
                {
                    TempData["Mensaje"] = "Cotización no encontrada.";
                    return RedirectToAction("VerCotizaciones");
                }

                var cotizacion = new Cotizacion
                {
                    CotizacionID = entidadCotizacion.CotizacionID,
                    IdCliente = entidadCotizacion.IdCliente,
                    FechaCotizacion = entidadCotizacion.FechaCotizacion,
                    Total = entidadCotizacion.Total,
                    Estado = entidadCotizacion.Estado,
                    Observaciones = entidadCotizacion.Observaciones
                };

                ViewBag.Cotizacion = cotizacion;
                ViewBag.ClienteNombre = context.tbClientes
                    .Where(c => c.IdCliente == entidadCotizacion.IdCliente)
                    .Select(c => c.Nombre + " " + c.Apellidos)
                    .FirstOrDefault();

                CargarDetalleCotizacion(q.Value);

                return View();
            }
        }

        // ---------------------------------------------------------
        // ELIMINAR DETALLE DE COTIZACION
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarDetalleCotizacion(int q, int cotizacionId)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var detalle = context.tbDetalleCotizacion
                        .FirstOrDefault(d => d.DetalleCotizacionID == q);

                    if (detalle != null)
                    {
                        context.tbDetalleCotizacion.Remove(detalle);
                        context.SaveChanges();

                        ActualizarTotalCotizacion(cotizacionId);

                        TempData["Mensaje"] = "Producto eliminado de la cotización correctamente.";
                    }
                    else
                    {
                        TempData["Mensaje"] = "Detalle no encontrado.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction("AgregarDetalleCotizacion", new { q = cotizacionId });
        }

        // ---------------------------------------------------------
        // CAMBIAR ESTADO DE COTIZACION
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstadoCotizacion(int q, string estado)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var cotizacion = context.tbCotizaciones
                        .FirstOrDefault(c => c.CotizacionID == q);

                    if (cotizacion != null)
                    {
                        cotizacion.Estado = estado;
                        context.SaveChanges();

                        TempData["Mensaje"] = "Estado de cotización actualizado correctamente.";
                    }
                    else
                    {
                        TempData["Mensaje"] = "Cotización no encontrada.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error: " + ex.Message;
            }

            return RedirectToAction("VerCotizaciones");
        }

        // ---------------------------------------------------------
        // ACTUALIZAR TOTAL DE COTIZACION
        // ---------------------------------------------------------
        private void ActualizarTotalCotizacion(int cotizacionId)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var cotizacion = context.tbCotizaciones
                    .FirstOrDefault(c => c.CotizacionID == cotizacionId);

                if (cotizacion != null)
                {
                    var total = context.tbDetalleCotizacion
                        .Where(d => d.CotizacionID == cotizacionId)
                        .Select(d => (decimal?)d.Subtotal)
                        .Sum() ?? 0;

                    cotizacion.Total = total;
                    context.SaveChanges();
                }
            }
        }

        // ---------------------------------------------------------
        // CARGAR CLIENTES
        // ---------------------------------------------------------
        private void CargarClientes(int? clienteId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Clientes = new SelectList(
                    context.tbClientes
                        .AsNoTracking()
                        .Select(c => new
                        {
                            c.IdCliente,
                            NombreCompleto = c.Nombre + " " + c.Apellidos
                        })
                        .ToList(),
                    "IdCliente",
                    "NombreCompleto",
                    clienteId
                );
            }
        }

        // ---------------------------------------------------------
        // CARGAR PRODUCTOS DISPONIBLES
        // ---------------------------------------------------------
        private void CargarProductosDisponibles(int? productoId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Productos = new SelectList(
                    context.tbProductos
                        .AsNoTracking()
                        .Where(p => p.Stock > 0)
                        .Select(p => new
                        {
                            p.ProductoID,
                            NombreProducto = p.Nombre + " - ₡ " + p.Precio
                        })
                        .ToList(),
                    "ProductoID",
                    "NombreProducto",
                    productoId
                );
            }
        }

        // ---------------------------------------------------------
        // CARGAR DETALLE DE COTIZACION
        // ---------------------------------------------------------
        private void CargarDetalleCotizacion(int cotizacionId)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var detalle = (from d in context.tbDetalleCotizacion
                               join p in context.tbProductos
                               on d.ProductoID equals p.ProductoID
                               where d.CotizacionID == cotizacionId
                               select new DetalleCotizacionViewModel
                               {
                                   DetalleCotizacionID = d.DetalleCotizacionID,
                                   CotizacionID = d.CotizacionID,
                                   ProductoID = d.ProductoID,
                                   NombreProducto = p.Nombre,
                                   Cantidad = d.Cantidad,
                                   PrecioUnitario = d.PrecioUnitario,
                                   Subtotal = d.Subtotal
                               }).ToList();

                ViewBag.DetalleCotizacion = detalle;
            }
        }
    }
}