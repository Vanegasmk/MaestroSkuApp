namespace MaestroSkuApp.Models
{
    /// <summary>
    /// Tabla de catálogo jerárquico RELEX de 4 niveles (Anexo A.3).
    /// Permite filtrar en cascada: Departamento -> Categoría -> Subcategoría -> Tipo.
    /// </summary>
    public class CatalogoJerarquia
    {
        public int Id { get; set; }
        public string Departamento { get; set; } = string.Empty; // Nivel 1
        public string Categoria { get; set; } = string.Empty; // Nivel 2
        public string Subcategoria { get; set; } = string.Empty; // Nivel 3
        public string Tipo { get; set; } = string.Empty; // Nivel 4

    }
}
