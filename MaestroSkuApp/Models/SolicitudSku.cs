using System.ComponentModel.DataAnnotations;

namespace MaestroSkuApp.Models;

// Tabla única consolidada para todo el flujo de solicitud de SKU (Etapa 1 y Etapa 2)
public class SolicitudSku
{
    // --- Control y trazabilidad ---
    [Key]
    public int Id { get; set; } // Identificador único generado al crear en Etapa 1
    public DateTime FechaCreacion { get; set; } = DateTime.Now; // Fecha/hora de creación
    public string CreadoPor { get; set; } = "Usuario 1"; // Quién inicia la solicitud
    public string Estado { get; set; } = "Pendiente Etapa 2"; // "Pendiente Etapa 2" o "Completado"
    public string? CompletadoPor { get; set; } // Quién finaliza en Etapa 2
    public DateTime? FechaFinalizacion { get; set; } // Fecha/hora al pasar a "Completado"

    // --- Etapa 1: Campos principales y jerarquía ---
    [Required(ErrorMessage = "El Motivo es obligatorio.")]
    public string Motivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Categoría motivo es obligatoria.")]
    public string CategoriaMotivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe indicar si es Producto sustituto o no.")]
    public string ProductoSustituto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Material es obligatorio.")]
    public string Material { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Marca es obligatoria.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Clasificación es obligatoria.")]
    public string Clasificacion { get; set; } = string.Empty;

    public string? Temporada { get; set; } // Aplica solo si Clasificacion == "Temporada"
    public string ProductoPlanificado { get; set; } = string.Empty; // Calculado según clasificación

    [Required(ErrorMessage = "El Departamento es obligatorio.")]
    public string Departamento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Categoría es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "La Subcategoría es obligatoria.")]
    public string Subcategoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Tipo es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;

    // --- Etapa 1: Campos condicionales de sustitución ---
    public decimal? PrecioSugeridoSustituto { get; set; }
    public string? SkuReferencia { get; set; }
    public decimal? FactorEscalada { get; set; }
    public int? PromedioVentaMensual { get; set; }

    // --- Etapa 2: Campos principales y condicionales ---
    public string? PaisOrigen { get; set; }
    public string? RequiereRegistroSanitario { get; set; }

    public string? NumeroRegistroSanitario { get; set; }
    public DateTime? FechaEmisionRegistro { get; set; }
    public DateTime? FechaVencimientoRegistro { get; set; }

    public string? Embalaje { get; set; }
    public decimal? AltoApilado { get; set; }
    public int? UnidadesPorApilado { get; set; }

    public int? CantidadCajas { get; set; } // 1, 2 o 3 cajas

    // --- Etapa 2: Medidas (cm) y peso (kg) por caja ---
    // Caja 1
    public decimal? Caja1ProfundoCm { get; set; }
    public decimal? Caja1AnchoCm { get; set; }
    public decimal? Caja1AltoCm { get; set; }
    public decimal? Caja1PesoKg { get; set; }

    // Caja 2
    public decimal? Caja2ProfundoCm { get; set; }
    public decimal? Caja2AnchoCm { get; set; }
    public decimal? Caja2AltoCm { get; set; }
    public decimal? Caja2PesoKg { get; set; }

    // Caja 3
    public decimal? Caja3ProfundoCm { get; set; }
    public decimal? Caja3AnchoCm { get; set; }
    public decimal? Caja3AltoCm { get; set; }
    public decimal? Caja3PesoKg { get; set; }
}