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
    public class OrdenesCompraController : Controller
    {
        // ---------------------------------------------------------
        // VER ORDENES DE COMPRA
        // ---------------------------------------------------------
        public ActionResult VerOrdenesCompra()
        {
            var resultado = ConsultarOrdenesCompra();

            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Proveedores = new SelectList(
                    context.tbProveedores.AsNoTracking().ToList(),
                    "ProveedorID",
                    "NombreEmpresa"
                );
            }

            return View(resultado);
        }

        // ---------------------------------------------------------
        // GET: AGREGAR ORDEN DE COMPRA
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarOrdenCompra()
        {
            CargarCombos();
            return View(new OrdenCompra
            {
                FechaOrden = DateTime.Now,
                Estado = true
            });
        }

        // ---------------------------------------------------------
        // POST: AGREGAR ORDEN DE COMPRA
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarOrdenCompra(OrdenCompra ordenCompra)
        {
            if (ordenCompra.ProveedorID <= 0)
            {
                ModelState.AddModelError("ProveedorID", "Debe seleccionar un proveedor.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(ordenCompra.ProveedorID);
                return View(ordenCompra);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var nuevaOrden = new tbOrdenesCompra
                    {
                        ProveedorID = ordenCompra.ProveedorID,
                        FechaOrden = ordenCompra.FechaOrden,
                        Total = ordenCompra.Total,
                        Estado = true
                    };

                    context.tbOrdenesCompra.Add(nuevaOrden);
                    context.SaveChanges();

                    TempData["Mensaje"] = "Orden de compra agregada correctamente.";
                    return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarCombos(ordenCompra.ProveedorID);
                return View(ordenCompra);
            }
        }

        // ---------------------------------------------------------
        // GET: ACTUALIZAR ORDEN DE COMPRA
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult ActualizarOrdenCompra(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerOrdenesCompra");

            using (var context = new DATABASE_PYEEntities())
            {
                var orden = context.tbOrdenesCompra
                    .AsNoTracking()
                    .FirstOrDefault(o => o.OrdenCompraID == q.Value);

                if (orden == null)
                {
                    TempData["Mensaje"] = "Orden de compra no encontrada.";
                    return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                }

                var datos = new OrdenCompra
                {
                    OrdenCompraID = orden.OrdenCompraID,
                    ProveedorID = orden.ProveedorID,
                    FechaOrden = orden.FechaOrden,
                    Total = orden.Total,
                    Estado = orden.Estado
                };

                CargarCombos(datos.ProveedorID);
                return View(datos);
            }
        }

        // ---------------------------------------------------------
        // POST: ACTUALIZAR ORDEN DE COMPRA
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarOrdenCompra(OrdenCompra ordenCompra)
        {
            if (ordenCompra.ProveedorID <= 0)
            {
                ModelState.AddModelError("ProveedorID", "Debe seleccionar un proveedor.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(ordenCompra.ProveedorID);
                return View(ordenCompra);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var ordenExistente = context.tbOrdenesCompra
                        .FirstOrDefault(o => o.OrdenCompraID == ordenCompra.OrdenCompraID);

                    if (ordenExistente == null)
                    {
                        ViewBag.Mensaje = "Orden de compra no encontrada.";
                        CargarCombos(ordenCompra.ProveedorID);
                        return View(ordenCompra);
                    }

                    ordenExistente.ProveedorID = ordenCompra.ProveedorID;
                    ordenExistente.FechaOrden = ordenCompra.FechaOrden;
                    ordenExistente.Total = ordenCompra.Total;

                    context.SaveChanges();

                    TempData["Mensaje"] = "Orden de compra actualizada correctamente.";
                    return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarCombos(ordenCompra.ProveedorID);
                return View(ordenCompra);
            }
        }

        // ---------------------------------------------------------
        // CAMBIAR ESTADO ORDEN DE COMPRA
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstadoOrdenCompra(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var orden = context.tbOrdenesCompra
                    .FirstOrDefault(o => o.OrdenCompraID == q);

                if (orden != null)
                {
                    orden.Estado = !orden.Estado;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerOrdenesCompra", "OrdenesCompra");
        }

        // ---------------------------------------------------------
        // CONSULTAR ORDENES DE COMPRA
        // ---------------------------------------------------------
        private List<OrdenCompra> ConsultarOrdenesCompra()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = (from o in context.tbOrdenesCompra.AsNoTracking()
                                 join p in context.tbProveedores.AsNoTracking()
                                    on o.ProveedorID equals p.ProveedorID
                                 select new OrdenCompra
                                 {
                                     OrdenCompraID = o.OrdenCompraID,
                                     ProveedorID = o.ProveedorID,
                                     NombreProveedor = p.NombreEmpresa,
                                     FechaOrden = o.FechaOrden,
                                     Total = o.Total,
                                     Estado = o.Estado
                                 })
                                 .ToList();

                return resultado;
            }
        }

        // ---------------------------------------------------------
        // CARGAR COMBOS
        // ---------------------------------------------------------
        private void CargarCombos(int? proveedorId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Proveedores = new SelectList(
                    context.tbProveedores.AsNoTracking().ToList(),
                    "ProveedorID",
                    "NombreEmpresa",
                    proveedorId
                );
            }
        }
    }
}