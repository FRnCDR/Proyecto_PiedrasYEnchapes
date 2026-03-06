using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CategoriaController : Controller
    {
        // ---------------------------------------------------------
        // LISTAR
        // ---------------------------------------------------------
        public ActionResult VerCategorias()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var categoriasModelo = context.tbCategorias
                    .AsNoTracking()
                    .Select(c => new Categoria
                    {
                        CategoriaID = c.CategoriaID,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion
                    })
                    .ToList();

                return View(categoriasModelo);
            }
        }

        // ---------------------------------------------------------
        // GET: AGREGAR
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult AgregarCategoria()
        {
            return View(new Categoria());
        }

        // ---------------------------------------------------------
        // POST: AGREGAR
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarCategoria(Categoria categoria)
        {
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var nuevaCategoria = new tbCategorias
                    {
                        Nombre = categoria.Nombre,
                        Descripcion = categoria.Descripcion
                    };

                    context.tbCategorias.Add(nuevaCategoria);
                    var resultado = context.SaveChanges();

                    if (resultado > 0) return RedirectToAction("VerCategorias");

                    ViewBag.Mensaje = "No se pudo agregar la categoría.";
                    return View(categoria);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(categoria);
            }
        }

        // ---------------------------------------------------------
        // GET: ACTUALIZAR
        // ---------------------------------------------------------
        [HttpGet]
        public ActionResult ActualizarCategoria(int? id)
        {
            if (!id.HasValue) return RedirectToAction("VerCategorias");

            using (var context = new DATABASE_PYEEntities())
            {
                var categoria = context.tbCategorias
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CategoriaID == id.Value);

                if (categoria == null)
                {
                    TempData["Mensaje"] = "Categoría no encontrada.";
                    return RedirectToAction("VerCategorias");
                }

                var model = new Categoria
                {
                    CategoriaID = categoria.CategoriaID,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion
                };

                return View(model);
            }
        }

        // ---------------------------------------------------------
        // POST: ACTUALIZAR
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarCategoria(Categoria categoria)
        {
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var existente = context.tbCategorias
                        .FirstOrDefault(c => c.CategoriaID == categoria.CategoriaID);

                    if (existente == null)
                    {
                        ViewBag.Mensaje = "Categoría no encontrada.";
                        return View(categoria);
                    }

                    existente.Nombre = categoria.Nombre;
                    existente.Descripcion = categoria.Descripcion;

                    var resultado = context.SaveChanges();
                    if (resultado > 0) return RedirectToAction("VerCategorias");

                    ViewBag.Mensaje = "No se pudo actualizar la información.";
                    return View(categoria);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(categoria);
            }
        }

        // ---------------------------------------------------------
        // ELIMINAR
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarCategoria(int id)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var categoria = context.tbCategorias.FirstOrDefault(c => c.CategoriaID == id);
                if (categoria != null)
                {
                    context.tbCategorias.Remove(categoria);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerCategorias");
        }
    }
}