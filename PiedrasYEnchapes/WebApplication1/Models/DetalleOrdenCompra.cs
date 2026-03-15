using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DetalleOrdenCompra
    {
        public int DetalleOrdenCompraID { get; set; }

        public int OrdenCompraID { get; set; }

        [Required]
        public int ProductoID { get; set; }

        public string NombreProducto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioCompra { get; set; }

        public decimal Subtotal { get; set; }
    }
}