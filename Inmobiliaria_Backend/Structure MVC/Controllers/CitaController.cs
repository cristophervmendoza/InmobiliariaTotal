using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitaController : ControllerBase
    {
        private readonly CitaDao _citaDao;
        private readonly CitaHistorialDao _historialDao;

        public CitaController()
        {
            _citaDao = new CitaDao();
            _historialDao = new CitaHistorialDao();
        }

        // POST: api/Cita
        [HttpPost]
        public async Task<IActionResult> CrearCita([FromBody] CitaCreateDto dto)
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

            var cita = new Cita
            {
                Fecha = dto.Fecha,
                Hora = dto.Hora,
                IdCliente = dto.IdCliente,
                IdAgente = dto.IdAgente,
                IdEstadoCita = dto.IdEstadoCita,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(cita);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(cita, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _citaDao.CrearCitaAsync(cita);

            if (exito && id.HasValue)
            {
                // Crear registro en historial
                await _historialDao.CrearHistorialAsync(new CitaHistorial
                {
                    IdCita = id.Value,
                    CitaEstadoId = cita.IdEstadoCita,
                    Observacion = "Cita creada",
                    CreadoAt = DateTime.Now
                });

                return CreatedAtAction(nameof(ObtenerCita),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCita(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, cita) = await _citaDao.ObtenerCitaPorIdAsync(id);

            if (exito && cita != null)
            {
                return Ok(new { exito = true, mensaje, data = cita });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Cita
        [HttpGet]
        public async Task<IActionResult> ListarCitas()
        {
            var (exito, mensaje, citas) = await _citaDao.ListarCitasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarCitasPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, citas, totalRegistros) =
                await _citaDao.ListarCitasPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = citas,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarCitas(
            [FromQuery] int? idCliente = null,
            [FromQuery] int? idAgente = null,
            [FromQuery] int? idEstado = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var (exito, mensaje, citas) =
                await _citaDao.BuscarCitasAsync(idCliente, idAgente, idEstado, fechaInicio, fechaFin);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/cliente/{idCliente}
        [HttpGet("cliente/{idCliente}")]
        public async Task<IActionResult> ObtenerCitasPorCliente(int idCliente)
        {
            if (idCliente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cliente inválido" });
            }

            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasPorClienteAsync(idCliente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/agente/{idAgente}
        [HttpGet("agente/{idAgente}")]
        public async Task<IActionResult> ObtenerCitasPorAgente(int idAgente)
        {
            if (idAgente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de agente inválido" });
            }

            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasPorAgenteAsync(idAgente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/estado/{idEstado}
        [HttpGet("estado/{idEstado}")]
        public async Task<IActionResult> ObtenerCitasPorEstado(int idEstado)
        {
            if (idEstado <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de estado inválido" });
            }

            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasPorEstadoAsync(idEstado);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/hoy
        [HttpGet("hoy")]
        public async Task<IActionResult> ObtenerCitasDeHoy()
        {
            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasDeHoyAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/semana
        [HttpGet("semana")]
        public async Task<IActionResult> ObtenerCitasDeLaSemana()
        {
            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasDeLaSemanaAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/mes
        [HttpGet("mes")]
        public async Task<IActionResult> ObtenerCitasDelMes()
        {
            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasDelMesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/proximas?dias=7
        [HttpGet("proximas")]
        public async Task<IActionResult> ObtenerCitasProximas([FromQuery] int dias = 7)
        {
            if (dias <= 0 || dias > 365)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 365" });
            }

            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasProximasAsync(dias);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/pasadas?dias=30
        [HttpGet("pasadas")]
        public async Task<IActionResult> ObtenerCitasPasadas([FromQuery] int dias = 30)
        {
            if (dias <= 0 || dias > 365)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 365" });
            }

            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasPasadasAsync(dias);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/pendientes
        [HttpGet("pendientes")]
        public async Task<IActionResult> ObtenerCitasPendientes()
        {
            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasPendientesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/confirmadas
        [HttpGet("confirmadas")]
        public async Task<IActionResult> ObtenerCitasConfirmadas()
        {
            var (exito, mensaje, citas) = await _citaDao.ObtenerCitasConfirmadasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = citas, total = citas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/disponibilidad/{idAgente}?fecha=2025-10-25
        [HttpGet("disponibilidad/{idAgente}")]
        public async Task<IActionResult> ObtenerDisponibilidadAgente(int idAgente, [FromQuery] DateTime fecha)
        {
            if (idAgente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de agente inválido" });
            }

            var (exito, mensaje, horasDisponibles) =
                await _citaDao.ObtenerDisponibilidadAgenteAsync(idAgente, fecha);

            if (exito)
            {
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    horasDisponibles = horasDisponibles?.Select(h => h.ToString(@"hh\:mm")).ToList(),
                    total = horasDisponibles?.Count ?? 0
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _citaDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/estadisticas/estado
        [HttpGet("estadisticas/estado")]
        public async Task<IActionResult> ObtenerEstadisticasPorEstado()
        {
            var (exito, mensaje, estadisticas) = await _citaDao.ObtenerEstadisticasPorEstadoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/estadisticas/agente
        [HttpGet("estadisticas/agente")]
        public async Task<IActionResult> ObtenerEstadisticasPorAgente()
        {
            var (exito, mensaje, estadisticas) = await _citaDao.ObtenerEstadisticasPorAgenteAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/contar/cliente/{idCliente}
        [HttpGet("contar/cliente/{idCliente}")]
        public async Task<IActionResult> ContarCitasPorCliente(int idCliente)
        {
            if (idCliente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cliente inválido" });
            }

            var (exito, mensaje, total) = await _citaDao.ContarCitasPorClienteAsync(idCliente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cita/contar/agente/{idAgente}
        [HttpGet("contar/agente/{idAgente}")]
        public async Task<IActionResult> ContarCitasPorAgente(int idAgente)
        {
            if (idAgente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de agente inválido" });
            }

            var (exito, mensaje, total) = await _citaDao.ContarCitasPorAgenteAsync(idAgente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Cita/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCita(int id, [FromBody] CitaUpdateDto dto)
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

            var (existeExito, _, citaExistente) = await _citaDao.ObtenerCitaPorIdAsync(id);
            if (!existeExito || citaExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Cita no encontrada" });
            }

            var estadoAnterior = citaExistente.IdEstadoCita;

            citaExistente.Fecha = dto.Fecha;
            citaExistente.Hora = dto.Hora;
            citaExistente.IdAgente = dto.IdAgente;
            citaExistente.IdEstadoCita = dto.IdEstadoCita;
            citaExistente.ActualizadoAt = DateTime.Now;

            var (exito, mensaje) = await _citaDao.ActualizarCitaAsync(citaExistente);

            if (exito)
            {
                // Registrar en historial si cambió el estado
                if (estadoAnterior != dto.IdEstadoCita)
                {
                    await _historialDao.CrearHistorialAsync(new CitaHistorial
                    {
                        IdCita = id,
                        CitaEstadoId = dto.IdEstadoCita,
                        Observacion = dto.Observacion ?? "Estado actualizado",
                        CreadoAt = DateTime.Now
                    });
                }

                return Ok(new { exito = true, mensaje, data = citaExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Cita/{id}/estado
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstadoCita(int id, [FromBody] CambiarEstadoDto dto)
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

            var (exito, mensaje) = await _citaDao.CambiarEstadoCitaAsync(id, dto.IdEstado);

            if (exito)
            {
                // Registrar en historial
                await _historialDao.CrearHistorialAsync(new CitaHistorial
                {
                    IdCita = id,
                    CitaEstadoId = dto.IdEstado,
                    Observacion = dto.Observacion ?? "Estado actualizado",
                    CreadoAt = DateTime.Now
                });

                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Cita/{id}/cancelar
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarCita(int id, [FromBody] ObservacionDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _citaDao.CancelarCitaAsync(id);

            if (exito)
            {
                // Obtener ID del estado cancelada
                var (_, _, cita) = await _citaDao.ObtenerCitaPorIdAsync(id);
                if (cita != null)
                {
                    await _historialDao.CrearHistorialAsync(new CitaHistorial
                    {
                        IdCita = id,
                        CitaEstadoId = cita.IdEstadoCita,
                        Observacion = dto.Observacion ?? "Cita cancelada",
                        CreadoAt = DateTime.Now
                    });
                }

                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Cita/{id}/confirmar
        [HttpPatch("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarCita(int id, [FromBody] ObservacionDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _citaDao.ConfirmarCitaAsync(id);

            if (exito)
            {
                var (_, _, cita) = await _citaDao.ObtenerCitaPorIdAsync(id);
                if (cita != null)
                {
                    await _historialDao.CrearHistorialAsync(new CitaHistorial
                    {
                        IdCita = id,
                        CitaEstadoId = cita.IdEstadoCita,
                        Observacion = dto.Observacion ?? "Cita confirmada",
                        CreadoAt = DateTime.Now
                    });
                }

                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Cita/{id}/completar
        [HttpPatch("{id}/completar")]
        public async Task<IActionResult> CompletarCita(int id, [FromBody] ObservacionDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _citaDao.CompletarCitaAsync(id);

            if (exito)
            {
                var (_, _, cita) = await _citaDao.ObtenerCitaPorIdAsync(id);
                if (cita != null)
                {
                    await _historialDao.CrearHistorialAsync(new CitaHistorial
                    {
                        IdCita = id,
                        CitaEstadoId = cita.IdEstadoCita,
                        Observacion = dto.Observacion ?? "Cita completada",
                        CreadoAt = DateTime.Now
                    });
                }

                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Cita/{id}/reprogramar
        [HttpPatch("{id}/reprogramar")]
        public async Task<IActionResult> ReprogramarCita(int id, [FromBody] ReprogramarCitaDto dto)
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

            var (exito, mensaje) = await _citaDao.ReprogramarCitaAsync(id, dto.NuevaFecha, dto.NuevaHora);

            if (exito)
            {
                var (_, _, cita) = await _citaDao.ObtenerCitaPorIdAsync(id);
                if (cita != null)
                {
                    await _historialDao.CrearHistorialAsync(new CitaHistorial
                    {
                        IdCita = id,
                        CitaEstadoId = cita.IdEstadoCita,
                        Observacion = dto.Observacion ?? $"Cita reprogramada para {dto.NuevaFecha:dd/MM/yyyy} a las {dto.NuevaHora:hh\\:mm}",
                        CreadoAt = DateTime.Now
                    });
                }

                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Cita/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCita(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _citaDao.EliminarCitaAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class CitaCreateDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdCliente { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdAgente { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoCita { get; set; }
    }

    public class CitaUpdateDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdAgente { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoCita { get; set; }

        [StringLength(2000)]
        public string? Observacion { get; set; }
    }

    public class CambiarEstadoDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstado { get; set; }

        [StringLength(2000)]
        public string? Observacion { get; set; }
    }

    public class ObservacionDto
    {
        [StringLength(2000)]
        public string? Observacion { get; set; }
    }

    public class ReprogramarCitaDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime NuevaFecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan NuevaHora { get; set; }

        [StringLength(2000)]
        public string? Observacion { get; set; }
    }
}
