using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("reporte")]
    public class Reporte : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdReporte { get; set; }

        [Required(ErrorMessage = "El tipo de reporte es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El tipo de reporte debe tener entre 3 y 100 caracteres.")]
        [RegularExpression(@"^(Ventas|Propiedades|Usuarios|Clientes|Financiero|Postulaciones|Agenda|Inventario|Mensual|Anual|Trimestral|Semanal|Diario|Personalizado)$",
            ErrorMessage = "El tipo de reporte debe ser uno de los valores predefinidos.")]
        public string TipoReporte { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de generación es obligatoria.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El administrador es obligatorio.")]
        [ForeignKey("Administrador")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdAdministrador debe ser un número válido mayor que cero.")]
        public int IdAdministrador { get; set; }

        [StringLength(255, MinimumLength = 5, ErrorMessage = "La ruta del archivo debe tener entre 5 y 255 caracteres.")]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(pdf|xlsx|xls|csv|docx|doc|txt))$",
            ErrorMessage = "El formato del archivo no es válido. Solo se permiten PDF, XLSX, XLS, CSV, DOCX, DOC o TXT.")]
        public string? Archivo { get; set; }

        [InverseProperty("Reportes")]
        public virtual Administrador? Administrador { get; set; }

        [NotMapped]
        public string EstadoReporte
        {
            get
            {
                var minutosDesdeGeneracion = (DateTime.UtcNow - FechaGeneracion).TotalMinutes;
                if (minutosDesdeGeneracion <= 5) return "Generando";
                if (minutosDesdeGeneracion <= 60) return "Reciente";
                if (minutosDesdeGeneracion <= 1440) return "Disponible";
                if (minutosDesdeGeneracion <= 43200) return "Antiguo";
                return "Archivado";
            }
        }

        [NotMapped]
        public bool TieneArchivo => !string.IsNullOrWhiteSpace(Archivo);

        [NotMapped]
        public string ExtensionArchivo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Archivo))
                    return string.Empty;

                return Path.GetExtension(Archivo)?.ToLower() ?? string.Empty;
            }
        }

        [NotMapped]
        public string CategoriaReporte
        {
            get
            {
                var reportesPeriodicos = new[] { "Mensual", "Anual", "Trimestral", "Semanal", "Diario" };
                var reportesOperativos = new[] { "Ventas", "Propiedades", "Usuarios", "Clientes", "Postulaciones", "Agenda", "Inventario" };

                if (reportesPeriodicos.Contains(TipoReporte))
                    return "Periódico";
                if (reportesOperativos.Contains(TipoReporte))
                    return "Operativo";
                if (TipoReporte == "Financiero")
                    return "Financiero";
                if (TipoReporte == "Personalizado")
                    return "Personalizado";

                return "Otro";
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (FechaGeneracion > DateTime.UtcNow.AddMinutes(1))
            {
                results.Add(new ValidationResult(
                    "La fecha de generación no puede ser futura.",
                    new[] { nameof(FechaGeneracion) }
                ));
            }

            if (FechaGeneracion < DateTime.UtcNow.AddYears(-5))
            {
                results.Add(new ValidationResult(
                    "La fecha de generación no puede ser mayor a 5 años atrás.",
                    new[] { nameof(FechaGeneracion) }
                ));
            }

            if (!string.IsNullOrWhiteSpace(TipoReporte))
            {
                if (TipoReporte.Any(char.IsDigit))
                {
                    results.Add(new ValidationResult(
                        "El tipo de reporte no debe contener números.",
                        new[] { nameof(TipoReporte) }
                    ));
                }

                if (TipoReporte != TipoReporte.Trim())
                {
                    results.Add(new ValidationResult(
                        "El tipo de reporte no debe tener espacios al inicio o final.",
                        new[] { nameof(TipoReporte) }
                    ));
                }

                if (Regex.IsMatch(TipoReporte, @"\s{2,}"))
                {
                    results.Add(new ValidationResult(
                        "El tipo de reporte no debe tener espacios múltiples.",
                        new[] { nameof(TipoReporte) }
                    ));
                }
            }

            if (!string.IsNullOrEmpty(Archivo))
            {
                var extension = Path.GetExtension(Archivo)?.ToLower();
                var extensionesValidas = new[] { ".pdf", ".xlsx", ".xls", ".csv", ".docx", ".doc", ".txt" };

                if (!extensionesValidas.Contains(extension))
                {
                    results.Add(new ValidationResult(
                        "El archivo debe ser de tipo PDF, XLSX, XLS, CSV, DOCX, DOC o TXT.",
                        new[] { nameof(Archivo) }
                    ));
                }

                if (Regex.IsMatch(Archivo, @"[<>:""|?*]"))
                {
                    results.Add(new ValidationResult(
                        "El nombre del archivo contiene caracteres no permitidos.",
                        new[] { nameof(Archivo) }
                    ));
                }

                var nombreSinExtension = Path.GetFileNameWithoutExtension(Archivo);
                if (!string.IsNullOrEmpty(nombreSinExtension) && nombreSinExtension.Contains('.'))
                {
                    results.Add(new ValidationResult(
                        "El archivo no debe tener doble extensión por seguridad.",
                        new[] { nameof(Archivo) }
                    ));
                }

                if (nombreSinExtension != null && nombreSinExtension.Length < 3)
                {
                    results.Add(new ValidationResult(
                        "El nombre del archivo debe tener al menos 3 caracteres.",
                        new[] { nameof(Archivo) }
                    ));
                }

                if (Regex.IsMatch(Archivo, @"\.{2,}"))
                {
                    results.Add(new ValidationResult(
                        "El archivo no debe contener puntos múltiples consecutivos.",
                        new[] { nameof(Archivo) }
                    ));
                }
            }

            var tiposReporteValidos = new[]
            {
                "Ventas", "Propiedades", "Usuarios", "Clientes", "Financiero",
                "Postulaciones", "Agenda", "Inventario", "Mensual", "Anual",
                "Trimestral", "Semanal", "Diario", "Personalizado"
            };

            if (!tiposReporteValidos.Contains(TipoReporte))
            {
                results.Add(new ValidationResult(
                    $"El tipo de reporte '{TipoReporte}' no es válido. Debe ser uno de: {string.Join(", ", tiposReporteValidos)}.",
                    new[] { nameof(TipoReporte) }
                ));
            }

            return results;
        }

        public void ValidarDatos()
        {
            if (IdAdministrador <= 0)
                throw new ArgumentException("El IdAdministrador debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(TipoReporte))
                throw new ArgumentException("El tipo de reporte no puede estar vacío o contener solo espacios.");

            if (TipoReporte.Length < 3 || TipoReporte.Length > 100)
                throw new ArgumentException("El tipo de reporte debe tener entre 3 y 100 caracteres.");

            var tiposReporteValidos = new[]
            {
                "Ventas", "Propiedades", "Usuarios", "Clientes", "Financiero",
                "Postulaciones", "Agenda", "Inventario", "Mensual", "Anual",
                "Trimestral", "Semanal", "Diario", "Personalizado"
            };

            if (!tiposReporteValidos.Contains(TipoReporte))
                throw new ArgumentException($"El tipo de reporte '{TipoReporte}' no es válido. Debe ser uno de: {string.Join(", ", tiposReporteValidos)}.");

            if (TipoReporte.Any(char.IsDigit))
                throw new ArgumentException("El tipo de reporte no debe contener números.");

            if (TipoReporte != TipoReporte.Trim())
                throw new ArgumentException("El tipo de reporte no debe tener espacios al inicio o final.");

            if (Regex.IsMatch(TipoReporte, @"\s{2,}"))
                throw new ArgumentException("El tipo de reporte no debe tener espacios múltiples.");

            if (FechaGeneracion > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de generación no puede ser futura.");

            if (FechaGeneracion < DateTime.UtcNow.AddYears(-5))
                throw new ArgumentException("La fecha de generación no puede ser mayor a 5 años atrás.");

            if (FechaGeneracion == default(DateTime))
                throw new ArgumentException("Debe especificar una fecha de generación válida.");

            if (!string.IsNullOrEmpty(Archivo))
            {
                if (Archivo.Length < 5 || Archivo.Length > 255)
                    throw new ArgumentException("La ruta del archivo debe tener entre 5 y 255 caracteres.");

                var extension = Path.GetExtension(Archivo)?.ToLower();
                var extensionesValidas = new[] { ".pdf", ".xlsx", ".xls", ".csv", ".docx", ".doc", ".txt" };

                if (string.IsNullOrEmpty(extension) || !extensionesValidas.Contains(extension))
                    throw new ArgumentException("El archivo debe ser de tipo PDF, XLSX, XLS, CSV, DOCX, DOC o TXT.");

                if (!Regex.IsMatch(Archivo, @"^[\w,\s\-\/\\]+(\.(pdf|xlsx|xls|csv|docx|doc|txt))$", RegexOptions.IgnoreCase))
                    throw new ArgumentException("El formato de ruta del archivo no es válido.");

                if (Regex.IsMatch(Archivo, @"[<>:""|?*]"))
                    throw new ArgumentException("El nombre del archivo contiene caracteres no permitidos.");

                var nombreSinExtension = Path.GetFileNameWithoutExtension(Archivo);
                if (!string.IsNullOrEmpty(nombreSinExtension) && nombreSinExtension.Contains('.'))
                    throw new ArgumentException("El archivo no debe tener doble extensión por seguridad.");

                if (nombreSinExtension != null && nombreSinExtension.Length < 3)
                    throw new ArgumentException("El nombre del archivo debe tener al menos 3 caracteres.");

                if (Regex.IsMatch(Archivo, @"\.{2,}"))
                    throw new ArgumentException("El archivo no debe contener puntos múltiples consecutivos.");
            }
        }

        public bool TieneArchivoValido()
        {
            if (string.IsNullOrWhiteSpace(Archivo))
                return false;

            var extension = Path.GetExtension(Archivo)?.ToLower();
            var extensionesValidas = new[] { ".pdf", ".xlsx", ".xls", ".csv", ".docx", ".doc", ".txt" };

            return !string.IsNullOrEmpty(extension) && extensionesValidas.Contains(extension);
        }

        public static int TamanoMaximoArchivo => 50;

        public bool EsReporteReciente()
        {
            return (DateTime.UtcNow - FechaGeneracion).TotalHours <= 24;
        }

        public bool EsReportePeriodico()
        {
            var reportesPeriodicos = new[] { "Mensual", "Anual", "Trimestral", "Semanal", "Diario" };
            return reportesPeriodicos.Contains(TipoReporte);
        }

        public bool EsReporteFinanciero()
        {
            return TipoReporte == "Financiero";
        }

        public int HorasDesdeGeneracion()
        {
            return (int)(DateTime.UtcNow - FechaGeneracion).TotalHours;
        }

        public int DiasDesdeGeneracion()
        {
            return (DateTime.UtcNow - FechaGeneracion).Days;
        }

        public bool DebeArchivar()
        {
            return DiasDesdeGeneracion() > 30;
        }

        public string ObtenerNombreArchivo()
        {
            if (string.IsNullOrWhiteSpace(Archivo))
                return string.Empty;

            return Path.GetFileName(Archivo);
        }

        public string GenerarNombreArchivoSugerido()
        {
            var timestamp = FechaGeneracion.ToString("yyyyMMdd_HHmmss");
            var extension = TipoReporte == "Financiero" ? "xlsx" : "pdf";
            return $"Reporte_{TipoReporte}_{timestamp}.{extension}";
        }

        public void LimpiarTipoReporte()
        {
            if (!string.IsNullOrEmpty(TipoReporte))
            {
                TipoReporte = Regex.Replace(TipoReporte, @"\s+", " ");
                TipoReporte = TipoReporte.Trim();
            }
        }

        public bool RequiereAprobacion()
        {
            return TipoReporte == "Financiero" || TipoReporte == "Anual";
        }

        public string ObtenerNivelAcceso()
        {
            if (TipoReporte == "Financiero") return "Alto";
            if (EsReportePeriodico()) return "Medio";
            return "Básico";
        }
    }
}
