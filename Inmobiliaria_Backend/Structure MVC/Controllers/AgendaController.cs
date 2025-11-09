using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgendaController : ControllerBase
    {
        private readonly AgendaDao _agendaDao;

        public AgendaController()
        {
            _agendaDao = new AgendaDao();
        }

        // POST: api/Agenda
        [HttpPost]
        public async Task<IActionResult> CrearAgenda([FromBody] AgendaCreateDto dto)
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

            var agenda = new Agenda
            {
                IdUsuario = dto.IdUsuario,
                Titulo = dto.Titulo,
                Tipo = dto.Tipo,
                Telefono = dto.Telefono,
                FechaHora = dto.FechaHora,
                DescripcionEvento = dto.DescripcionEvento,
                Estado = dto.Estado ?? "Pendiente",
                Ubicacion = dto.Ubicacion,
                CreadoAt = DateTime.UtcNow,
                ActualizadoAt = DateTime.UtcNow,
                IdTipoPrioridad = dto.IdTipoPrioridad
            };

            var validationContext = new ValidationContext(agenda);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(agenda, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            try
            {
                agenda.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, id) = await _agendaDao.CrearAgendaAsync(agenda);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerAgendaPorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerAgendaPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, agenda) = await _agendaDao.ObtenerAgendaPorIdAsync(id);

            if (exito && agenda != null)
            {
                var response = new
                {
                    exito = true,
                    mensaje,
                    data = agenda,
                    esProximo = agenda.EsEventoProximo(),
                    esPasado = agenda.EsEventoPasado(),
                    tiempoRestante = agenda.TiempoRestante(),
                    puedeCancelar = agenda.PuedeCancelar(),
                    puedeCompletar = agenda.PuedeCompletar()
                };
                return Ok(response);
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Agenda
        [HttpGet]
        public async Task<IActionResult> ListarAgendas()
        {
            var (exito, mensaje, agendas) = await _agendaDao.ListarAgendasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendas, total = agendas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarAgendasPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos. Página debe ser > 0 y tamaño entre 1 y 100"
                });
            }

            var (exito, mensaje, agendas, totalRegistros) =
                await _agendaDao.ListarAgendasPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = agendas,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/buscar?termino=reunion&estado=Pendiente
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarAgendas(
            [FromQuery] string? termino = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? idUsuario = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idTipoPrioridad = null)
        {
            if (!string.IsNullOrEmpty(estado))
            {
                var estadosValidos = new[] { "Pendiente", "Confirmado", "Cancelado", "Completado", "En Proceso", "Reagendado" };
                if (!estadosValidos.Contains(estado))
                {
                    return BadRequest(new { exito = false, mensaje = $"Estado inválido. Debe ser: {string.Join(", ", estadosValidos)}" });
                }
            }

            var (exito, mensaje, agendas) =
                await _agendaDao.BuscarAgendasAsync(termino, estado, idUsuario, fechaInicio, fechaFin, idTipoPrioridad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendas, total = agendas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/proximos?idUsuario=1
        [HttpGet("proximos")]
        public async Task<IActionResult> ObtenerEventosProximos([FromQuery] int? idUsuario = null)
        {
            var (exito, mensaje, agendas) = await _agendaDao.ObtenerEventosProximosAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendas, total = agendas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/rango?fechaInicio=2025-01-01&fechaFin=2025-12-31
        [HttpGet("rango")]
        public async Task<IActionResult> ObtenerEventosPorRango(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] int? idUsuario = null)
        {
            if (fechaInicio > fechaFin)
            {
                return BadRequest(new { exito = false, mensaje = "La fecha de inicio no puede ser posterior a la fecha de fin" });
            }

            var (exito, mensaje, agendas) =
                await _agendaDao.ObtenerEventosPorRangoAsync(fechaInicio, fechaFin, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendas, total = agendas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerEventosPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, agendas) =
                await _agendaDao.ObtenerEventosPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendas, total = agendas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Agenda/estadisticas?idUsuario=1
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas([FromQuery] int? idUsuario = null)
        {
            var (exito, mensaje, estadisticas) = await _agendaDao.ObtenerEstadisticasAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Agenda/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarAgenda(int id, [FromBody] AgendaUpdateDto dto)
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

            var (existeExito, _, agendaExistente) = await _agendaDao.ObtenerAgendaPorIdAsync(id);
            if (!existeExito || agendaExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Evento de agenda no encontrado" });
            }

            agendaExistente.Titulo = dto.Titulo;
            agendaExistente.Tipo = dto.Tipo;
            agendaExistente.Telefono = dto.Telefono;
            agendaExistente.FechaHora = dto.FechaHora;
            agendaExistente.DescripcionEvento = dto.DescripcionEvento;
            agendaExistente.Estado = dto.Estado ?? agendaExistente.Estado;
            agendaExistente.Ubicacion = dto.Ubicacion;
            agendaExistente.IdTipoPrioridad = dto.IdTipoPrioridad;
            agendaExistente.ActualizarTiempos();

            var validationContext = new ValidationContext(agendaExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(agendaExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            try
            {
                agendaExistente.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _agendaDao.ActualizarAgendaAsync(agendaExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agendaExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Agenda/{id}/estado
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoAgendaDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var estadosValidos = new[] { "Pendiente", "Confirmado", "Cancelado", "Completado", "En Proceso", "Reagendado" };
            if (!estadosValidos.Contains(dto.NuevoEstado))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = $"Estado inválido. Debe ser: {string.Join(", ", estadosValidos)}"
                });
            }

            var (existeExito, _, agenda) = await _agendaDao.ObtenerAgendaPorIdAsync(id);
            if (!existeExito || agenda == null)
            {
                return NotFound(new { exito = false, mensaje = "Evento no encontrado" });
            }

            if (dto.NuevoEstado == "Cancelado" && !agenda.PuedeCancelar())
            {
                return BadRequest(new { exito = false, mensaje = "Este evento no puede ser cancelado" });
            }

            if (dto.NuevoEstado == "Completado" && !agenda.PuedeCompletar())
            {
                return BadRequest(new { exito = false, mensaje = "Este evento no puede ser completado" });
            }

            var (exito, mensaje) = await _agendaDao.CambiarEstadoAsync(id, dto.NuevoEstado);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Agenda/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAgenda(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _agendaDao.EliminarAgendaAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs para las peticiones
    public class AgendaCreateDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdUsuario debe ser mayor que cero")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-()]+$", ErrorMessage = "El título contiene caracteres no válidos")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El tipo debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s.\-]*$", ErrorMessage = "El tipo solo puede contener letras, espacios, puntos y guiones")]
        public string? Tipo { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 caracteres")]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y contener exactamente 9 dígitos")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha y hora del evento es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime FechaHora { get; set; }

        [StringLength(5000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 5000 caracteres")]
        [DataType(DataType.MultilineText)]
        public string? DescripcionEvento { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El estado debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^(Pendiente|Confirmado|Cancelado|Completado|En Proceso|Reagendado)$",
            ErrorMessage = "El estado debe ser: Pendiente, Confirmado, Cancelado, Completado, En Proceso o Reagendado")]
        public string? Estado { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "La ubicación debe tener entre 5 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-#°/]+$", ErrorMessage = "La ubicación contiene caracteres no válidos")]
        public string? Ubicacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El IdTipoPrioridad debe ser mayor que cero")]
        public int? IdTipoPrioridad { get; set; }
    }

    public class AgendaUpdateDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-()]+$", ErrorMessage = "El título contiene caracteres no válidos")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El tipo debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s.\-]*$", ErrorMessage = "El tipo solo puede contener letras")]
        public string? Tipo { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 caracteres")]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha y hora del evento es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime FechaHora { get; set; }

        [StringLength(5000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 5000 caracteres")]
        [DataType(DataType.MultilineText)]
        public string? DescripcionEvento { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El estado debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^(Pendiente|Confirmado|Cancelado|Completado|En Proceso|Reagendado)$",
            ErrorMessage = "El estado debe ser válido")]
        public string? Estado { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "La ubicación debe tener entre 5 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-#°/]+$", ErrorMessage = "La ubicación contiene caracteres no válidos")]
        public string? Ubicacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El IdTipoPrioridad debe ser mayor que cero")]
        public int? IdTipoPrioridad { get; set; }
    }

    // Renombrado de CambiarEstadoDto a CambiarEstadoAgendaDto
    public class CambiarEstadoAgendaDto
    {
        [Required(ErrorMessage = "El nuevo estado es obligatorio")]
        [RegularExpression(@"^(Pendiente|Confirmado|Cancelado|Completado|En Proceso|Reagendado)$",
            ErrorMessage = "El estado debe ser: Pendiente, Confirmado, Cancelado, Completado, En Proceso o Reagendado")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
