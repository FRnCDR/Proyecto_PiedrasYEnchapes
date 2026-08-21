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

        public ActionResult VerProductos(int pagina = 1, string buscar = null)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                IQueryable<tbProductos> query = context.tbProductos.AsNoTracking();

                // La búsqueda se aplica en la base de datos (sobre todo el catálogo),
                // no sobre las tarjetas ya renderizadas de la página actual.
                var termino = (buscar ?? string.Empty).Trim();

                if (termino.Length > 0)
                {
                    query = query.Where(p => p.Nombre.Contains(termino)
                                          || p.Descripcion.Contains(termino));
                }

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
                ViewBag.Buscar = termino;
                ViewBag.TotalProductos = totalProductos;

                // El buscador en vivo pide solo el bloque de resultados.
                if (Request.IsAjaxRequest())
                    return PartialView("_ListaProductos", lista);

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