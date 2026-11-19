using MaestroSkuApp.Controllers;
using MaestroSkuApp.Data;
using MaestroSkuApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MaestroSkuApp.Tests
{
    public class SolicitudesControllerTests
    {
        private static AppDbContext CrearContexto(string nombreDb)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: nombreDb)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private static SolicitudesController CrearControlador(AppDbContext context)
        {
            return new SolicitudesController(context)
            {
                TempData = new TempDataDictionary(
                    new DefaultHttpContext(),
                    new FakeTempDataProvider()
                )
            };
        }

        private static SolicitudSku CrearBaseEtapa1(string clasificacion = "Reorden", string? temporada = null)
        {
            return new SolicitudSku
            {
                Motivo = "CREACIÓN",
                CategoriaMotivo = "Producto nuevo",
                ProductoSustituto = "Producto No sustituto",
                PromedioVentaMensual = 50,
                Material = "Plástico",
                Marca = "PM Home",
                Clasificacion = clasificacion,
                Temporada = temporada,
                ProductoPlanificado = clasificacion == "Stocks" ? "No planificado" : "Planificado",
                Departamento = "ABARROTES",
                Categoria = "ACEITES Y GRASAS",
                Subcategoria = "ACEITES EN SPRAY",
                Tipo = "ACEITE DE CANOLA EN SPRAY",
                Estado = "Pendiente Etapa 2",
                CreadoPor = "Usuario 1",
                FechaCreacion = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task CrearEtapa1_ClasificacionNoStocks_AsignaPlanificado()
        {
            using var context = CrearContexto(nameof(CrearEtapa1_ClasificacionNoStocks_AsignaPlanificado));
            var controller = CrearControlador(context);

            var modelo = new SolicitudSku
            {
                Motivo = "CREACIÓN",
                CategoriaMotivo = "Producto nuevo",
                ProductoSustituto = "Producto No sustituto",
                PromedioVentaMensual = 80,
                Material = "Madera",
                Marca = "Genérica",
                Clasificacion = "Producción",
                Departamento = "BEBÉ",
                Categoria = "ALIMENTOS INFANTILES",
                Subcategoria = "CEREALES INFANTILES",
                Tipo = "CEREAL DE ARROZ INFANTIL"
            };

            var resultado = await controller.CrearEtapa1(modelo);

            Assert.IsType<RedirectToActionResult>(resultado);
            var guardado = await context.SolicitudesSku.SingleAsync();
            Assert.Equal("Planificado", guardado.ProductoPlanificado);
            Assert.Equal("Pendiente Etapa 2", guardado.Estado);
        }

        [Fact]
        public async Task CrearEtapa1_ClasificacionTemporada_ConservaTemporadaValida()
        {
            using var context = CrearContexto(nameof(CrearEtapa1_ClasificacionTemporada_ConservaTemporadaValida));
            var controller = CrearControlador(context);

            var modelo = new SolicitudSku
            {
                Motivo = "CREACIÓN",
                CategoriaMotivo = "Producto nuevo",
                ProductoSustituto = "Producto No sustituto",
                PromedioVentaMensual = 200,
                Material = "Tela",
                Marca = "PM Home",
                Clasificacion = "Temporada",
                Temporada = "Navideña",
                Departamento = "BEBÉ",
                Categoria = "BAÑO Y CUIDADO DEL BEBÉ",
                Subcategoria = "JABONES INFANTILES",
                Tipo = "JABÓN LÍQUIDO INFANTIL"
            };

            var resultado = await controller.CrearEtapa1(modelo);

            Assert.IsType<RedirectToActionResult>(resultado);
            var guardado = await context.SolicitudesSku.SingleAsync();
            Assert.Equal("Temporada", guardado.Clasificacion);
            Assert.Equal("Navideña", guardado.Temporada);
        }

        [Fact]
        public async Task CrearEtapa1_ClasificacionStocks_LimpiaTemporadaYMarcaNoPlanificado()
        {
            using var context = CrearContexto(nameof(CrearEtapa1_ClasificacionStocks_LimpiaTemporadaYMarcaNoPlanificado));
            var controller = CrearControlador(context);

            var modelo = new SolicitudSku
            {
                Motivo = "REACTIVACIÓN",
                CategoriaMotivo = "Reactivación por disponibilidad",
                ProductoSustituto = "Producto Sustituto",
                PrecioSugeridoSustituto = 12.5m,
                SkuReferencia = "SKU-100",
                FactorEscalada = 1.2m,
                Material = "Vidrio",
                Marca = "Igloo",
                Clasificacion = "Stocks",
                Temporada = "Verano",
                Departamento = "ABARROTES",
                Categoria = "ACEITES Y GRASAS",
                Subcategoria = "ACEITES LÍQUIDOS",
                Tipo = "ACEITE DE AGUACATE"
            };

            var resultado = await controller.CrearEtapa1(modelo);

            Assert.IsType<RedirectToActionResult>(resultado);
            var guardado = await context.SolicitudesSku.SingleAsync();
            Assert.Equal("No planificado", guardado.ProductoPlanificado);
            Assert.Null(guardado.Temporada);
        }

        [Fact]
        public async Task CrearEtapa1_ProductoSustituto_LimpiaPromedioVentaMensual()
        {
            using var context = CrearContexto(nameof(CrearEtapa1_ProductoSustituto_LimpiaPromedioVentaMensual));
            var controller = CrearControlador(context);

            var modelo = new SolicitudSku
            {
                Motivo = "CREACIÓN",
                CategoriaMotivo = "Producto sustituto",
                ProductoSustituto = "Producto Sustituto",
                PromedioVentaMensual = 999, // No aplica para sustituto
                PrecioSugeridoSustituto = 18.75m,
                SkuReferencia = "SKU-BASE-01",
                FactorEscalada = 0.85m,
                Material = "Metal",
                Marca = "Igloo",
                Clasificacion = "Reorden",
                Departamento = "ABARROTES",
                Categoria = "ACEITES Y GRASAS",
                Subcategoria = "ACEITES LÍQUIDOS",
                Tipo = "ACEITE DE COCO"
            };

            var resultado = await controller.CrearEtapa1(modelo);

            Assert.IsType<RedirectToActionResult>(resultado);
            var guardado = await context.SolicitudesSku.SingleAsync();
            Assert.Null(guardado.PromedioVentaMensual);
            Assert.Equal(18.75m, guardado.PrecioSugeridoSustituto);
            Assert.Equal("SKU-BASE-01", guardado.SkuReferencia);
            Assert.Equal(0.85m, guardado.FactorEscalada);
        }

        [Fact]
        public async Task CrearEtapa1_ProductoNoSustituto_LimpiaCamposDeSustitucion()
        {
            using var context = CrearContexto(nameof(CrearEtapa1_ProductoNoSustituto_LimpiaCamposDeSustitucion));
            var controller = CrearControlador(context);

            var modelo = new SolicitudSku
            {
                Motivo = "CREACIÓN",
                CategoriaMotivo = "Producto nuevo",
                ProductoSustituto = "Producto No sustituto",
                PromedioVentaMensual = 45,
                PrecioSugeridoSustituto = 25m, // No aplica
                SkuReferencia = "SKU-SOBRANTE", // No aplica
                FactorEscalada = 1.5m, // No aplica
                Material = "Plástico",
                Marca = "PM Home",
                Clasificacion = "Reorden",
                Departamento = "ABARROTES",
                Categoria = "ACEITES Y GRASAS",
                Subcategoria = "ACEITES EN SPRAY",
                Tipo = "ACEITE DE CANOLA EN SPRAY"
            };

            var resultado = await controller.CrearEtapa1(modelo);

            Assert.IsType<RedirectToActionResult>(resultado);
            var guardado = await context.SolicitudesSku.SingleAsync();
            Assert.Equal(45, guardado.PromedioVentaMensual);
            Assert.Null(guardado.PrecioSugeridoSustituto);
            Assert.Null(guardado.SkuReferencia);
            Assert.Null(guardado.FactorEscalada);
        }

        [Fact]
        public async Task CompletarEtapa2_IdInexistente_RetornaNotFound()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_IdInexistente_RetornaNotFound));
            var controller = CrearControlador(context);

            var entrada = new SolicitudSku
            {
                Id = 9999,
                PaisOrigen = "Costa Rica",
                RequiereRegistroSanitario = "NO",
                Embalaje = "Master Carton",
                CantidadCajas = 1,
                Caja1ProfundoCm = 10,
                Caja1AnchoCm = 10,
                Caja1AltoCm = 10,
                Caja1PesoKg = 1
            };

            var resultado = await controller.CompletarEtapa2(9999, entrada);

            Assert.IsType<NotFoundResult>(resultado);
        }

        [Fact]
        public async Task CompletarEtapa2_DimensionNegativa_RechazaYNoCompleta()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_DimensionNegativa_RechazaYNoCompleta));
            var controller = CrearControlador(context);

            var etapa1 = CrearBaseEtapa1();
            context.SolicitudesSku.Add(etapa1);
            await context.SaveChangesAsync();

            var entrada = new SolicitudSku
            {
                Id = etapa1.Id,
                PaisOrigen = "México",
                RequiereRegistroSanitario = "NO",
                Embalaje = "Master Carton",
                CantidadCajas = 1,
                Caja1ProfundoCm = -5, // Inválido
                Caja1AnchoCm = 20,
                Caja1AltoCm = 20,
                Caja1PesoKg = 2
            };

            var resultado = await controller.CompletarEtapa2(etapa1.Id, entrada);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);

            var enDb = await context.SolicitudesSku.FindAsync(etapa1.Id);
            Assert.Equal("Pendiente Etapa 2", enDb!.Estado);
        }

        [Fact]
        public async Task CompletarEtapa2_PesoEnCero_RechazaYNoCompleta()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_PesoEnCero_RechazaYNoCompleta));
            var controller = CrearControlador(context);

            var etapa1 = CrearBaseEtapa1();
            context.SolicitudesSku.Add(etapa1);
            await context.SaveChangesAsync();

            var entrada = new SolicitudSku
            {
                Id = etapa1.Id,
                PaisOrigen = "Guatemala",
                RequiereRegistroSanitario = "NO",
                Embalaje = "Master Carton",
                CantidadCajas = 1,
                Caja1ProfundoCm = 12,
                Caja1AnchoCm = 12,
                Caja1AltoCm = 12,
                Caja1PesoKg = 0 // Inválido: debe ser > 0
            };

            var resultado = await controller.CompletarEtapa2(etapa1.Id, entrada);

            Assert.IsType<ViewResult>(resultado);
            Assert.False(controller.ModelState.IsValid);

            var enDb = await context.SolicitudesSku.FindAsync(etapa1.Id);
            Assert.Equal("Pendiente Etapa 2", enDb!.Estado);
        }

        [Fact]
        public async Task CompletarEtapa2_CantidadCajas1_LimpiaCajas2Y3()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_CantidadCajas1_LimpiaCajas2Y3));
            var controller = CrearControlador(context);

            var etapa1 = CrearBaseEtapa1();
            context.SolicitudesSku.Add(etapa1);
            await context.SaveChangesAsync();

            var entrada = new SolicitudSku
            {
                Id = etapa1.Id,
                PaisOrigen = "China",
                RequiereRegistroSanitario = "NO",
                Embalaje = "Nested / Anidado",
                CantidadCajas = 1,
                Caja1ProfundoCm = 30,
                Caja1AnchoCm = 20,
                Caja1AltoCm = 15,
                Caja1PesoKg = 4,
                // Valores residuales en cajas 2 y 3 que no deben persistir
                Caja2ProfundoCm = 50,
                Caja2AnchoCm = 50,
                Caja2AltoCm = 50,
                Caja2PesoKg = 10,
                Caja3ProfundoCm = 60,
                Caja3AnchoCm = 60,
                Caja3AltoCm = 60,
                Caja3PesoKg = 12
            };

            var resultado = await controller.CompletarEtapa2(etapa1.Id, entrada);

            Assert.IsType<RedirectToActionResult>(resultado);
            var final = await context.SolicitudesSku.FindAsync(etapa1.Id);
            Assert.Equal("Completado", final!.Estado);
            Assert.Equal(1, final.CantidadCajas);
            Assert.Null(final.Caja2ProfundoCm);
            Assert.Null(final.Caja2AnchoCm);
            Assert.Null(final.Caja2AltoCm);
            Assert.Null(final.Caja2PesoKg);
            Assert.Null(final.Caja3ProfundoCm);
            Assert.Null(final.Caja3AnchoCm);
            Assert.Null(final.Caja3AltoCm);
            Assert.Null(final.Caja3PesoKg);
        }

        [Fact]
        public async Task CompletarEtapa2_EmbalajeNoApilado_LimpiaDatosDeApilado()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_EmbalajeNoApilado_LimpiaDatosDeApilado));
            var controller = CrearControlador(context);

            var etapa1 = CrearBaseEtapa1();
            context.SolicitudesSku.Add(etapa1);
            await context.SaveChangesAsync();

            var entrada = new SolicitudSku
            {
                Id = etapa1.Id,
                PaisOrigen = "Estados Unidos",
                RequiereRegistroSanitario = "NO",
                Embalaje = "Master Carton", // No es Stacked / Apilado
                AltoApilado = 150, // No debe quedar guardado
                UnidadesPorApilado = 30, // No debe quedar guardado
                CantidadCajas = 1,
                Caja1ProfundoCm = 25,
                Caja1AnchoCm = 25,
                Caja1AltoCm = 25,
                Caja1PesoKg = 5
            };

            var resultado = await controller.CompletarEtapa2(etapa1.Id, entrada);

            Assert.IsType<RedirectToActionResult>(resultado);
            var final = await context.SolicitudesSku.FindAsync(etapa1.Id);
            Assert.Equal("Completado", final!.Estado);
            Assert.Null(final.AltoApilado);
            Assert.Null(final.UnidadesPorApilado);
        }

        [Fact]
        public async Task CompletarEtapa2_RegistroSanitarioMismaFechaEmisionYVencimiento_EsValido()
        {
            using var context = CrearContexto(nameof(CompletarEtapa2_RegistroSanitarioMismaFechaEmisionYVencimiento_EsValido));
            var controller = CrearControlador(context);

            var etapa1 = CrearBaseEtapa1();
            context.SolicitudesSku.Add(etapa1);
            await context.SaveChangesAsync();

            var fecha = new DateTime(2026, 11, 19);
            var entrada = new SolicitudSku
            {
                Id = etapa1.Id,
                PaisOrigen = "Costa Rica",
                RequiereRegistroSanitario = "YES/SI",
                NumeroRegistroSanitario = "RS-CR-2026-01",
                FechaEmisionRegistro = fecha,
                FechaVencimientoRegistro = fecha, // Regla: posterior o igual a emisión
                Embalaje = "Master Carton",
                CantidadCajas = 1,
                Caja1ProfundoCm = 10,
                Caja1AnchoCm = 15,
                Caja1AltoCm = 20,
                Caja1PesoKg = 2.5m
            };

            var resultado = await controller.CompletarEtapa2(etapa1.Id, entrada);

            Assert.IsType<RedirectToActionResult>(resultado);
            var final = await context.SolicitudesSku.FindAsync(etapa1.Id);
            Assert.Equal("Completado", final!.Estado);
            Assert.Equal("RS-CR-2026-01", final.NumeroRegistroSanitario);
        }

        [Fact]
        public async Task ObtenerCategoriasMotivo_MotivoCreacion_RetornaCategoriasEsperadas()
        {
            using var context = CrearContexto(nameof(ObtenerCategoriasMotivo_MotivoCreacion_RetornaCategoriasEsperadas));
            var controller = CrearControlador(context);

            var resultado = await controller.ObtenerCategoriasMotivo("CREACIÓN");

            var json = Assert.IsType<JsonResult>(resultado);
            Assert.NotNull(json.Value);
        }

        private sealed class FakeTempDataProvider : ITempDataProvider
        {
            public IDictionary<string, object?> LoadTempData(HttpContext context) => new Dictionary<string, object?>();
            public void SaveTempData(HttpContext context, IDictionary<string, object?> values) { }
        }
    }
}