using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class InventarioController : Controller
    {
        private readonly DATABASE_PYEEntities db = new DATABASE_PYEEntities(); // Aquí creamos la instancia del contexto

        // Consultar todos los productos
        public ActionResult VerInventario()
        {
            var resultado = ConsultarProductos();
            // Consultar todas las categorías
            ViewBag.Categorias = new SelectList(db.tbCategorias.ToList(), "CategoriaID");
            return View(resultado);
        }

        // Agregar nuevo producto (GET)
        [HttpGet]
        public ActionResult AgregarProducto()
        {
            // Obtener todas las categorías para el dropdown
            return View(new Producto());
        }

        // Agregar nuevo producto (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarProducto(Producto producto, HttpPostedFileBase Imagen)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var nuevoProducto = new tbProductos
                {
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Stock = producto.Stock,
                    Precio = producto.Precio,
                    CategoriaID = producto.CategoriaID, // Asociar con el ID de la categoría
                    Imagen = string.Empty, // Iniciar el campo de la imagen vacío
                };

                context.tbProductos.Add(nuevoProducto);
                var resultadoInsercion = context.SaveChanges();

                if (resultadoInsercion > 0)
                {
                    // Lógica para guardar la imagen (si es que se seleccionó)
                    if (Imagen != null && Imagen.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(Imagen.FileName).ToLower();
                        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        if (!validExtensions.Contains(extension))
                        {
                            ViewBag.Mensaje = "Solo se permiten imágenes en formato .jpg, .jpeg, .png o .gif.";
                            return View(producto);
                        }

                        var rutaImagen = Path.Combine(Server.MapPath("~/wwwroot/imgProductos/"), nuevoProducto.ProductoID + extension);
                        var folderPath = Server.MapPath("~/wwwroot/imgProductos/");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        Imagen.SaveAs(rutaImagen);
                        nuevoProducto.Imagen = "imgProductos/" + nuevoProducto.ProductoID + extension;
                        context.SaveChanges();
                    }

                    return RedirectToAction("VerInventario", "Inventario");
                }
            }

            ViewBag.Mensaje = "La información no se pudo insertar.";
            return View();
        }


        // Actualizar un producto (GET)
        [HttpGet]
        public ActionResult ActualizarProducto(int? q)
        {
            if (!q.HasValue)
            {
                return RedirectToAction("VerInventario", "Inventario");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos
                    .Where(p => p.ProductoID == q)
                    .FirstOrDefault();

                if (producto == null)
                {
                    TempData["Mensaje"] = "Producto no encontrado.";
                    return RedirectToAction("VerInventario", "Inventario");
                }

                var datos = new Producto
                {
                    ProductoID = producto.ProductoID,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Stock = producto.Stock,
                    Precio = producto.Precio,
                    CategoriaID = producto.CategoriaID, // Solo cargar CategoriaID
                    Imagen = producto.Imagen
                };

                ViewBag.Categorias = new SelectList(context.tbCategorias.ToList(), "CategoriaID", "CategoriaID"); // Solo mostrar el ID de la categoría
                return View(datos);
            }
        }
        // Actualizar un producto (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarProducto(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var productoExistente = context.tbProductos
                        .FirstOrDefault(p => p.ProductoID == producto.ProductoID);

                    if (productoExistente == null)
                    {
                        ViewBag.Mensaje = "Producto no encontrado.";
                        return View(producto);
                    }

                    // Solo actualizar el CategoriaID
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Stock = producto.Stock;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.CategoriaID = producto.CategoriaID; // Solo actualizar el CategoriaID
                    productoExistente.Imagen = producto.Imagen;

                    var resultadoActualizacion = context.SaveChanges();

                    if (resultadoActualizacion > 0)
                    {
                        return RedirectToAction("VerInventario", "Inventario");
                    }

                    ViewBag.Mensaje = "No se pudo actualizar la información.";
                    return View(producto);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(producto);
            }
        }
        // Eliminar un producto
        public ActionResult EliminarProducto(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos.FirstOrDefault(p => p.ProductoID == q);
                if (producto != null)
                {
                    context.tbProductos.Remove(producto);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerInventario", "Inventario");
        }

        // Consultar productos
        private List<Producto> ConsultarProductos()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbProductos
                    .ToList()
                    .Select(p => new Producto
                    {
                        ProductoID = p.ProductoID,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Stock = p.Stock,
                        Precio = p.Precio,
                        Imagen = p.Imagen,
                        CategoriaID = p.CategoriaID,

                    })
                    .ToList();

                return resultado;
            }
        }


    }
}