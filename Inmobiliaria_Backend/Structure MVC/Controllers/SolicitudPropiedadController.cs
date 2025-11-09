using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudPropiedadController : ControllerBase
    {
        private readonly SolicitudPropiedadDao _solicitudPropiedadDao;

        public SolicitudPropiedadController()
        {
            _solicitudPropiedadDao = new SolicitudPropiedadDao();
        }

        // POST: api/SolicitudPropiedad
        [HttpPost]
        public async Task<IActionResult> CrearSolicitudPropiedad([FromBody] SolicitudPropiedadCreateDto dto)
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

            var solicitud = new SolicitudPropiedad
            {
                IdUsuario = dto.IdUsuario,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion ?? string.Empty,
                FotoPropiedad = dto.FotoPropiedad ?? string.Empty,
                SolicitudEstado = dto.SolicitudEstado ?? "Pendiente",
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(solicitud);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(solicitud, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _solicitudPropiedadDao.CrearSolicitudPropiedadAsync(solicitud);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerSolicitudPropiedad),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerSolicitudPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, solicitud) = await _solicitudPropiedadDao.ObtenerSolicitudPropiedadPorIdAsync(id);

            if (exito && solicitud != null)
            {
                return Ok(new { exito = true, mensaje, data = solicitud });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad
        [HttpGet]
        public async Task<IActionResult> ListarSolicitudesPropiedad()
        {
            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ListarSolicitudesPropiedadAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarSolicitudesPropiedadPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, solicitudes, totalRegistros) =
                await _solicitudPropiedadDao.ListarSolicitudesPropiedadPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = solicitudes,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarSolicitudesPropiedad(
            [FromQuery] string? termino = null,
            [FromQuery] string? estado = null,
            [FromQuery] int? idUsuario = null)
        {
            if (!string.IsNullOrEmpty(estado))
            {
                var estadosValidos = new[] { "Pendiente", "En Revision", "Aprobada", "Rechazada", "Cancelada", "En Proceso", "Completada" };
                if (!estadosValidos.Any(e => e.Equals(estado, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new
                    {
                        exito = false,
                        mensaje = $"Estado inválido. Debe ser: {string.Join(", ", estadosValidos)}"
                    });
                }
            }

            var (exito, mensaje, solicitudes) =
                await _solicitudPropiedadDao.BuscarSolicitudesPropiedadAsync(termino, estado, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerSolicitudesPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/estado/{estado}
        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> ObtenerSolicitudesPorEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return BadRequest(new { exito = false, mensaje = "Estado inválido" });
            }

            var estadosValidos = new[] { "Pendiente", "En Revision", "Aprobada", "Rechazada", "Cancelada", "En Proceso", "Completada" };
            if (!estadosValidos.Any(e => e.Equals(estado, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = $"Estado inválido. Debe ser: {string.Join(", ", estadosValidos)}"
                });
            }

            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesPorEstadoAsync(estado);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/pendientes
        [HttpGet("pendientes")]
        public async Task<IActionResult> ObtenerSolicitudesPendientes()
        {
            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesPendientesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/aprobadas
        [HttpGet("aprobadas")]
        public async Task<IActionResult> ObtenerSolicitudesAprobadas()
        {
            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesAprobadasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/rechazadas
        [HttpGet("rechazadas")]
        public async Task<IActionResult> ObtenerSolicitudesRechazadas()
        {
            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesRechazadasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/en-revision
        [HttpGet("en-revision")]
        public async Task<IActionResult> ObtenerSolicitudesEnRevision()
        {
            var (exito, mensaje, solicitudes) = await _solicitudPropiedadDao.ObtenerSolicitudesEnRevisionAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudes, total = solicitudes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/usuario/{idUsuario}/contar
        [HttpGet("usuario/{idUsuario}/contar")]
        public async Task<IActionResult> ContarSolicitudesPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, total) = await _solicitudPropiedadDao.ContarSolicitudesPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _solicitudPropiedadDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/SolicitudPropiedad/estadisticas/estado
        [HttpGet("estadisticas/estado")]
        public async Task<IActionResult> ObtenerEstadisticasPorEstado()
        {
            var (exito, mensaje, estadisticas) = await _solicitudPropiedadDao.ObtenerEstadisticasPorEstadoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/SolicitudPropiedad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarSolicitudPropiedad(int id, [FromBody] SolicitudPropiedadUpdateDto dto)
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

            var (existeExito, _, solicitudExistente) = await _solicitudPropiedadDao.ObtenerSolicitudPropiedadPorIdAsync(id);
            if (!existeExito || solicitudExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Solicitud de propiedad no encontrada" });
            }

            solicitudExistente.Titulo = dto.Titulo;
            solicitudExistente.Descripcion = dto.Descripcion ?? string.Empty;
            solicitudExistente.FotoPropiedad = dto.FotoPropiedad ?? string.Empty;
            solicitudExistente.SolicitudEstado = dto.SolicitudEstado ?? solicitudExistente.SolicitudEstado;
            solicitudExistente.ActualizadoAt = DateTime.Now;

            var validationContext = new ValidationContext(solicitudExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(solicitudExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.ActualizarSolicitudPropiedadAsync(solicitudExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = solicitudExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/SolicitudPropiedad/{id}/estado
        [HttpPatch("{id}/estado")]
        public async Task<IActionResult> CambiarEstadoSolicitud(int id, [FromBody] CambiarEstadoSolicitudDto dto)
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

            var estadosValidos = new[] { "Pendiente", "En Revision", "Aprobada", "Rechazada", "Cancelada", "En Proceso", "Completada" };
            if (!estadosValidos.Any(e => e.Equals(dto.NuevoEstado, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = $"Estado inválido. Debe ser: {string.Join(", ", estadosValidos)}"
                });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.CambiarEstadoSolicitudAsync(id, dto.NuevoEstado);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/SolicitudPropiedad/{id}/aprobar
        [HttpPatch("{id}/aprobar")]
        public async Task<IActionResult> AprobarSolicitud(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.AprobarSolicitudAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/SolicitudPropiedad/{id}/rechazar
        [HttpPatch("{id}/rechazar")]
        public async Task<IActionResult> RechazarSolicitud(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.RechazarSolicitudAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/SolicitudPropiedad/{id}/revision
        [HttpPatch("{id}/revision")]
        public async Task<IActionResult> PonerEnRevision(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.PonerEnRevisionAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/SolicitudPropiedad/{id}/cancelar
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> CancelarSolicitud(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.CancelarSolicitudAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/SolicitudPropiedad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarSolicitudPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _solicitudPropiedadDao.EliminarSolicitudPropiedadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class SolicitudPropiedadCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 10)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^[a-zA-Z0-9\-_\.\/\\:]+\.(jpg|jpeg|png|gif|webp|bmp)$")]
        public string? FotoPropiedad { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string? SolicitudEstado { get; set; }
    }

    public class SolicitudPropiedadUpdateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 10)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^[a-zA-Z0-9\-_\.\/\\:]+\.(jpg|jpeg|png|gif|webp|bmp)$")]
        public string? FotoPropiedad { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string? SolicitudEstado { get; set; }
    }

    public class CambiarEstadoSolicitudDto
    {
        [Required]
        [RegularExpression(@"^(Pendiente|En Revision|Aprobada|Rechazada|Cancelada|En Proceso|Completada)$",
            ErrorMessage = "El estado debe ser: Pendiente, En Revision, Aprobada, Rechazada, Cancelada, En Proceso o Completada")]
        public string NuevoEstado { get; set; } = string.Empty;
    }
}
