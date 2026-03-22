namespace WebApplication1.Models
{
    public class DetalleCompraModel
    {
        public int DetalleCompraID { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}