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
    // Gestión de las compras de los clientes. Es exclusiva del administrador:
    // el cliente solo registra la compra (queda Pendiente) y la consulta en su
    // historial, pero no puede cambiarle el estado.
    [ValidarSesion]
    [ValidarPerfilAdmin]
    public class CompraController : Controller
    {
        // Estados válidos de una compra.
        private static readonly string[] EstadosValidos = { "Pendiente", "Completada", "Cancelada" };

        public ActionResult VerCompras()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var lista = (from c in context.tbCompras.AsNoTracking()
                             join u in context.tbUsuario.AsNoTracking()
                                on c.IdUsuario equals u.IdUsuario
                             orderby c.CompraID descending
                             select new CompraAdmin
                             {
                                 CompraID = c.CompraID,
                                 IdUsuario = c.IdUsuario,
                                 NombreCliente = u.Nombre,
                                 CorreoCliente = u.CorreoElectronico,
                                 FechaCompra = c.FechaCompra,
                                 Total = c.Total,
                                 Estado = c.Estado,
                                 CantidadProductos = context.tbDetalleCompra
                                     .Count(d => d.CompraID == c.CompraID)
                             })
                             .ToList();

                return View(lista);
            }
        }

        // Detalle de una compra concreta, sin la restricción de dueño que aplica
        // al cliente en ClienteController.
        public ActionResult DetalleCompraAdmin(int id)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var compra = (from c in context.tbCompras.AsNoTracking()
                              join u in context.tbUsuario.AsNoTracking()
                                 on c.IdUsuario equals u.IdUsuario
                              where c.CompraID == id
                              select new CompraAdmin
                              {
                                  CompraID = c.CompraID,
                                  IdUsuario = c.IdUsuario,
                                  NombreCliente = u.Nombre,
                                  CorreoCliente = u.CorreoElectronico,
                                  FechaCompra = c.FechaCompra,
                                  Total = c.Total,
                                  Estado = c.Estado
                              })
                              .FirstOrDefault();

                if (compra == null)
                {
                    TempData["Mensaje"] = "La compra no existe.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("VerCompras");
                }

                var detalle = context.tbDetalleCompra
                    .AsNoTracking()
                    .Where(d => d.CompraID == id)
                    .Select(d => new DetalleCompraModel
                    {
                        DetalleCompraID = d.DetalleCompraID,
                        NombreProducto = d.tbProductos.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    })
                    .ToList();

                ViewBag.Compra = compra;
                return View(detalle);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstadoCompra(int q, string estado)
        {
            if (!EstadosValidos.Contains(estado))
            {
                TempData["Mensaje"] = "Estado no válido.";
                TempData["TipoMensaje"] = "error";
                return RedirectToAction("VerCompras");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var compra = context.tbCompras.FirstOrDefault(c => c.CompraID == q);

                if (compra == null)
                {
                    TempData["Mensaje"] = "La compra no existe.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("VerCompras");
                }

                if (compra.Estado == estado)
                {
                    TempData["Mensaje"] = "La compra ya está en estado " + estado + ".";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("VerCompras");
                }

                // El stock se descuenta al registrar la compra, así que al cancelar
                // hay que devolverlo; y si se reactiva una cancelada, volver a descontarlo.
                bool estabaCancelada = compra.Estado == "Cancelada";
                bool quedaCancelada = estado == "Cancelada";

                if (estabaCancelada != quedaCancelada)
                {
                    var detalles = context.tbDetalleCompra
                        .Where(d => d.CompraID == compra.CompraID)
                        .ToList();

                    foreach (var d in detalles)
                    {
                        var producto = context.tbProductos
                            .FirstOrDefault(p => p.ProductoID == d.ProductoID);

                        if (producto == null)
                            continue;

                        if (quedaCancelada)
                        {
                            producto.Stock += d.Cantidad;
                        }
                        else
                        {
                            if (producto.Stock < d.Cantidad)
                            {
                                TempData["Mensaje"] = "No hay stock suficiente de '" + producto.Nombre +
                                                      "' para reactivar la compra #" + compra.CompraID + ".";
                                TempData["TipoMensaje"] = "warning";
                                return RedirectToAction("VerCompras");
                            }

                            producto.Stock -= d.Cantidad;
                        }
                    }
                }

                compra.Estado = estado;
                context.SaveChanges();

                TempData["Mensaje"] = "La compra #" + compra.CompraID + " quedó como " + estado + ".";
                TempData["TipoMensaje"] = "success";
            }

            return RedirectToAction("VerCompras");
        }
    }
}
