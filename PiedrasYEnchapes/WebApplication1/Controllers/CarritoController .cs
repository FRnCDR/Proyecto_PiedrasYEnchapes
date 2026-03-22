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
            return View(carrito);
        }

        [HttpPost]
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
                        Imagen = producto.Imagen
                    });
                }

                Session["Carrito"] = carrito;
                TempData["Mensaje"] = "Producto agregado al carrito.";
                TempData["TipoMensaje"] = "success";
            }

            return RedirectToAction("VerCarrito");
        }

        [HttpPost]
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

                var nuevaCompra = new tbCompras
                {
                    IdUsuario = idUsuario,
                    FechaCompra = DateTime.Now,
                    Total = totalCompra,
                    Estado = "Completada"
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
            }

            Session["Carrito"] = new List<CarritoItem>();

            TempData["Mensaje"] = "Compra realizada correctamente.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction("HistorialCompras", "Cliente");
        }
    }
}