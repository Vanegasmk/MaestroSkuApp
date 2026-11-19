using MaestroSkuApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MaestroSkuApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        // Tabla consolidada única donde se crean (Etapa 1) y actualizan (Etapa 2) los SKU
        public DbSet<SolicitudSku> SolicitudesSku => Set<SolicitudSku>();

        // Tablas de catálogos para filtros dependientes y jerárquicos
        public DbSet<CatalogoMotivo> CatalogoMotivos => Set<CatalogoMotivo>();
        public DbSet<CatalogoJerarquia> CatalogoJerarquias => Set<CatalogoJerarquia>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precarga de Motivo -> Categoría motivo
            modelBuilder.Entity<CatalogoMotivo>().HasData(
                new CatalogoMotivo { Id = 1, Motivo = "CREACIÓN", CategoriaMotivo = "Producto nuevo" },
                new CatalogoMotivo { Id = 2, Motivo = "CREACIÓN", CategoriaMotivo = "Producto sustituto" },
                new CatalogoMotivo { Id = 3, Motivo = "CREACIÓN", CategoriaMotivo = "Producto con variantes" },

                new CatalogoMotivo { Id = 4, Motivo = "REACTIVACIÓN", CategoriaMotivo = "Reactivación por disponibilidad" },
                new CatalogoMotivo { Id = 5, Motivo = "REACTIVACIÓN", CategoriaMotivo = "Reactivación por cambio de proveedor" },
                new CatalogoMotivo { Id = 6, Motivo = "REACTIVACIÓN", CategoriaMotivo = "Reactivación por corrección de datos" },

                new CatalogoMotivo { Id = 7, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Clasificación" },
                new CatalogoMotivo { Id = 8, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Lead Time" },
                new CatalogoMotivo { Id = 9, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Descripción" },
                new CatalogoMotivo { Id = 10, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de código de barras y/o GTIN14" },
                new CatalogoMotivo { Id = 11, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de UXB y/o medidas" },
                new CatalogoMotivo { Id = 12, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Proveedor" },
                new CatalogoMotivo { Id = 13, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Impuesto" },
                new CatalogoMotivo { Id = 14, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Departamento/Categoría" },
                new CatalogoMotivo { Id = 15, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Vida útil" },
                new CatalogoMotivo { Id = 16, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Unidad y/o Valor de medida" },
                new CatalogoMotivo { Id = 17, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Comprador" },
                new CatalogoMotivo { Id = 18, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Garantía" },
                new CatalogoMotivo { Id = 19, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Registro Sanitario" },
                new CatalogoMotivo { Id = 20, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Restricción alimenticia" },
                new CatalogoMotivo { Id = 21, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Información del almacenaje" },
                new CatalogoMotivo { Id = 22, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Aroma/Sabor/Tipo Aceite" },
                new CatalogoMotivo { Id = 23, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de CABYS" },
                new CatalogoMotivo { Id = 24, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Tiempo de Producción" },
                new CatalogoMotivo { Id = 25, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Temporada" },
                new CatalogoMotivo { Id = 26, Motivo = "MODIFICACIÓN", CategoriaMotivo = "Modificación de Jerarquía" }
            );

            // Precarga de Jerarquía: Departamento -> Categoría -> Subcategoría -> Tipo
            modelBuilder.Entity<CatalogoJerarquia>().HasData(
                new CatalogoJerarquia { Id = 1, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES EN SPRAY", Tipo = "ACEITE DE AGUACATE EN SPRAY" },
                new CatalogoJerarquia { Id = 2, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES EN SPRAY", Tipo = "ACEITE DE CANOLA EN SPRAY" },
                new CatalogoJerarquia { Id = 3, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES EN SPRAY", Tipo = "ACEITE DE COCO EN SPRAY" },
                new CatalogoJerarquia { Id = 4, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES LÍQUIDOS", Tipo = "ACEITE DE AGUACATE" },
                new CatalogoJerarquia { Id = 5, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES LÍQUIDOS", Tipo = "ACEITE DE CANOLA" },
                new CatalogoJerarquia { Id = 6, Departamento = "ABARROTES", Categoria = "ACEITES Y GRASAS", Subcategoria = "ACEITES LÍQUIDOS", Tipo = "ACEITE DE COCO" },
                new CatalogoJerarquia { Id = 7, Departamento = "BEBÉ", Categoria = "ALIMENTOS INFANTILES", Subcategoria = "CEREALES INFANTILES", Tipo = "CEREAL DE ARROZ INFANTIL" },
                new CatalogoJerarquia { Id = 8, Departamento = "BEBÉ", Categoria = "ALIMENTOS INFANTILES", Subcategoria = "CEREALES INFANTILES", Tipo = "CEREAL DE AVENA INFANTIL" },
                new CatalogoJerarquia { Id = 9, Departamento = "BEBÉ", Categoria = "ALIMENTOS INFANTILES", Subcategoria = "COLADOS INFANTILES", Tipo = "COLADO DE FRUTAS" },
                new CatalogoJerarquia { Id = 10, Departamento = "BEBÉ", Categoria = "ALIMENTOS INFANTILES", Subcategoria = "COLADOS INFANTILES", Tipo = "COLADO DE VEGETALES" },
                new CatalogoJerarquia { Id = 11, Departamento = "BEBÉ", Categoria = "BAÑO Y CUIDADO DEL BEBÉ", Subcategoria = "JABONES INFANTILES", Tipo = "JABÓN LÍQUIDO INFANTIL" },
                new CatalogoJerarquia { Id = 12, Departamento = "BEBÉ", Categoria = "BAÑO Y CUIDADO DEL BEBÉ", Subcategoria = "SHAMPOO INFANTIL", Tipo = "SHAMPOO SUAVE INFANTIL" }
            );
        }

    }
}
