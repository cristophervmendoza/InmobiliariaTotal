using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly ReporteDao _reporteDao;
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "reportes");

        public ReporteController()
        {
            _reporteDao = new ReporteDao();

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        // POST: api/Reporte
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearReporte([FromForm] ReporteCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Datos inválidos",
                    errores = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            string? archivoPath = null;

            if (dto.Archivo != null && dto.Archivo.Length > 0)
            {
                var validacion = ValidarArchivo(dto.Archivo);
                if (!validacion.esValido)
                {
                    return BadRequest(new { exito = false, mensaje = validacion.mensaje });
                }

                try
                {
                    archivoPath = await GuardarArchivoAsync(dto.Archivo, dto.TipoReporte);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar archivo: {ex.Message}" });
                }
            }

            var reporte = new Reporte
            {
                TipoReporte = dto.TipoReporte,
                FechaGeneracion = DateTime.UtcNow,
                IdAdministrador = dto.IdAdministrador,
                Archivo = archivoPath
            };

            var validationContext = new ValidationContext(reporte);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(reporte, validationContext, validationResults, true))
            {
                if (!string.IsNullOrEmpty(archivoPath) && System.IO.File.Exists(archivoPath))
                {
                    System.IO.File.Delete(archivoPath);
                }

                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            try
            {
                reporte.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                if (!string.IsNullOrEmpty(archivoPath) && System.IO.File.Exists(archivoPath))
                {
                    System.IO.File.Delete(archivoPath);
                }
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, id) = await _reporteDao.CrearReporteAsync(reporte);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerReportePorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            if (!string.IsNullOrEmpty(archivoPath) && System.IO.File.Exists(archivoPath))
            {
                System.IO.File.Delete(archivoPath);
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerReportePorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, reporte) = await _reporteDao.ObtenerReportePorIdAsync(id);

            if (exito && reporte != null)
            {
                var response = new
                {
                    exito = true,
                    mensaje,
                    data = reporte,
                    estadoReporte = reporte.EstadoReporte,
                    tieneArchivo = reporte.TieneArchivo,
                    extensionArchivo = reporte.ExtensionArchivo,
                    categoriaReporte = reporte.CategoriaReporte,
                    esReciente = reporte.EsReporteReciente(),
                    esPeriodico = reporte.EsReportePeriodico(),
                    esFinanciero = reporte.EsReporteFinanciero(),
                    horasDesdeGeneracion = reporte.HorasDesdeGeneracion(),
                    diasDesdeGeneracion = reporte.DiasDesdeGeneracion(),
                    debeArchivar = reporte.DebeArchivar(),
                    requiereAprobacion = reporte.RequiereAprobacion(),
                    nivelAcceso = reporte.ObtenerNivelAcceso()
                };
                return Ok(response);
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Reporte
        [HttpGet]
        public async Task<IActionResult> ListarReportes()
        {
            var (exito, mensaje, reportes) = await _reporteDao.ListarReportesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarReportesPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, reportes, totalRegistros) =
                await _reporteDao.ListarReportesPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = reportes,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarReportes(
            [FromQuery] string? tipoReporte = null,
            [FromQuery] int? idAdministrador = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] bool? tieneArchivo = null)
        {
            var (exito, mensaje, reportes) =
                await _reporteDao.BuscarReportesAsync(tipoReporte, idAdministrador, fechaInicio, fechaFin, tieneArchivo);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/administrador/{idAdministrador}
        [HttpGet("administrador/{idAdministrador}")]
        public async Task<IActionResult> ObtenerReportesPorAdministrador(int idAdministrador)
        {
            if (idAdministrador <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de administrador inválido" });
            }

            var (exito, mensaje, reportes) =
                await _reporteDao.ObtenerReportesPorAdministradorAsync(idAdministrador);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/tipo/{tipoReporte}
        [HttpGet("tipo/{tipoReporte}")]
        public async Task<IActionResult> ObtenerReportesPorTipo(string tipoReporte)
        {
            if (string.IsNullOrWhiteSpace(tipoReporte))
            {
                return BadRequest(new { exito = false, mensaje = "Tipo de reporte inválido" });
            }

            var (exito, mensaje, reportes) =
                await _reporteDao.ObtenerReportesPorTipoAsync(tipoReporte);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/recientes?horas=24
        [HttpGet("recientes")]
        public async Task<IActionResult> ObtenerReportesRecientes([FromQuery] int horas = 24)
        {
            if (horas <= 0 || horas > 720)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro horas debe estar entre 1 y 720" });
            }

            var (exito, mensaje, reportes) =
                await _reporteDao.ObtenerReportesRecientesAsync(horas);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/rango?fechaInicio=2025-01-01&fechaFin=2025-12-31
        [HttpGet("rango")]
        public async Task<IActionResult> ObtenerReportesPorRango(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                return BadRequest(new { exito = false, mensaje = "La fecha de inicio no puede ser posterior a la fecha de fin" });
            }

            var (exito, mensaje, reportes) =
                await _reporteDao.ObtenerReportesPorRangoFechasAsync(fechaInicio, fechaFin);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/archivar?diasAntiguedad=30
        [HttpGet("archivar")]
        public async Task<IActionResult> ObtenerReportesParaArchivar([FromQuery] int diasAntiguedad = 30)
        {
            if (diasAntiguedad <= 0 || diasAntiguedad > 3650)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 3650" });
            }

            var (exito, mensaje, reportes) =
                await _reporteDao.ObtenerReportesParaArchivarAsync(diasAntiguedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = reportes, total = reportes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _reporteDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Reporte/{id}/descargar
        [HttpGet("{id}/descargar")]
        public async Task<IActionResult> DescargarReporte(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, _, reporte) = await _reporteDao.ObtenerReportePorIdAsync(id);

            if (!exito || reporte == null)
            {
                return NotFound(new { exito = false, mensaje = "Reporte no encontrado" });
            }

            if (string.IsNullOrEmpty(reporte.Archivo) || !System.IO.File.Exists(reporte.Archivo))
            {
                return NotFound(new { exito = false, mensaje = "Archivo no encontrado" });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(reporte.Archivo, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = reporte.ExtensionArchivo switch
            {
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".csv" => "text/csv",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            return File(memory, contentType, Path.GetFileName(reporte.Archivo));
        }

        // PUT: api/Reporte/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ActualizarReporte(int id, [FromForm] ReporteUpdateDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Datos inválidos",
                    errores = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var (existeExito, _, reporteExistente) = await _reporteDao.ObtenerReportePorIdAsync(id);
            if (!existeExito || reporteExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Reporte no encontrado" });
            }

            string? archivoPath = reporteExistente.Archivo;
            string? archivoAnterior = null;

            if (dto.Archivo != null && dto.Archivo.Length > 0)
            {
                var validacion = ValidarArchivo(dto.Archivo);
                if (!validacion.esValido)
                {
                    return BadRequest(new { exito = false, mensaje = validacion.mensaje });
                }

                try
                {
                    archivoAnterior = archivoPath;
                    archivoPath = await GuardarArchivoAsync(dto.Archivo, dto.TipoReporte);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar archivo: {ex.Message}" });
                }
            }

            reporteExistente.TipoReporte = dto.TipoReporte;
            reporteExistente.Archivo = archivoPath;

            try
            {
                reporteExistente.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                if (archivoAnterior != null && !string.IsNullOrEmpty(archivoPath) && System.IO.File.Exists(archivoPath))
                {
                    System.IO.File.Delete(archivoPath);
                }
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _reporteDao.ActualizarReporteAsync(reporteExistente);

            if (exito)
            {
                if (archivoAnterior != null && !string.IsNullOrEmpty(archivoAnterior) && System.IO.File.Exists(archivoAnterior))
                {
                    System.IO.File.Delete(archivoAnterior);
                }
                return Ok(new { exito = true, mensaje, data = reporteExistente });
            }

            if (archivoAnterior != null && !string.IsNullOrEmpty(archivoPath) && System.IO.File.Exists(archivoPath))
            {
                System.IO.File.Delete(archivoPath);
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Reporte/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarReporte(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (existeExito, _, reporte) = await _reporteDao.ObtenerReportePorIdAsync(id);

            var (exito, mensaje) = await _reporteDao.EliminarReporteAsync(id);

            if (exito)
            {
                if (existeExito && reporte != null &&
                    !string.IsNullOrEmpty(reporte.Archivo) &&
                    System.IO.File.Exists(reporte.Archivo))
                {
                    try
                    {
                        System.IO.File.Delete(reporte.Archivo);
                    }
                    catch { }
                }
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // Métodos auxiliares
        private (bool esValido, string mensaje) ValidarArchivo(IFormFile archivo)
        {
            var tamanoMaximoMB = Reporte.TamanoMaximoArchivo;
            var tamanoMaximoBytes = tamanoMaximoMB * 1024 * 1024;
            if (archivo.Length > tamanoMaximoBytes)
            {
                return (false, $"El archivo no debe superar {tamanoMaximoMB} MB");
            }

            if (archivo.Length == 0)
            {
                return (false, "El archivo está vacío");
            }

            var extension = Path.GetExtension(archivo.FileName)?.ToLower();
            var extensionesValidas = new[] { ".pdf", ".xlsx", ".xls", ".csv", ".docx", ".doc", ".txt" };
            if (string.IsNullOrEmpty(extension) || !extensionesValidas.Contains(extension))
            {
                return (false, "Solo se permiten archivos PDF, XLSX, XLS, CSV, DOCX, DOC o TXT");
            }

            return (true, string.Empty);
        }

        private async Task<string> GuardarArchivoAsync(IFormFile archivo, string tipoReporte)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var nombreArchivo = $"Reporte_{tipoReporte}_{timestamp}{Path.GetExtension(archivo.FileName)}";
            var rutaCompleta = Path.Combine(_uploadFolder, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return rutaCompleta;
        }
    }

    // DTOs
    public class ReporteCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^(Ventas|Propiedades|Usuarios|Clientes|Financiero|Postulaciones|Agenda|Inventario|Mensual|Anual|Trimestral|Semanal|Diario|Personalizado)$")]
        public string TipoReporte { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int IdAdministrador { get; set; }

        public IFormFile? Archivo { get; set; }
    }

    public class ReporteUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^(Ventas|Propiedades|Usuarios|Clientes|Financiero|Postulaciones|Agenda|Inventario|Mensual|Anual|Trimestral|Semanal|Diario|Personalizado)$")]
        public string TipoReporte { get; set; } = string.Empty;

        public IFormFile? Archivo { get; set; }
    }
}
