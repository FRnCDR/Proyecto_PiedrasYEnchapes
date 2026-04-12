using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;
using WebApplication1.Filtros;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class ProductosController : Controller
    {
        public ActionResult VerProductos()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var lista = context.tbProductos
                    .AsNoTracking()
                    .Select(p => new Producto
                    {
                        ProductoID = p.ProductoID,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Imagen = p.Imagen,
                        ImagenEjemplo = p.ImagenEjemplo,
                        CategoriaID = p.CategoriaID,
                        ProveedorID = p.ProveedorID,
                        Estado = p.Estado
                    })
                    .ToList();

                return View(lista);
            }
        }

        //Para ver el detalle de los productos

        public ActionResult DetalleProducto(int id)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var producto = context.tbProductos
                    .Where(p => p.ProductoID == id)
                    .Select(p => new WebApplication1.Models.Producto
                    {
                        ProductoID = p.ProductoID,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Imagen = p.Imagen,
                        ImagenEjemplo = p.ImagenEjemplo,
                        CategoriaID = p.CategoriaID,
                        ProveedorID = p.ProveedorID,
                        Estado = p.Estado
                    })
                    .FirstOrDefault();

                if (producto == null)
                {
                    return HttpNotFound();
                }

                return View(producto);
            }
        }
    }


}