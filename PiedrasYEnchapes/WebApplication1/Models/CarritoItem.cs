namespace WebApplication1.Models
{
    public class CarritoItem
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }

        // Stock disponible, refrescado al abrir el carrito: el producto puede
        // cambiar mientras los artículos siguen en la sesión.
        public int Stock { get; set; }

        public decimal Subtotal
        {
            get { return Precio * Cantidad; }
        }
    }
}