using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class OrdenCompra
    {
        public int OrdenCompraID { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        public int ProveedorID { get; set; }

        public string NombreProveedor { get; set; }

        [Required(ErrorMessage = "Debe ingresar la fecha de la orden.")]
        public DateTime FechaOrden { get; set; }

        [Required(ErrorMessage = "Debe ingresar el total.")]
        public decimal Total { get; set; }

        public bool Estado { get; set; }

        public string EstadoRecepcion { get; set; }
    }
}