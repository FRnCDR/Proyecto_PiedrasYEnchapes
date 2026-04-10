using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.EF;
using WebApplication1.Filtros;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ValidarSesion]
    public class ClienteController : Controller
    {
        // Consultar todos los clientes
        public ActionResult VerClientes()
        {
            var resultado = ConsultarClientes();
            return View(resultado);
        }

        // Agregar nuevo cliente (GET)
        [HttpGet]
        public ActionResult AgregarCliente()
        {
            return View(new Cliente());
        }

        [HttpPost]
        public ActionResult AgregarCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "El modelo NO es válido";
                return View(cliente);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    // Validar correo duplicado
                    bool correoExiste = context.tbClientes
                        .Any(c => c.CorreoElectronico == cliente.CorreoElectronico);

                    if (correoExiste)
                    {
                        ViewBag.Mensaje = "El correo ingresado ya está registrado.";
                        return View(cliente);
                    }

                    // Validar identificación duplicada
                    bool identificacionExiste = context.tbClientes
                        .Any(c => c.Identificacion == cliente.Identificacion);

                    if (identificacionExiste)
                    {
                        ViewBag.Mensaje = "La identificación ya está registrada.";
                        return View(cliente);
                    }

                    var nuevoCliente = new tbClientes
                    {
                        Identificacion = cliente.Identificacion,
                        Nombre = cliente.Nombre,
                        Apellidos = cliente.Apellidos,
                        CorreoElectronico = cliente.CorreoElectronico,
                        Telefono = cliente.Telefono,
                        Direccion = cliente.Direccion,
                        Estado = cliente.Estado, 
                        FechaRegistro = DateTime.Now
                    };

                    context.tbClientes.Add(nuevoCliente);
                    var resultadoInsercion = context.SaveChanges();

                    if (resultadoInsercion > 0)
                    {
                        return RedirectToAction("VerClientes", "Cliente");
                    }
                }

                ViewBag.Mensaje = "No se pudo registrar el cliente.";
                return View(cliente);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        ViewBag.Mensaje += $"Propiedad: {ve.PropertyName} - Error: {ve.ErrorMessage}<br/>";
                    }
                }

                return View(cliente);
            }
        }

        [HttpGet]
        public ActionResult ActualizarCliente(int? q)
        {
            if (!q.HasValue)
            {
                return RedirectToAction("VerClientes", "Cliente");
            }

            using (var context = new DATABASE_PYEEntities())
            {
                var cliente = context.tbClientes
                    .FirstOrDefault(c => c.IdCliente == q.Value);

                if (cliente == null)
                {
                    TempData["Mensaje"] = "Cliente no encontrado.";
                    return RedirectToAction("VerClientes", "Cliente");
                }

                var datos = new Cliente
                {
                    IdCliente = cliente.IdCliente,
                    Identificacion = cliente.Identificacion,
                    Nombre = cliente.Nombre,
                    Apellidos = cliente.Apellidos,
                    CorreoElectronico = cliente.CorreoElectronico,
                    Telefono = cliente.Telefono,
                    Direccion =  cliente.Direccion,
                    FechaRegistro = cliente.FechaRegistro,
                    Estado = cliente.Estado
                };

                ModelState.Clear();

                return View(datos);
            }
        }

        // Actualizar cliente (POST)
        [HttpPost]
        public ActionResult ActualizarCliente(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                using (var context = new DATABASE_PYEEntities())
                {
                    var clienteExistente = context.tbClientes
                        .FirstOrDefault(c => c.IdCliente == cliente.IdCliente);

                    if (clienteExistente == null)
                    {
                        ViewBag.Mensaje = "Cliente no encontrado.";
                        return View(cliente);
                    }

                    // Validar correo duplicado
                    bool correoDuplicado = context.tbClientes
                        .Any(c => c.CorreoElectronico == cliente.CorreoElectronico
                                  && c.IdCliente != cliente.IdCliente);

                    if (correoDuplicado)
                    {
                        ViewBag.Mensaje = "El correo ya está asignado a otro cliente.";
                        return View(cliente);
                    }


                    clienteExistente.Nombre = cliente.Nombre;
                    clienteExistente.Apellidos = cliente.Apellidos;
                    clienteExistente.CorreoElectronico = cliente.CorreoElectronico;
                    clienteExistente.Telefono = cliente.Telefono;
                    clienteExistente.Direccion = cliente.Direccion;
                    clienteExistente.Estado = cliente.Estado;

                    int resultado = context.SaveChanges();

                    if (resultado > 0)
                    {
                        TempData["Mensaje"] = "Cliente actualizado correctamente.";
                        return RedirectToAction("VerClientes", "Cliente");
                    }

                    ViewBag.Mensaje = "No se realizaron cambios.";
                    return View(cliente);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = "Error: " + ex.Message;
                return View(cliente);
            }
        }

        // Cambiar estado (activar/inactivar)
        public ActionResult CambiarEstadoCliente(int q)
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var cliente = context.tbClientes
                    .FirstOrDefault(c => c.IdCliente == q);

                if (cliente != null)
                {
                    cliente.Estado = !cliente.Estado;
                    context.SaveChanges();
                }
            }

            return RedirectToAction("VerClientes", "Cliente");
        }

        // Consultar clientes
        private List<Cliente> ConsultarClientes()
        {
            using (var context = new DATABASE_PYEEntities())
            {
                var resultado = context.tbClientes
                    .Select(c => new Cliente
                    {
                        IdCliente = c.IdCliente,
                        Identificacion = c.Identificacion,
                        Nombre = c.Nombre,
                        Apellidos = c.Apellidos,
                        CorreoElectronico = c.CorreoElectronico,
                        Telefono = c.Telefono,
                        Direccion = c.Direccion,
                        FechaRegistro = c.FechaRegistro,
                        Estado = c.Estado
                    })
                    .ToList();

                return resultado;
            }
        }

        public ActionResult HistorialCompras()
        {
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            using (var context = new DATABASE_PYEEntities())
            {
                var historial = context.tbCompras
                    .Where(c => c.IdUsuario == idUsuario)
                    .OrderByDescending(c => c.FechaCompra)
                    .Select(c => new HistorialCompra
                    {
                        CompraID = c.CompraID,
                        FechaCompra = c.FechaCompra,
                        Total = c.Total,
                        Estado = c.Estado
                    })
                    .ToList();

                return View(historial);
            }
        }

        public ActionResult DetalleCompra(int id)
        {
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            using (var context = new DATABASE_PYEEntities())
            {
                bool compraValida = context.tbCompras.Any(c => c.CompraID == id && c.IdUsuario == idUsuario);

                if (!compraValida)
                {
                    return RedirectToAction("HistorialCompras");
                }

                var detalle = context.tbDetalleCompra
                    .Where(d => d.CompraID == id)
                    .Select(d => new DetalleCompraModel
                    {
                        DetalleCompraID = d.DetalleCompraID,
                        NombreProducto = d.tbProductos.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    })
                    .ToList();

                ViewBag.CompraID = id;
                return View(detalle);
            }
        }

        public ActionResult DescargarPdf(int compraId)
        {
            // Verificar si el usuario está autenticado
            if (Session["IdUsuario"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            using (var context = new DATABASE_PYEEntities())
            {
                bool compraValida = context.tbCompras.Any(c => c.CompraID == compraId && c.IdUsuario == idUsuario);

                if (!compraValida)
                {
                    return RedirectToAction("HistorialCompras");
                }

                var detalle = context.tbDetalleCompra
                    .Where(d => d.CompraID == compraId)
                    .Select(d => new DetalleCompraModel
                    {
                        DetalleCompraID = d.DetalleCompraID,
                        NombreProducto = d.tbProductos.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    })
                    .ToList();

                // Crear el documento PDF
                using (var memoryStream = new MemoryStream())
                {
                    // Crear un documento PDF con tamaño A4
                    Document document = new Document(PageSize.A4);
                    PdfWriter.GetInstance(document, memoryStream);

                    document.Open();

                    // Título del PDF
                    document.Add(new Paragraph($"Detalle de la Compra {compraId}"));
                    document.Add(new Paragraph($"Fecha de compra: {DateTime.Now.ToString("dd/MM/yyyy")}"));
                    document.Add(new Paragraph(" "));

                    // Agregar los detalles de los productos comprados
                    PdfPTable table = new PdfPTable(4);
                    table.AddCell("Producto");
                    table.AddCell("Cantidad");
                    table.AddCell("Precio Unitario");
                    table.AddCell("Subtotal");

                    foreach (var item in detalle)
                    {
                        table.AddCell(item.NombreProducto);
                        table.AddCell(item.Cantidad.ToString());
                        table.AddCell($"₡ {item.PrecioUnitario:N2}");
                        table.AddCell($"₡ {item.Subtotal:N2}");
                    }

                    document.Add(table);

                    // Cerrar el documento PDF
                    document.Close();

                    // Devolver el archivo PDF
                    byte[] fileBytes = memoryStream.ToArray();
                    return File(fileBytes, "application/pdf", $"DetalleCompra_{compraId}.pdf");
                }
            }
        }

    }
}