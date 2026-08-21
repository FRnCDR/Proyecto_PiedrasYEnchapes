using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;
using WebApplication1.Filtros;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class CarritoController : Controller
    {
        private List<CarritoItem> ObtenerCarrito()
        {
            if (Session["Carrito"] == null)
            {
                Session["Carrito"] = new List<CarritoItem>();
            }

            return (List<CarritoItem>)Session["Carrito"];
        }

        public ActionResult VerCarrito()
        {
            var carrito = ObtenerCarrito();

            // El stock se refresca aquí porque el carrito vive en sesión y el
            // inventario pudo cambiar desde que se agregó el producto.
            if (carrito.Any())
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var ids = carrito.Select(x => x.ProductoID).ToList();

                    var productos = context.tbProductos
                        .Where(p => ids.Contains(p.ProductoID))
                        .Select(p => new { p.ProductoID, p.Stock, p.Precio, p.Estado })
                        .ToList();

                    foreach (var item in carrito.ToList())
                    {
                        var producto = productos.FirstOrDefault(p => p.ProductoID == item.ProductoID);

                        if (producto == null || !producto.Estado)
                        {
                            carrito.Remove(item);
                            TempData["Mensaje"] = "Se quitaron productos que ya no están disponibles.";
                            TempData["TipoMensaje"] = "warning";
                            continue;
                        }

                        item.Stock = producto.Stock;
                        item.Precio = producto.Precio;

                        // Si el stock bajó, se ajusta la cantidad en lugar de dejar
                        // que la compra falle al confirmar.
                        if (item.Cantidad > producto.Stock)
                        {
                            item.Cantidad = producto.Stock;
                            TempData["Mensaje"] = "Se ajustaron cantidades por disponibilidad de stock.";
                            TempData["TipoMensaje"] = "warning";
                        }
                    }

                    carrito.RemoveAll(x => x.Cantidad <= 0);
                    Session["Carrito"] = carrito;
                }
            }

            return View(carrito);
        }

        /// <summary>
        /// Fija la cantidad de una línea del carrito (no la suma, la reemplaza).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.ProductoID == productoId);

            if (item == null)
            {
                TempData["Mensaje"] = "El producto ya no está en el carrito.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction("VerCarrito");
            }

            // Cantidad 0 o menos equivale a quitar la línea.
            if (cantidad <= 0)
            {
                carrito.Remove(item);
                Session["Carrito"] = carrito;

                TempData["Mensaje"] = "Producto eliminado del carrito.";
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("VerCarrito");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos
                    .FirstOrDefault(p => p.ProductoID == productoId);

                if (producto == null || !producto.Estado)
                {
                    carrito.Remove(item);
                    Session["Carrito"] = carrito;

                    TempData["Mensaje"] = "Este producto ya no está disponible.";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("VerCarrito");
                }

                if (cantidad > producto.Stock)
                {
                    cantidad = producto.Stock;
                    TempData["Mensaje"] = "Solo hay " + producto.Stock + " unidad(es) disponibles de '" + producto.Nombre + "'.";
                    TempData["TipoMensaje"] = "warning";
                }
                else
                {
                    TempData["Mensaje"] = "Cantidad actualizada.";
                    TempData["TipoMensaje"] = "success";
                }

                if (cantidad <= 0)
                {
                    carrito.Remove(item);
                }
                else
                {
                    item.Cantidad = cantidad;
                    item.Stock = producto.Stock;
                    item.Precio = producto.Precio;
                }
            }

            Session["Carrito"] = carrito;
            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarAlCarrito(int productoId, int cantidad = 1)
        {
            if (cantidad <= 0)
            {
                TempData["Mensaje"] = "Cantidad no válida.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos.FirstOrDefault(p => p.ProductoID == productoId);

                if (producto == null)
                {
                    TempData["Mensaje"] = "Producto no encontrado.";
                    TempData["TipoMensaje"] = "error";
                    return RedirectToAction("Index", "Home");
                }

                if (!producto.Estado)
                {
                    TempData["Mensaje"] = "Este producto no está disponible.";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                if (producto.Stock <= 0)
                {
                    TempData["Mensaje"] = "Este producto está agotado.";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                var carrito = ObtenerCarrito();
                var itemExistente = carrito.FirstOrDefault(x => x.ProductoID == productoId);

                int cantidadEnCarrito = itemExistente != null ? itemExistente.Cantidad : 0;
                int cantidadTotal = cantidadEnCarrito + cantidad;

                if (cantidadTotal > producto.Stock)
                {
                    TempData["Mensaje"] = "No hay suficiente stock disponible.";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("Index", "Home");
                }

                if (itemExistente != null)
                {
                    itemExistente.Cantidad += cantidad;
                }
                else
                {
                    carrito.Add(new CarritoItem
                    {
                        ProductoID = producto.ProductoID,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = cantidad,
                        Imagen = producto.Imagen,
                        Stock = producto.Stock
                    });
                }

                Session["Carrito"] = carrito;
                TempData["Mensaje"] = "Producto agregado al carrito.";
                TempData["TipoMensaje"] = "success";
            }

            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarDelCarrito(int productoId)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(x => x.ProductoID == productoId);

            if (item != null)
            {
                carrito.Remove(item);
            }

            Session["Carrito"] = carrito;

            TempData["Mensaje"] = "Producto eliminado del carrito.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarCompra()
        {
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);
            var carrito = ObtenerCarrito();

            if (carrito == null || !carrito.Any())
            {
                TempData["Mensaje"] = "El carrito está vacío.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction("VerCarrito");
            }

            // ID de la compra registrada, para redirigir luego al comprobante.
            int nuevaCompraId;

            using (var context = new DATABASE_PYEEntities())
            {
                foreach (var item in carrito)
                {
                    var producto = context.tbProductos.FirstOrDefault(p => p.ProductoID == item.ProductoID);

                    if (producto == null)
                    {
                        TempData["Mensaje"] = "Uno de los productos ya no existe.";
                        TempData["TipoMensaje"] = "error";
                        return RedirectToAction("VerCarrito");
                    }

                    if (!producto.Estado)
                    {
                        TempData["Mensaje"] = "El producto '" + producto.Nombre + "' ya no está disponible.";
                        TempData["TipoMensaje"] = "warning";
                        return RedirectToAction("VerCarrito");
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        TempData["Mensaje"] = "No hay suficiente stock para '" + producto.Nombre + "'.";
                        TempData["TipoMensaje"] = "warning";
                        return RedirectToAction("VerCarrito");
                    }
                }

                decimal totalCompra = carrito.Sum(x => x.Subtotal);

                // La compra nace Pendiente: solo el administrador la marca como
                // Completada (o Cancelada) desde Administración -> Compras.
                var nuevaCompra = new tbCompras
                {
                    IdUsuario = idUsuario,
                    FechaCompra = DateTime.Now,
                    Total = totalCompra,
                    Estado = "Pendiente"
                };

                context.tbCompras.Add(nuevaCompra);
                context.SaveChanges();

                foreach (var item in carrito)
                {
                    var detalle = new tbDetalleCompra
                    {
                        CompraID = nuevaCompra.CompraID,
                        ProductoID = item.ProductoID,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Subtotal = item.Subtotal
                    };

                    context.tbDetalleCompra.Add(detalle);

                    var producto = context.tbProductos.FirstOrDefault(p => p.ProductoID == item.ProductoID);
                    if (producto != null)
                    {
                        producto.Stock -= item.Cantidad;
                    }
                }

                context.SaveChanges();

                nuevaCompraId = nuevaCompra.CompraID;
            }

            Session["Carrito"] = new List<CarritoItem>();

            TempData["Mensaje"] = "Compra registrada y pendiente de confirmación. Coordina la entrega por WhatsApp.";
            TempData["TipoMensaje"] = "success";

            // Redirige al comprobante de la compra recién registrada, donde está el botón de WhatsApp.
            return RedirectToAction("DetalleCompra", "Cliente", new { id = nuevaCompraId });
        }
    }
}