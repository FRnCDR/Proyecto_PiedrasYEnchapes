using System;

namespace WebApplication1.Models
{
    public class HistorialCompra
    {
        public int CompraID { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
    }
}