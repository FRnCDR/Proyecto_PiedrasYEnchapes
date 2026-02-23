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

        [HttpGet]
        public ActionResult AgregarProducto()
        {
            // Obtener todas las categorías y proveedores
            ViewBag.Categorias = new SelectList(db.tbCategorias, "CategoriaID", "Nombre");
            ViewBag.Proveedores = new SelectList(db.tbProveedores, "ProveedorID", "NombreEmpresa");

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
                    ProveedorID = (int)producto.ProveedorID,
                    Imagen = string.Empty // Iniciar el campo de la imagen vacío
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


        // GET: ActualizarProducto
        [HttpGet]
        public ActionResult ActualizarProducto(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerInventario");

            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos.FirstOrDefault(p => p.ProductoID == q.Value);
                if (producto == null)
                {
                    TempData["Mensaje"] = "Producto no encontrado.";
                    return RedirectToAction("VerInventario");
                }

                var datos = new Producto
                {
                    ProductoID = producto.ProductoID,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Stock = producto.Stock,
                    Precio = producto.Precio,
                    CategoriaID = producto.CategoriaID,
                    ProveedorID = producto.ProveedorID,
                    Imagen = producto.Imagen
                };

                // Cargar dropdowns con valor seleccionado
                ViewBag.ListaCategorias = new SelectList(context.tbCategorias.ToList(), "CategoriaID", "Nombre", datos.CategoriaID);
                ViewBag.ListaProveedores = new SelectList(context.tbProveedores.ToList(), "ProveedorID", "NombreEmpresa", datos.ProveedorID);

                return View(datos);
            }
        }


        // POST: ActualizarProducto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarProducto(Producto producto, HttpPostedFileBase Imagen)
        {
            if (!ModelState.IsValid)
            {
                // Recargar dropdowns si hay error de validación
                using (var context = new DATABASE_PYEEntities())
                {
                    ViewBag.Categorias = new SelectList(context.tbCategorias.ToList(), "CategoriaID", "Nombre", producto.CategoriaID);
                    ViewBag.Proveedores = new SelectList(context.tbProveedores.ToList(), "ProveedorID", "NombreEmpresa", producto.ProveedorID);
                }

                return View(producto);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var productoExistente = context.tbProductos.FirstOrDefault(p => p.ProductoID == producto.ProductoID);
                    if (productoExistente == null)
                    {
                        ViewBag.Mensaje = "Producto no encontrado.";
                        return View(producto);
                    }

                    // Actualizar campos
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Stock = producto.Stock;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.CategoriaID = producto.CategoriaID;

                    if (producto.ProveedorID.HasValue)
                        productoExistente.ProveedorID = producto.ProveedorID.Value;

                    // Manejo de imagen opcional
                    if (Imagen != null && Imagen.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(Imagen.FileName).ToLower();
                        var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        if (!validExtensions.Contains(extension))
                        {
                            ViewBag.Mensaje = "Solo se permiten imágenes en formato .jpg, .jpeg, .png o .gif.";
                            // Recargar dropdowns antes de retornar
                            ViewBag.ListaCategorias = new SelectList(context.tbCategorias.ToList(), "CategoriaID", "Nombre", producto.CategoriaID);
                            ViewBag.ListaProveedores = new SelectList(context.tbProveedores.ToList(), "ProveedorID", "NombreEmpresa", producto.ProveedorID);
                            return View(producto);
                        }

                        var folderPath = Server.MapPath("~/wwwroot/imgProductos/");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var rutaImagen = Path.Combine(folderPath, productoExistente.ProductoID + extension);
                        Imagen.SaveAs(rutaImagen);
                        productoExistente.Imagen = "imgProductos/" + productoExistente.ProductoID + extension;
                    }

                    context.SaveChanges();
                    TempData["Mensaje"] = "Producto actualizado correctamente.";
                    return RedirectToAction("VerInventario");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                using (var context = new DATABASE_PYEEntities())
                {
                    ViewBag.Categorias = new SelectList(context.tbCategorias.ToList(), "CategoriaID", "Nombre", producto.CategoriaID);
                    ViewBag.Proveedores = new SelectList(context.tbProveedores.ToList(), "ProveedorID", "NombreEmpresa", producto.ProveedorID);
                }
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