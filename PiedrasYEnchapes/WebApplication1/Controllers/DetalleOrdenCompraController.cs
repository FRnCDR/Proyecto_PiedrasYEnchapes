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
using WebApplication1.Filtros;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class DetalleOrdenCompraController : Controller
    {
        // ---------------------------------------------------------
        // VER DETALLE DE UNA ORDEN
        // ---------------------------------------------------------
        public ActionResult VerDetalleOrdenCompra(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");

            using (var context = new DATABASE_PYEEntities())
            {
                var orden = (from o in context.tbOrdenesCompra.AsNoTracking()
                             join p in context.tbProveedores.AsNoTracking()
                                on o.ProveedorID equals p.ProveedorID
                             where o.OrdenCompraID == q.Value
                             select new OrdenCompra
                             {
                                 OrdenCompraID = o.OrdenCompraID,
                                 ProveedorID = o.ProveedorID,
                                 NombreProveedor = p.NombreEmpresa,
                                 FechaOrden = o.FechaOrden,
                                 Total = o.Total,
                                 Estado = o.Estado
                             }).FirstOrDefault();

                if (orden == null)
                {
                    TempData["Mensaje"] = "La orden de compra no fue encontrada.";
                    return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                }

                ViewBag.OrdenCompra = orden;
                ViewBag.OrdenCompraID = orden.OrdenCompraID;

                var detalle = ConsultarDetallesOrdenCompra(q.Value);

                return View(detalle);
            }
        }

        // ---------------------------------------------------------
        // GET: AGREGAR DETALLE DE ORDEN
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarDetalleOrdenCompra(int? ordenId)
        {
            if (!ordenId.HasValue)
                return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");

            using (var context = new DATABASE_PYEEntities())
            {
                var ordenExiste = context.tbOrdenesCompra
                    .AsNoTracking()
                    .Any(o => o.OrdenCompraID == ordenId.Value);

                if (!ordenExiste)
                {
                    TempData["Mensaje"] = "La orden de compra no fue encontrada.";
                    return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                }
            }

            CargarCombos();

            var model = new DetalleOrdenCompra
            {
                OrdenCompraID = ordenId.Value,
                Cantidad = 1,
                PrecioCompra = 0,
                Subtotal = 0
            };

            return View(model);
        }

        // ---------------------------------------------------------
        // POST: AGREGAR DETALLE DE ORDEN
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarDetalleOrdenCompra(DetalleOrdenCompra detalleOrdenCompra)
        {
            if (detalleOrdenCompra.ProductoID <= 0)
            {
                ModelState.AddModelError("ProductoID", "Debe seleccionar un producto.");
            }

            if (detalleOrdenCompra.Cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor que cero.");
            }

            if (detalleOrdenCompra.PrecioCompra <= 0)
            {
                ModelState.AddModelError("PrecioCompra", "El precio de compra debe ser mayor que cero.");
            }

            detalleOrdenCompra.Subtotal = detalleOrdenCompra.Cantidad * detalleOrdenCompra.PrecioCompra;

            if (!ModelState.IsValid)
            {
                CargarCombos();
                return View(detalleOrdenCompra);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var ordenExistente = context.tbOrdenesCompra
                        .FirstOrDefault(o => o.OrdenCompraID == detalleOrdenCompra.OrdenCompraID);

                    if (ordenExistente == null)
                    {
                        TempData["Mensaje"] = "La orden de compra no fue encontrada.";
                        return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                    }

                    var nuevoDetalle = new tbDetalleOrdenCompra
                    {
                        OrdenCompraID = detalleOrdenCompra.OrdenCompraID,
                        ProductoID = detalleOrdenCompra.ProductoID,
                        Cantidad = detalleOrdenCompra.Cantidad,
                        PrecioCompra = detalleOrdenCompra.PrecioCompra,
                        Subtotal = detalleOrdenCompra.Subtotal
                    };

                    context.tbDetalleOrdenCompra.Add(nuevoDetalle);
                    context.SaveChanges();

                    // Actualizar total de la orden
                    ordenExistente.Total = context.tbDetalleOrdenCompra
                        .Where(d => d.OrdenCompraID == detalleOrdenCompra.OrdenCompraID)
                        .Sum(d => (decimal?)d.Subtotal) ?? 0;

                    context.SaveChanges();

                    TempData["Mensaje"] = "Detalle agregado correctamente.";
                    return RedirectToAction("VerDetalleOrdenCompra", "DetalleOrdenCompra", new { q = detalleOrdenCompra.OrdenCompraID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarCombos();
                return View(detalleOrdenCompra);
            }
        }

        // ---------------------------------------------------------
        // ELIMINAR DETALLE DE ORDEN
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarDetalleOrdenCompra(int q)
        {
            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var detalle = context.tbDetalleOrdenCompra
                        .FirstOrDefault(d => d.DetalleOrdenCompraID == q);

                    if (detalle != null)
                    {
                        int ordenId = detalle.OrdenCompraID;

                        context.tbDetalleOrdenCompra.Remove(detalle);
                        context.SaveChanges();

                        var orden = context.tbOrdenesCompra
                            .FirstOrDefault(o => o.OrdenCompraID == ordenId);

                        if (orden != null)
                        {
                            orden.Total = context.tbDetalleOrdenCompra
                                .Where(d => d.OrdenCompraID == ordenId)
                                .Sum(d => (decimal?)d.Subtotal) ?? 0;

                            context.SaveChanges();
                        }

                        TempData["Mensaje"] = "Detalle eliminado correctamente.";
                        return RedirectToAction("VerDetalleOrdenCompra", "DetalleOrdenCompra", new { q = ordenId });
                    }
                }

                TempData["Mensaje"] = "Detalle no encontrado.";
                return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = "Error al eliminar el detalle: " + ex.Message;
                return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
            }
        }

        // ---------------------------------------------------------
        // CONSULTAR DETALLES DE UNA ORDEN
        // ---------------------------------------------------------
        private List<DetalleOrdenCompra> ConsultarDetallesOrdenCompra(int ordenCompraId)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = (from d in context.tbDetalleOrdenCompra.AsNoTracking()
                                 join p in context.tbProductos.AsNoTracking()
                                    on d.ProductoID equals p.ProductoID
                                 where d.OrdenCompraID == ordenCompraId
                                 select new DetalleOrdenCompra
                                 {
                                     DetalleOrdenCompraID = d.DetalleOrdenCompraID,
                                     OrdenCompraID = d.OrdenCompraID,
                                     ProductoID = d.ProductoID,
                                     NombreProducto = p.Nombre,
                                     Cantidad = d.Cantidad,
                                     PrecioCompra = d.PrecioCompra,
                                     Subtotal = d.Subtotal
                                 })
                                 .ToList();

                return resultado;
            }
        }

        // ---------------------------------------------------------
        // CARGAR COMBOS
        // ---------------------------------------------------------
        private void CargarCombos(int? productoId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Productos = new SelectList(
                    context.tbProductos
                        .AsNoTracking()
                        .Where(p => p.Estado == true)
                        .ToList(),
                    "ProductoID",
                    "Nombre",
                    productoId
                );
            }
        }
    }
}