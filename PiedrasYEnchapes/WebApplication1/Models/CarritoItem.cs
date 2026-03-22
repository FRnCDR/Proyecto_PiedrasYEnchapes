namespace WebApplication1.Models
{
    public class CarritoItem
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }

        public decimal Subtotal
        {
            get { return Precio * Cantidad; }
        }
    }
}