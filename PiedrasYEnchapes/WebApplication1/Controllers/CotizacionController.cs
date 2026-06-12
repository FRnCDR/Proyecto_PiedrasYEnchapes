using System;
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
        // ── helpers de sesión ──────────────────────────────────────
        private int GetIdUsuario() => (int)Session["IdUsuario"];
        private bool EsAdmin()
        {
            var perfil = Session["IdPerfil"];
            return perfil != null && perfil.ToString() == "1";
        }

        // ---------------------------------------------------------
        // VER COTIZACIONES
        // ---------------------------------------------------------
        public ActionResult VerCotizaciones()
        {
            int idUsuario = GetIdUsuario();

            using (var context = new DATABASE_PYEEntities())
            {
                IQueryable<tbCotizaciones> query = context.tbCotizaciones;

                // Si no es admin, solo ve las suyas
                if (!EsAdmin())
                    query = query.Where(c => c.IdUsuario == idUsuario);

                var lista = query
                    .OrderByDescending(c => c.CotizacionID)
                    .Select(c => new CotizacionLista
                    {
                        CotizacionID = c.CotizacionID,
                        IdCliente = c.IdCliente,
                        NombreCliente = c.tbUsuario != null ? c.tbUsuario.Nombre : "—",
                        FechaCotizacion = c.FechaCotizacion,
                        Total = c.Total,
                        Estado = c.Estado,
                        Observaciones = c.Observaciones
                    })
                    .ToList();

                return View(lista);
            }
        }

        // ---------------------------------------------------------
        // GET: CREAR COTIZACION
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult CrearCotizacion()
        {
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
                // IdCliente ya no es requerido en este flujo
                ModelState.Remove("IdCliente");

                if (!ModelState.IsValid)
                    return View(cotizacion);

                using (var context = new DATABASE_PYEEntities())
                {
                    var nueva = new tbCotizaciones
                    {
                        IdUsuario = GetIdUsuario(),
                        IdCliente = null,
                        FechaCotizacion = DateTime.Now,
                        Total = 0,
                        Estado = "Pendiente",
                        Observaciones = cotizacion.Observaciones
                    };

                    context.tbCotizaciones.Add(nueva);
                    context.SaveChanges();

                    TempData["Mensaje"] = "Cotización creada. Ahora agrega los productos.";
                    return RedirectToAction("AgregarDetalleCotizacion", new { q = nueva.CotizacionID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(cotizacion);
            }
        }

        // ---------------------------------------------------------
        // GET: AGREGAR DETALLE
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarDetalleCotizacion(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerCotizaciones");

            using (var context = new DATABASE_PYEEntities())
            {
                var entidad = context.tbCotizaciones
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CotizacionID == q.Value);

                if (entidad == null)
                {
                    TempData["Mensaje"] = "Cotización no encontrada.";
                    return RedirectToAction("VerCotizaciones");
                }

                if (!EsAdmin() && entidad.IdUsuario != GetIdUsuario())
                {
                    TempData["Mensaje"] = "No tienes permiso para editar esta cotización.";
                    return RedirectToAction("VerCotizaciones");
                }

                ViewBag.Cotizacion = new Cotizacion
                {
                    CotizacionID = entidad.CotizacionID,
                    FechaCotizacion = entidad.FechaCotizacion,
                    Total = entidad.Total,
                    Estado = entidad.Estado,
                    Observaciones = entidad.Observaciones
                };
                ViewBag.NombreUsuario = Session["Nombre"]?.ToString();

                CargarProductosDisponibles();
                CargarDetalleCotizacion(q.Value);

                return View(new DetalleCotizacion { CotizacionID = q.Value });
            }
        }

        // ---------------------------------------------------------
        // POST: AGREGAR DETALLE
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

                    if (!EsAdmin() && cotizacion.IdUsuario != GetIdUsuario())
                    {
                        TempData["Mensaje"] = "No tienes permiso para modificar esta cotización.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    if (detalle.ProductoID <= 0)
                        ModelState.AddModelError("ProductoID", "Debe seleccionar un producto.");

                    if (detalle.Cantidad <= 0)
                        ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a 0.");

                    var producto = context.tbProductos
                        .FirstOrDefault(p => p.ProductoID == detalle.ProductoID && p.Estado == true);

                    if (producto == null)
                    {
                        ModelState.AddModelError("ProductoID", "Producto no encontrado o inactivo.");
                    }
                    else if (detalle.Cantidad > producto.Stock)
                    {
                        ModelState.AddModelError("Cantidad",
                            $"La cantidad supera el stock disponible ({producto.Stock}).");
                    }

                    if (!ModelState.IsValid)
                    {
                        ViewBag.Cotizacion = new Cotizacion
                        {
                            CotizacionID = cotizacion.CotizacionID,
                            FechaCotizacion = cotizacion.FechaCotizacion,
                            Total = cotizacion.Total,
                            Estado = cotizacion.Estado,
                            Observaciones = cotizacion.Observaciones
                        };
                        ViewBag.NombreUsuario = Session["Nombre"]?.ToString();
                        CargarProductosDisponibles(detalle.ProductoID);
                        CargarDetalleCotizacion(detalle.CotizacionID);
                        return View(detalle);
                    }

                    var existente = context.tbDetalleCotizacion
                        .FirstOrDefault(d => d.CotizacionID == detalle.CotizacionID
                                          && d.ProductoID == detalle.ProductoID);

                    if (existente != null)
                    {
                        int nuevaCantidad = existente.Cantidad + detalle.Cantidad;
                        if (nuevaCantidad > producto.Stock)
                        {
                            TempData["Mensaje"] =
                                $"La suma de cantidades ({nuevaCantidad}) supera el stock ({producto.Stock}).";
                            return RedirectToAction("AgregarDetalleCotizacion",
                                new { q = detalle.CotizacionID });
                        }
                        existente.Cantidad = nuevaCantidad;
                        existente.PrecioUnitario = producto.Precio;
                        existente.Subtotal = nuevaCantidad * producto.Precio;
                    }
                    else
                    {
                        context.tbDetalleCotizacion.Add(new tbDetalleCotizacion
                        {
                            CotizacionID = detalle.CotizacionID,
                            ProductoID = detalle.ProductoID,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = producto.Precio,
                            Subtotal = detalle.Cantidad * producto.Precio
                        });
                    }

                    context.SaveChanges();
                }

                ActualizarTotalCotizacion(detalle.CotizacionID);
                TempData["Mensaje"] = "Producto agregado correctamente.";
                return RedirectToAction("AgregarDetalleCotizacion", new { q = detalle.CotizacionID });
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
                var entidad = context.tbCotizaciones
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CotizacionID == q.Value);

                if (entidad == null)
                {
                    TempData["Mensaje"] = "Cotización no encontrada.";
                    return RedirectToAction("VerCotizaciones");
                }

                if (!EsAdmin() && entidad.IdUsuario != GetIdUsuario())
                {
                    TempData["Mensaje"] = "No tienes permiso para ver esta cotización.";
                    return RedirectToAction("VerCotizaciones");
                }

                ViewBag.Cotizacion = new Cotizacion
                {
                    CotizacionID = entidad.CotizacionID,
                    FechaCotizacion = entidad.FechaCotizacion,
                    Total = entidad.Total,
                    Estado = entidad.Estado,
                    Observaciones = entidad.Observaciones
                };

                using (var ctx2 = new DATABASE_PYEEntities())
                {
                    ViewBag.NombreUsuario = ctx2.tbUsuario
                        .Where(u => u.IdUsuario == entidad.IdUsuario)
                        .Select(u => u.Nombre)
                        .FirstOrDefault() ?? Session["Nombre"]?.ToString();
                }

                CargarDetalleCotizacion(q.Value);
                return View();
            }
        }

        // ---------------------------------------------------------
        // POST: ELIMINAR COTIZACION COMPLETA
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCotizacion(int q)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var cotizacion = context.tbCotizaciones
                        .FirstOrDefault(c => c.CotizacionID == q);

                    if (cotizacion == null)
                    {
                        TempData["Mensaje"] = "Cotización no encontrada.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    if (!EsAdmin() && cotizacion.IdUsuario != GetIdUsuario())
                    {
                        TempData["Mensaje"] = "No tienes permiso para eliminar esta cotización.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    // Eliminar detalles primero (respeta la FK)
                    var detalles = context.tbDetalleCotizacion
                        .Where(d => d.CotizacionID == q)
                        .ToList();

                    context.tbDetalleCotizacion.RemoveRange(detalles);
                    context.tbCotizaciones.Remove(cotizacion);
                    context.SaveChanges();

                    TempData["Mensaje"] = "Cotización eliminada correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction("VerCotizaciones");
        }

        // ---------------------------------------------------------
        // POST: ELIMINAR DETALLE (una línea)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarDetalleCotizacion(int q, int cotizacionId)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var cotizacion = context.tbCotizaciones
                        .FirstOrDefault(c => c.CotizacionID == cotizacionId);

                    if (cotizacion == null)
                    {
                        TempData["Mensaje"] = "Cotización no encontrada.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    if (!EsAdmin() && cotizacion.IdUsuario != GetIdUsuario())
                    {
                        TempData["Mensaje"] = "No tienes permiso para modificar esta cotización.";
                        return RedirectToAction("VerCotizaciones");
                    }

                    var detalle = context.tbDetalleCotizacion
                        .FirstOrDefault(d => d.DetalleCotizacionID == q);

                    if (detalle != null)
                    {
                        context.tbDetalleCotizacion.Remove(detalle);
                        context.SaveChanges();
                        ActualizarTotalCotizacion(cotizacionId);
                        TempData["Mensaje"] = "Producto eliminado de la cotización.";
                    }
                    else
                    {
                        TempData["Mensaje"] = "Línea no encontrada.";
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
        // POST: CAMBIAR ESTADO (solo admin)
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
                        TempData["Mensaje"] = "Estado actualizado correctamente.";
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

        // ── helpers privados ───────────────────────────────────────

        private void ActualizarTotalCotizacion(int cotizacionId)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var cotizacion = context.tbCotizaciones
                    .FirstOrDefault(c => c.CotizacionID == cotizacionId);

                if (cotizacion != null)
                {
                    cotizacion.Total = context.tbDetalleCotizacion
                        .Where(d => d.CotizacionID == cotizacionId)
                        .Sum(d => (decimal?)d.Subtotal) ?? 0;

                    context.SaveChanges();
                }
            }
        }

        private void CargarProductosDisponibles(int? productoId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Productos = new SelectList(
                    context.tbProductos
                        .AsNoTracking()
                        .Where(p => p.Estado == true && p.Stock > 0)
                        .Select(p => new
                        {
                            p.ProductoID,
                            NombreProducto = p.Nombre + " — ₡ " + p.Precio
                        })
                        .ToList(),
                    "ProductoID",
                    "NombreProducto",
                    productoId
                );
            }
        }

        private void CargarDetalleCotizacion(int cotizacionId)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.DetalleCotizacion = (
                    from d in context.tbDetalleCotizacion
                    join p in context.tbProductos on d.ProductoID equals p.ProductoID
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
                    }
                ).ToList();
            }
        }
    }
}