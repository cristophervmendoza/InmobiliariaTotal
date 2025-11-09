using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitaHistorialController : ControllerBase
    {
        private readonly CitaHistorialDao _historialDao;

        public CitaHistorialController()
        {
            _historialDao = new CitaHistorialDao();
        }

        // POST: api/CitaHistorial
        [HttpPost]
        public async Task<IActionResult> CrearHistorial([FromBody] CitaHistorialCreateDto dto)
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

            var historial = new CitaHistorial
            {
                IdCita = dto.IdCita,
                CitaEstadoId = dto.CitaEstadoId,
                Observacion = dto.Observacion ?? string.Empty,
                CreadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(historial);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(historial, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _historialDao.CrearHistorialAsync(historial);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerHistorial),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerHistorial(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, historial) = await _historialDao.ObtenerHistorialPorIdAsync(id);

            if (exito && historial != null)
            {
                return Ok(new { exito = true, mensaje, data = historial });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/cita/{idCita}
        [HttpGet("cita/{idCita}")]
        public async Task<IActionResult> ObtenerHistorialPorCita(int idCita)
        {
            if (idCita <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cita inválido" });
            }

            var (exito, mensaje, historiales) = await _historialDao.ObtenerHistorialPorCitaAsync(idCita);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/cita/{idCita}/ultimo
        [HttpGet("cita/{idCita}/ultimo")]
        public async Task<IActionResult> ObtenerUltimoHistorialCita(int idCita)
        {
            if (idCita <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cita inválido" });
            }

            var (exito, mensaje, historial) = await _historialDao.ObtenerUltimoHistorialCitaAsync(idCita);

            if (exito && historial != null)
            {
                return Ok(new { exito = true, mensaje, data = historial });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial
        [HttpGet]
        public async Task<IActionResult> ListarHistoriales()
        {
            var (exito, mensaje, historiales) = await _historialDao.ListarHistorialesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarHistorialesPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, historiales, totalRegistros) =
                await _historialDao.ListarHistorialesPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = historiales,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/estado/{idEstado}
        [HttpGet("estado/{idEstado}")]
        public async Task<IActionResult> BuscarHistorialesPorEstado(int idEstado)
        {
            if (idEstado <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de estado inválido" });
            }

            var (exito, mensaje, historiales) = await _historialDao.BuscarHistorialesPorEstadoAsync(idEstado);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/buscar?fechaInicio=2025-10-01&fechaFin=2025-10-31
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarHistorialesPorFecha(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (exito, mensaje, historiales) =
                await _historialDao.BuscarHistorialesPorFechaAsync(fechaInicio, fechaFin);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/recientes?dias=7
        [HttpGet("recientes")]
        public async Task<IActionResult> ObtenerHistorialesRecientes([FromQuery] int dias = 7)
        {
            if (dias <= 0 || dias > 365)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 365" });
            }

            var (exito, mensaje, historiales) = await _historialDao.ObtenerHistorialesRecientesAsync(dias);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/hoy
        [HttpGet("hoy")]
        public async Task<IActionResult> ObtenerHistorialesDeHoy()
        {
            var (exito, mensaje, historiales) = await _historialDao.ObtenerHistorialesDeHoyAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/con-observaciones
        [HttpGet("con-observaciones")]
        public async Task<IActionResult> ObtenerHistorialesConObservaciones()
        {
            var (exito, mensaje, historiales) = await _historialDao.ObtenerHistorialesConObservacionesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = historiales, total = historiales?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/contar/cita/{idCita}
        [HttpGet("contar/cita/{idCita}")]
        public async Task<IActionResult> ContarCambiosEstadoCita(int idCita)
        {
            if (idCita <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cita inválido" });
            }

            var (exito, mensaje, total) = await _historialDao.ContarCambiosEstadoCitaAsync(idCita);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/verificar/cita/{idCita}
        [HttpGet("verificar/cita/{idCita}")]
        public async Task<IActionResult> VerificarCitaTieneHistorial(int idCita)
        {
            if (idCita <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cita inválido" });
            }

            var tieneHistorial = await _historialDao.CitaTieneHistorialAsync(idCita);

            return Ok(new
            {
                exito = true,
                tieneHistorial,
                mensaje = tieneHistorial ? "La cita tiene historial" : "La cita no tiene historial"
            });
        }

        // GET: api/CitaHistorial/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _historialDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/CitaHistorial/estadisticas/estado
        [HttpGet("estadisticas/estado")]
        public async Task<IActionResult> ObtenerEstadisticasPorEstado()
        {
            var (exito, mensaje, estadisticas) = await _historialDao.ObtenerEstadisticasPorEstadoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/CitaHistorial/{id}/observacion
        [HttpPut("{id}/observacion")]
        public async Task<IActionResult> ActualizarObservacion(int id, [FromBody] ActualizarObservacionDto dto)
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

            var (exito, mensaje) = await _historialDao.ActualizarObservacionAsync(id, dto.Observacion);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // DELETE: api/CitaHistorial/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarHistorial(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _historialDao.EliminarHistorialAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // DELETE: api/CitaHistorial/cita/{idCita}
        [HttpDelete("cita/{idCita}")]
        public async Task<IActionResult> EliminarHistorialPorCita(int idCita)
        {
            if (idCita <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cita inválido" });
            }

            var (exito, mensaje) = await _historialDao.EliminarHistorialPorCitaAsync(idCita);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class CitaHistorialCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdCita { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CitaEstadoId { get; set; }

        [StringLength(2000)]
        public string? Observacion { get; set; }
    }

    public class ActualizarObservacionDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Observacion { get; set; } = string.Empty;
    }
}
