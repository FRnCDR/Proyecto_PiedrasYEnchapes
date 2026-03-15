using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;
using WebApplication1.Filtros;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class InventarioController : Controller
    {
        // ---------------------------------------------------------
        // VER INVENTARIO
        // ---------------------------------------------------------
        public ActionResult VerInventario()
        {
            var resultado = ConsultarProductos();

            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Categorias = new SelectList(
                    context.tbCategorias.AsNoTracking().ToList(),
                    "CategoriaID",
                    "Nombre"
                );
            }

            return View(resultado);
        }

        // ---------------------------------------------------------
        // GET: AGREGAR PRODUCTO
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarProducto()
        {
            CargarCombos();
            return View(new Producto());
        }

        // ---------------------------------------------------------
        // POST: AGREGAR PRODUCTO
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarProducto(Producto producto, HttpPostedFileBase Imagen)
        {
            // Validar imagen antes de insertar
            if (Imagen != null && Imagen.ContentLength > 0)
            {
                var extension = Path.GetExtension(Imagen.FileName).ToLower();
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!validExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Imagen", "Solo se permiten imágenes en formato .jpg, .jpeg, .png o .gif.");
                }
            }

            // Validar proveedor si es obligatorio
            if (!producto.ProveedorID.HasValue || producto.ProveedorID.Value <= 0)
            {
                ModelState.AddModelError("ProveedorID", "Debe seleccionar un proveedor.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(producto.CategoriaID, producto.ProveedorID);
                return View(producto);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var nuevoProducto = new tbProductos
                    {
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        Stock = producto.Stock,
                        Precio = producto.Precio,
                        CategoriaID = producto.CategoriaID,
                        ProveedorID = producto.ProveedorID.Value,
                        Imagen = string.Empty
                    };

                    context.tbProductos.Add(nuevoProducto);
                    context.SaveChanges();

                    // Guardar imagen si viene
                    if (Imagen != null && Imagen.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(Imagen.FileName).ToLower();
                        var folderPath = Server.MapPath("~/StaticFiles/images/");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        // Guardar solo el nombre del archivo
                        var nombreArchivo = nuevoProducto.ProductoID + extension;
                        var rutaImagen = Path.Combine(folderPath, nombreArchivo);

                        Imagen.SaveAs(rutaImagen);

                        // En BD solo guardar el nombre
                        nuevoProducto.Imagen = nombreArchivo;
                        context.SaveChanges();
                    }

                    TempData["Mensaje"] = "Producto agregado correctamente.";
                    return RedirectToAction("VerInventario", "Inventario");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarCombos(producto.CategoriaID, producto.ProveedorID);
                return View(producto);
            }
        }

        // ---------------------------------------------------------
        // GET: ACTUALIZAR PRODUCTO
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult ActualizarProducto(int? q)
        {
            if (!q.HasValue)
                return RedirectToAction("VerInventario");

            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos.AsNoTracking().FirstOrDefault(p => p.ProductoID == q.Value);

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

                CargarCombos(datos.CategoriaID, datos.ProveedorID);
                return View(datos);
            }
        }

        // ---------------------------------------------------------
        // POST: ACTUALIZAR PRODUCTO
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarProducto(Producto producto, HttpPostedFileBase Imagen)
        {
            // Validar imagen antes de actualizar
            if (Imagen != null && Imagen.ContentLength > 0)
            {
                var extension = Path.GetExtension(Imagen.FileName).ToLower();
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!validExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Imagen", "Solo se permiten imágenes en formato .jpg, .jpeg, .png o .gif.");
                }
            }

            // Validar proveedor si es obligatorio
            if (!producto.ProveedorID.HasValue || producto.ProveedorID.Value <= 0)
            {
                ModelState.AddModelError("ProveedorID", "Debe seleccionar un proveedor.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos(producto.CategoriaID, producto.ProveedorID);
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
                        CargarCombos(producto.CategoriaID, producto.ProveedorID);
                        return View(producto);
                    }

                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.Stock = producto.Stock;
                    productoExistente.Precio = producto.Precio;
                    productoExistente.CategoriaID = producto.CategoriaID;
                    productoExistente.ProveedorID = producto.ProveedorID.Value;

                    // Actualizar imagen si viene una nueva
                    if (Imagen != null && Imagen.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(Imagen.FileName).ToLower();
                        var folderPath = Server.MapPath("~/StaticFiles/images/");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(productoExistente.Imagen))
                        {
                            var rutaAnterior = Path.Combine(folderPath, productoExistente.Imagen);

                            if (System.IO.File.Exists(rutaAnterior))
                            {
                                System.IO.File.Delete(rutaAnterior);
                            }
                        }

                        // Guardar solo el nombre del archivo
                        var nombreArchivo = productoExistente.ProductoID + extension;
                        var rutaImagen = Path.Combine(folderPath, nombreArchivo);

                        Imagen.SaveAs(rutaImagen);

                        // En BD solo guardar el nombre
                        productoExistente.Imagen = nombreArchivo;
                    }

                    context.SaveChanges();
                    TempData["Mensaje"] = "Producto actualizado correctamente.";
                    return RedirectToAction("VerInventario");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                CargarCombos(producto.CategoriaID, producto.ProveedorID);
                return View(producto);
            }
        }

        // ---------------------------------------------------------
        // ELIMINAR PRODUCTO (POST)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstadoProducto(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos
                    .FirstOrDefault(p => p.ProductoID == q);

                if (producto != null)
                {
                    producto.Estado = !producto.Estado;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerInventario", "Inventario");
        }

        // ---------------------------------------------------------
        // CONSULTAR PRODUCTOS
        // ---------------------------------------------------------
        private List<Producto> ConsultarProductos()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = (from p in context.tbProductos.AsNoTracking()
                                 join c in context.tbCategorias.AsNoTracking()
                                    on p.CategoriaID equals c.CategoriaID
                                 join pr in context.tbProveedores.AsNoTracking()
                                    on p.ProveedorID equals pr.ProveedorID
                                 select new Producto
                                 {
                                     ProductoID = p.ProductoID,
                                     Nombre = p.Nombre,
                                     Descripcion = p.Descripcion,
                                     Stock = p.Stock,
                                     Precio = p.Precio,
                                     Imagen = p.Imagen,
                                     CategoriaID = p.CategoriaID,
                                     ProveedorID = p.ProveedorID,
                                     NombreEmpresa = pr.NombreEmpresa,
                                     Estado = p.Estado

                                 })
                                 .ToList();

                return resultado;
            }
        }

        // ---------------------------------------------------------
        // CARGAR COMBOS
        // ---------------------------------------------------------
        private void CargarCombos(int? categoriaId = null, int? proveedorId = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                ViewBag.Categorias = new SelectList(
                    context.tbCategorias.AsNoTracking().ToList(),
                    "CategoriaID",
                    "Nombre",
                    categoriaId
                );

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