using System;

namespace WebApplication1.Models
{
    // Fila del listado de compras para el administrador: la compra más los
    // datos del cliente que la realizó.
    public class CompraAdmin
    {
        public int CompraID { get; set; }
        public int IdUsuario { get; set; }
        public string NombreCliente { get; set; }
        public string CorreoCliente { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public int CantidadProductos { get; set; }
    }
}
