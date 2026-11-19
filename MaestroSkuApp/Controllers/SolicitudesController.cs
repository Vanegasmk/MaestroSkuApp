using MaestroSkuApp.Data;
using MaestroSkuApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaestroSkuApp.Controllers
{
    public class SolicitudesController : Controller
    {
        private readonly AppDbContext _context;

        public SolicitudesController(AppDbContext context)
        {
            _context = context;
        }

        // BASE CONSOLIDADA (Pantalla principal: muestra pendientes y completados)

        public async Task<IActionResult> Index(string? estadoFiltro)
        {
            var query = _context.SolicitudesSku.AsQueryable();

            if(!string.IsNullOrEmpty(estadoFiltro))
            {
                query = query.Where(s => s.Estado == estadoFiltro);
            }

            // Ordena los más recientes primero en la tabla consolidada
            var lista = await query.OrderByDescending(s => s.FechaCreacion).ToListAsync();

            ViewBag.estadoFiltro = estadoFiltro ?? "";

            return View(lista);
        }

        // ETAPA 1 - MOSTRAR FORMULARIO DE CREACIÓN

        public IActionResult CrearEtapa1()
        {
            CargarCatalogosEnViewBag();

            var modelo = new SolicitudSku
            {
                CreadoPor = "Usuario 1",
                Estado = "Pendiente Etapa 2",
            };

            return View(modelo);
        }

        // ETAPA 1 - GUARDAR NUEVA SOLICITUD EN SQLITE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearEtapa1(SolicitudSku solicitud)
        {
            // Aplica reglas automáticas y limpia campos condicionales que no correspondan
            AplicarReglasEtapa1(solicitud);
            ValidarReglasEtapa1(solicitud);

            if (!ModelState.IsValid)
            {
                CargarCatalogosEnViewBag(solicitud);
                return View(solicitud);
            }

            // Inicializa campos de control y trazabilidad del registro nuevo
            solicitud.FechaCreacion = DateTime.Now;
            solicitud.CreadoPor = string.IsNullOrWhiteSpace(solicitud.CreadoPor) ? "Usuario 1" : solicitud.CreadoPor;
            solicitud.Estado = "Pendiente Etapa 2";
            solicitud.CompletadoPor = null;
            solicitud.FechaFinalizacion = null;

            // Asegura que los campos exclusivos de Etapa 2 queden limpios al nacer en Etapa 1
            LimpiarCamposEtapa2(solicitud);

            _context.SolicitudesSku.Add(solicitud);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Solicitud #{solicitud.Id} creada en Etapa 1 con estado 'Pendiente Etapa 2'.";
            return RedirectToAction(nameof(Index));
        }

        // ETAPA 2 - CARGAR SOLICITUD EXISTENTE POR ID PARA COMPLETARLA

        public async Task<IActionResult> CompletarEtapa2(int id)
        {
            var solicitud = await _context.SolicitudesSku.FindAsync(id);

            if(solicitud == null)
            {
                return NotFound();
            }

            CargarCatalogosEnViewBag(solicitud);

            if(string.IsNullOrWhiteSpace(solicitud.CompletadoPor))
            {
                solicitud.CompletadoPor = "Usuario 2"; // Valor predeterminado para la prueba
            }
            
            return View(solicitud);
        }

        // ETAPA 2 - ACTUALIZAR EL MISMO REGISTRO (MISMO ID) A "COMPLETADO"

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CompletarEtapa2(int id, SolicitudSku entrada)
        {
            if (id != entrada.Id)
            {
                return BadRequest();
            }

            // Busca el registro original en SQLite para actualizarlo sin duplicar filas
            var solicitudDb = await _context.SolicitudesSku.FindAsync(id);
            if(solicitudDb == null)
            {
                return NotFound();
            }

            // Copia los campos principales y condicionales de Etapa 2 desde el formulario
            solicitudDb.PaisOrigen = entrada.PaisOrigen;
            solicitudDb.RequiereRegistroSanitario = entrada.RequiereRegistroSanitario;
            solicitudDb.NumeroRegistroSanitario = entrada.NumeroRegistroSanitario;
            solicitudDb.FechaEmisionRegistro = entrada.FechaEmisionRegistro;
            solicitudDb.FechaVencimientoRegistro = entrada.FechaVencimientoRegistro;

            solicitudDb.Embalaje = entrada.Embalaje;
            solicitudDb.AltoApilado = entrada.AltoApilado;
            solicitudDb.UnidadesPorApilado = entrada.UnidadesPorApilado;

            solicitudDb.CantidadCajas = entrada.CantidadCajas;

            // Caja 1
            solicitudDb.Caja1ProfundoCm = entrada.Caja1ProfundoCm;
            solicitudDb.Caja1AnchoCm = entrada.Caja1AnchoCm;
            solicitudDb.Caja1AltoCm = entrada.Caja1AltoCm;
            solicitudDb.Caja1PesoKg = entrada.Caja1PesoKg;

            // Caja 2
            solicitudDb.Caja2ProfundoCm = entrada.Caja2ProfundoCm;
            solicitudDb.Caja2AnchoCm = entrada.Caja2AnchoCm;
            solicitudDb.Caja2AltoCm = entrada.Caja2AltoCm;
            solicitudDb.Caja2PesoKg = entrada.Caja2PesoKg;

            // Caja 3
            solicitudDb.Caja3ProfundoCm = entrada.Caja3ProfundoCm;
            solicitudDb.Caja3AnchoCm = entrada.Caja3AnchoCm;
            solicitudDb.Caja3AltoCm = entrada.Caja3AltoCm;
            solicitudDb.Caja3PesoKg = entrada.Caja3PesoKg;

            solicitudDb.CompletadoPor = string.IsNullOrWhiteSpace(entrada.CompletadoPor) ? "Usuario 2" : entrada.CompletadoPor.Trim();

            // Limpia campos condicionales de Etapa 2 que no correspondan y valida
            LimpiarCamposEtapa2(solicitudDb);
            ValidarReglasEtapa2(solicitudDb);

            if (!ModelState.IsValid)
            {
                CargarCatalogosEnViewBag(solicitudDb);
                return View(solicitudDb);
            }

            // Marca la solicitud como completada sobre el mismo ID
            solicitudDb.Estado = "Completado";
            solicitudDb.FechaFinalizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Solicitud #{solicitudDb.Id} actualizada y marcada como 'Completado'.";
            return RedirectToAction(nameof(Index));
        }

        // ENDPOINTS JSON PARA FILTROS EN CASCADA (MOTIVO Y JERARQUÍA)

        [HttpGet]
        public async Task<IActionResult> ObtenerCategoriasMotivo(string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return Json(Array.Empty<string>());

            var categorias = await _context.CatalogoMotivos
                .Where(m => m.Motivo == motivo)
                .Select(m => m.CategoriaMotivo)
                .Distinct()
                .ToListAsync();

            return Json(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSubcategoriasJerarquia(string departamento, string categoria)
        {
            if (string.IsNullOrWhiteSpace(departamento) || string.IsNullOrWhiteSpace(categoria))
                return Json(Array.Empty<string>());

            var subcategorias = await _context.CatalogoJerarquias
                .Where(j => j.Departamento == departamento && j.Categoria == categoria)
                .Select(j => j.Subcategoria)
                .Distinct()
                .ToListAsync();

            return Json(subcategorias);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposJerarquia(string departamento, string categoria, string subcategoria)
        {
            if (string.IsNullOrWhiteSpace(departamento) || string.IsNullOrWhiteSpace(categoria) || string.IsNullOrWhiteSpace(subcategoria))
                return Json(Array.Empty<string>());

            var tipos = await _context.CatalogoJerarquias
                .Where(j => j.Departamento == departamento && j.Categoria == categoria && j.Subcategoria == subcategoria)
                .Select(j => j.Tipo)
                .Distinct()
                .ToListAsync();

            return Json(tipos);
        }

        //REGLAS DE NEGOCIO, LIMPIEZA Y VALIDACIONES EN SERVIDOR

        private void AplicarReglasEtapa1(SolicitudSku s)
        {
            // Cálculo automático de ProductoPlanificado según Clasificación
            s.ProductoPlanificado = string.Equals(s.Clasificacion, "Stocks", StringComparison.OrdinalIgnoreCase)
                ? "No planificado"
                : "Planificado";

            // Temporada solo aplica si Clasificación es "Temporada"
            if (!string.Equals(s.Clasificacion, "Temporada", StringComparison.OrdinalIgnoreCase))
            {
                s.Temporada = null;
            }

            // Limpieza según ProductoSustituto
            if (string.Equals(s.ProductoSustituto, "Producto Sustituto", StringComparison.OrdinalIgnoreCase))
            {
                s.PromedioVentaMensual = null;
                s.SkuReferencia = s.SkuReferencia?.Trim();
            }
            else if (string.Equals(s.ProductoSustituto, "Producto No sustituto", StringComparison.OrdinalIgnoreCase))
            {
                s.PrecioSugeridoSustituto = null;
                s.SkuReferencia = null;
                s.FactorEscalada = null;
            }
            else
            {
                s.PrecioSugeridoSustituto = null;
                s.SkuReferencia = null;
                s.FactorEscalada = null;
                s.PromedioVentaMensual = null;
            }
        }

        private void ValidarReglasEtapa1(SolicitudSku s)
        {
            if (string.Equals(s.Clasificacion, "Temporada", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(s.Temporada))
            {
                ModelState.AddModelError(nameof(s.Temporada), "La Temporada es obligatoria cuando la Clasificación es 'Temporada'.");
            }

            if (string.Equals(s.ProductoSustituto, "Producto Sustituto", StringComparison.OrdinalIgnoreCase))
            {
                if (!s.PrecioSugeridoSustituto.HasValue || s.PrecioSugeridoSustituto.Value <= 0)
                    ModelState.AddModelError(nameof(s.PrecioSugeridoSustituto), "El Precio sugerido sustituto debe ser mayor a 0.");

                if (string.IsNullOrWhiteSpace(s.SkuReferencia))
                    ModelState.AddModelError(nameof(s.SkuReferencia), "El SKU de referencia es obligatorio para producto sustituto.");

                if (!s.FactorEscalada.HasValue || s.FactorEscalada.Value <= 0)
                    ModelState.AddModelError(nameof(s.FactorEscalada), "El Factor de escalada debe ser mayor a 0.");
            }
            else if (string.Equals(s.ProductoSustituto, "Producto No sustituto", StringComparison.OrdinalIgnoreCase))
            {
                if (!s.PromedioVentaMensual.HasValue || s.PromedioVentaMensual.Value < 0)
                    ModelState.AddModelError(nameof(s.PromedioVentaMensual), "El Promedio de venta mensual debe ser 0 o mayor.");
            }
        }

        private static void LimpiarCamposEtapa2(SolicitudSku s)
        {
            // Registro sanitario: se limpia si no es "YES/SI"
            if (!string.Equals(s.RequiereRegistroSanitario, "YES/SI", StringComparison.OrdinalIgnoreCase))
            {
                s.NumeroRegistroSanitario = null;
                s.FechaEmisionRegistro = null;
                s.FechaVencimientoRegistro = null;
            }
            else
            {
                s.NumeroRegistroSanitario = s.NumeroRegistroSanitario?.Trim();
            }

            // Apilado: se limpia si el embalaje no es "Stacked / Apilado"
            if (!string.Equals(s.Embalaje, "Stacked / Apilado", StringComparison.OrdinalIgnoreCase))
            {
                s.AltoApilado = null;
                s.UnidadesPorApilado = null;
            }

            // Limpia cajas 2 y/o 3 según CantidadCajas (1 a 3)
            int cajas = s.CantidadCajas ?? 0;
            if (cajas < 2)
            {
                s.Caja2ProfundoCm = null;
                s.Caja2AnchoCm = null;
                s.Caja2AltoCm = null;
                s.Caja2PesoKg = null;
            }

            if (cajas < 3)
            {
                s.Caja3ProfundoCm = null;
                s.Caja3AnchoCm = null;
                s.Caja3AltoCm = null;
                s.Caja3PesoKg = null;
            }
        }

        private void ValidarReglasEtapa2(SolicitudSku s)
        {
            if (string.IsNullOrWhiteSpace(s.PaisOrigen))
                ModelState.AddModelError(nameof(s.PaisOrigen), "El País de origen es obligatorio.");

            if (string.IsNullOrWhiteSpace(s.RequiereRegistroSanitario))
                ModelState.AddModelError(nameof(s.RequiereRegistroSanitario), "Debe indicar si requiere Registro Sanitario.");

            if (string.Equals(s.RequiereRegistroSanitario, "YES/SI", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(s.NumeroRegistroSanitario))
                    ModelState.AddModelError(nameof(s.NumeroRegistroSanitario), "El Número de registro sanitario es obligatorio.");

                if (!s.FechaEmisionRegistro.HasValue)
                    ModelState.AddModelError(nameof(s.FechaEmisionRegistro), "La Fecha de emisión es obligatoria.");

                if (!s.FechaVencimientoRegistro.HasValue)
                    ModelState.AddModelError(nameof(s.FechaVencimientoRegistro), "La Fecha de vencimiento es obligatoria.");

                if (s.FechaEmisionRegistro.HasValue && s.FechaVencimientoRegistro.HasValue &&
                    s.FechaVencimientoRegistro.Value.Date < s.FechaEmisionRegistro.Value.Date)
                {
                    ModelState.AddModelError(nameof(s.FechaVencimientoRegistro), "La Fecha de vencimiento no puede ser anterior a la Fecha de emisión.");
                }
            }

            if (string.IsNullOrWhiteSpace(s.Embalaje))
                ModelState.AddModelError(nameof(s.Embalaje), "El Embalaje es obligatorio.");

            if (string.Equals(s.Embalaje, "Stacked / Apilado", StringComparison.OrdinalIgnoreCase))
            {
                if (!s.AltoApilado.HasValue || s.AltoApilado.Value <= 0)
                    ModelState.AddModelError(nameof(s.AltoApilado), "El Alto apilado debe ser mayor a 0.");

                if (!s.UnidadesPorApilado.HasValue || s.UnidadesPorApilado.Value <= 0)
                    ModelState.AddModelError(nameof(s.UnidadesPorApilado), "Las Unidades por apilado deben ser mayores a 0.");
            }

            if (!s.CantidadCajas.HasValue || s.CantidadCajas.Value < 1 || s.CantidadCajas.Value > 3)
            {
                ModelState.AddModelError(nameof(s.CantidadCajas), "La Cantidad de cajas debe ser entre 1 y 3.");
                return;
            }

            // Validación de Caja 1 (siempre obligatoria y > 0)
            ValidarCaja(1, s.Caja1ProfundoCm, s.Caja1AnchoCm, s.Caja1AltoCm, s.Caja1PesoKg);

            // Validación de Caja 2 (si CantidadCajas >= 2)
            if (s.CantidadCajas.Value >= 2)
            {
                ValidarCaja(2, s.Caja2ProfundoCm, s.Caja2AnchoCm, s.Caja2AltoCm, s.Caja2PesoKg);
            }

            // Validación de Caja 3 (si CantidadCajas == 3)
            if (s.CantidadCajas.Value == 3)
            {
                ValidarCaja(3, s.Caja3ProfundoCm, s.Caja3AnchoCm, s.Caja3AltoCm, s.Caja3PesoKg);
            }
        }

        private void ValidarCaja(int numeroCaja, decimal? profundo, decimal? ancho, decimal? alto, decimal? peso)
        {
            if (!profundo.HasValue || profundo.Value <= 0)
                ModelState.AddModelError($"Caja{numeroCaja}ProfundoCm", $"Caja {numeroCaja}: el Profundo (cm) debe ser mayor a 0.");

            if (!ancho.HasValue || ancho.Value <= 0)
                ModelState.AddModelError($"Caja{numeroCaja}AnchoCm", $"Caja {numeroCaja}: el Ancho (cm) debe ser mayor a 0.");

            if (!alto.HasValue || alto.Value <= 0)
                ModelState.AddModelError($"Caja{numeroCaja}AltoCm", $"Caja {numeroCaja}: el Alto (cm) debe ser mayor a 0.");

            if (!peso.HasValue || peso.Value <= 0)
                ModelState.AddModelError($"Caja{numeroCaja}PesoKg", $"Caja {numeroCaja}: el Peso (kg) debe ser mayor a 0.");
        }


        private void CargarCatalogosEnViewBag(SolicitudSku? actual = null)
        {
            ViewBag.Motivos = CatalogosEstaticos.Motivos;
            ViewBag.ProductoSustitutoOpciones = CatalogosEstaticos.ProductoSustituto;
            ViewBag.Materiales = CatalogosEstaticos.Materiales;
            ViewBag.Marcas = CatalogosEstaticos.Marcas;
            ViewBag.Clasificaciones = CatalogosEstaticos.Clasificaciones;
            ViewBag.Temporadas = CatalogosEstaticos.Temporadas;
            ViewBag.PaisesOrigen = CatalogosEstaticos.PaisesOrigen;
            ViewBag.RegistroSanitarioOpciones = CatalogosEstaticos.RegistroSanitario;
            ViewBag.Embalajes = CatalogosEstaticos.Embalajes;
            ViewBag.CantidadCajasOpciones = CatalogosEstaticos.CantidadCajas;

            ViewBag.Departamentos = _context.CatalogoJerarquias
                .Select(j => j.Departamento)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Si hay una solicitud cargada (por ejemplo, al devolver un formulario con errores), precarga las listas dependientes
            ViewBag.CategoriasMotivo = !string.IsNullOrWhiteSpace(actual?.Motivo)
                ? _context.CatalogoMotivos.Where(m => m.Motivo == actual.Motivo).Select(m => m.CategoriaMotivo).Distinct().ToList()
                : new List<string>();

            ViewBag.CategoriasJerarquia = !string.IsNullOrWhiteSpace(actual?.Departamento)
                ? _context.CatalogoJerarquias.Where(j => j.Departamento == actual.Departamento).Select(j => j.Categoria).Distinct().ToList()
                : new List<string>();

            ViewBag.SubcategoriasJerarquia = (!string.IsNullOrWhiteSpace(actual?.Departamento) && !string.IsNullOrWhiteSpace(actual?.Categoria))
                ? _context.CatalogoJerarquias.Where(j => j.Departamento == actual.Departamento && j.Categoria == actual.Categoria).Select(j => j.Subcategoria).Distinct().ToList()
                : new List<string>();

            ViewBag.TiposJerarquia = (!string.IsNullOrWhiteSpace(actual?.Departamento) && !string.IsNullOrWhiteSpace(actual?.Categoria) && !string.IsNullOrWhiteSpace(actual?.Subcategoria))
                ? _context.CatalogoJerarquias.Where(j => j.Departamento == actual.Departamento && j.Categoria == actual.Categoria && j.Subcategoria == actual.Subcategoria).Select(j => j.Tipo).Distinct().ToList()
                : new List<string>();
        }
    }
}
