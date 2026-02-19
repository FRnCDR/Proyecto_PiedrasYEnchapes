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
    public class CategoriaController : Controller
    {
        private readonly DATABASE_PYEEntities db = new DATABASE_PYEEntities(); // Aquí creamos la instancia del contexto

       public ActionResult VerCategorias()
{
    // Consultamos las categorías desde la base de datos
    var categorias = db.tbCategorias.ToList();

    // Convertimos las categorías de la base de datos al modelo adecuado
    var categoriasModelo = categorias.Select(c => new Categoria
    {
        CategoriaID = c.CategoriaID,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion
    }).ToList();

    // Pasamos el modelo a la vista
    return View(categoriasModelo);  // Ahora pasamos List<Categoria> en lugar de List<tbCategorias>
}

        // Agregar nueva categoría (GET)
        [HttpGet]
        public ActionResult AgregarCategoria()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarCategoria(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var nuevaCategoria = new tbCategorias
                {
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion
                };

                context.tbCategorias.Add(nuevaCategoria);
                var resultado = context.SaveChanges();

                if (resultado > 0)
                {
                    return RedirectToAction("VerCategorias");
                }

                ViewBag.Mensaje = "No se pudo agregar la categoría.";
                return View(categoria);
            }
        }

        // Actualizar una categoría (GET)
        [HttpGet]
        public ActionResult ActualizarCategoria(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("VerCategorias", "Categoria");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var categoria = context.tbCategorias
                    .Where(c => c.CategoriaID == id)
                    .FirstOrDefault();

                if (categoria == null)
                {
                    TempData["Mensaje"] = "Categoría no encontrada.";
                    return RedirectToAction("VerCategorias", "Categoria");
                }

                // Mapear los datos a un modelo de categoría
                var categoriaModel = new Categoria
                {
                    CategoriaID = categoria.CategoriaID,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion
                };

                return View(categoriaModel); // Pasar los datos a la vista
            }
        }

        // Actualizar una categoría (POST)
        [HttpPost]
        public ActionResult ActualizarCategoria(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria); // Si el modelo no es válido, regresar a la vista
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var categoriaExistente = context.tbCategorias
                        .FirstOrDefault(c => c.CategoriaID == categoria.CategoriaID);

                    if (categoriaExistente == null)
                    {
                        ViewBag.Mensaje = "Categoría no encontrada.";
                        return View(categoria); // Si no se encuentra la categoría, mostrar mensaje
                    }

                    // Actualizar los valores de la categoría
                    categoriaExistente.Nombre = categoria.Nombre;
                    categoriaExistente.Descripcion = categoria.Descripcion;

                    var resultadoActualizacion = context.SaveChanges(); // Guardar los cambios

                    if (resultadoActualizacion > 0)
                    {
                        return RedirectToAction("VerCategorias", "Categoria"); // Redirigir si la actualización fue exitosa
                    }

                    ViewBag.Mensaje = "No se pudo actualizar la información.";
                    return View(categoria); // Volver a la vista con el error
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(categoria); // Si ocurre un error, mostrarlo en la vista
            }
        }

        public ActionResult EliminarCategoria(int id)
        {
            var categoria = db.tbCategorias.FirstOrDefault(c => c.CategoriaID == id);
            if (categoria != null)
            {
                db.tbCategorias.Remove(categoria);
                db.SaveChanges();
            }

            return RedirectToAction("VerCategorias");
        }

    }
}