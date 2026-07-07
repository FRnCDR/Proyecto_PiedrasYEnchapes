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
        private const int ProductosPorPagina = 12;

        public ActionResult VerProductos(int pagina = 1)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var query = context.tbProductos.AsNoTracking();

                var totalProductos = query.Count();
                var totalPaginas = (int)Math.Ceiling(totalProductos / (double)ProductosPorPagina);

                if (pagina < 1) pagina = 1;
                if (totalPaginas > 0 && pagina > totalPaginas) pagina = totalPaginas;

                var lista = query
                    .OrderBy(p => p.Nombre)
                    .Skip((pagina - 1) * ProductosPorPagina)
                    .Take(ProductosPorPagina)
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

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;

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