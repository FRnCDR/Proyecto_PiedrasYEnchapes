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
                    //.Where(p => p.Estado == true)
                    .Select(p => new Producto
                    {
                        ProductoID = p.ProductoID,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Imagen = p.Imagen,
                        CategoriaID = p.CategoriaID,
                        ProveedorID = p.ProveedorID
                    })
                    .ToList();

                return View(lista);
            }
        }
    }
}