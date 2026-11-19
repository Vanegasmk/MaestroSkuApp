namespace MaestroSkuApp.Models
{
    /// <summary>
    /// Tabla de catálogo dependiente (Anexo A.2).
    /// Relaciona cada Motivo (CREACIÓN, MODIFICACIÓN, REACTIVACIÓN) con sus Categorías motivo permitidas.
    /// </summary>
    public class CatalogoMotivo
    {
        public int Id { get; set; }
        public string Motivo { get; set; } = string.Empty; // Ej: CREACIÓN, MODIFICACIÓN, REACTIVACIÓN
        public string CategoriaMotivo { get; set; } = string.Empty; // Categoría asociada al Motivo

    }
}
