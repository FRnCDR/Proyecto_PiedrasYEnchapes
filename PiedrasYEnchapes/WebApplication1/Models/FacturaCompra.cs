using System;

namespace WebApplication1.Models
{
    public class FacturaCompra
    {
        public int FacturaCompraID { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoPagado { get; set; }
        public string Estado { get; set; }
        public string NombreProveedor { get; set; }
    }
}