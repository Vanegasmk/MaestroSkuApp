namespace MaestroSkuApp.Data
{
    public class CatalogosEstaticos // Catalogos independientes de la base de datos, para uso en validaciones y selectores
    {
        public static readonly List<string> Motivos = new()
        {
            "Creación",
            "Modificación",
            "Reactivación"
        };

        public static readonly List<string> ProductoSustituto = new()
        {
            "Producto Sustituto",
            "Producto No Sustituto"
        };

        public static readonly List<string> Materiales = new()
        {
            "Plástico",
            "Metal",
            "Vidrio",
            "Madera",
            "Tela"
        };

        public static readonly List<string> Marcas = new()
        {
            "Igloo",
            "PM Home",
            "Genérica"
        };

        public static readonly List<string> Clasificaciones = new()
        {
            "Descontinuado",
            "E",
            "Producción",
            "Reorden",
            "Stocks",
            "Temporada"
        };

        public static readonly List<string> Temporadas = new()
        {
            "Navideña",
            "Escolar",
            "Juguetes",
            "Verano",
            "Frutas y Verduras"
        };

        public static readonly List<string> PaisesOrigen = new()
        {
            "Costa Rica",
            "China",
            "Estados Unidos",
            "México",
            "Guatemala"
        };

        public static readonly List<string> RegistroSanitario = new()
        {
            "YES/SI",
            "NO"
        };

        public static readonly List<string> Embalajes = new()
        {
            "Nested / Anidado",
            "Stacked / Apilado",
            "Master Carton Multiple",
            "Master Carton"
        };

        public static readonly List<int> CantidadCajas = new() { 1, 2, 3 };

    }
}
