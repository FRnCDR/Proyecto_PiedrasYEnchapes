using System.Data.Entity;

namespace PiedrasYEnchapes.DA
{
    public class Contexto : DbContext
    {
        public Contexto()
        {
            Database.SetInitializer<Contexto>(null);
        }

    }
}
