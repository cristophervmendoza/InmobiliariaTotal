using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MantenimientoController : ControllerBase
    {
        private readonly MantenimientoDao _mantenimientoDao;

        public MantenimientoController()
        {
            _mantenimientoDao = new MantenimientoDao();
        }

        // POST: api/Mantenimiento
        [HttpPost]
        public async Task<IActionResult> CrearMantenimiento([FromBody] MantenimientoCreateDto dto)
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

            var mantenimiento = new Mantenimiento
            {
                IdPropiedad = dto.IdPropiedad,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion ?? string.Empty,
                Costo = dto.Costo,
                FechaProgramada = dto.FechaProgramada,
                FechaRealizada = dto.FechaRealizada,
                IdEstadoMantenimiento = dto.IdEstadoMantenimiento,
                IdUsuario = dto.IdUsuario,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(mantenimiento);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(mantenimiento, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _mantenimientoDao.CrearMantenimientoAsync(mantenimiento);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerMantenimiento),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerMantenimiento(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, mantenimiento) = await _mantenimientoDao.ObtenerMantenimientoPorIdAsync(id);

            if (exito && mantenimiento != null)
            {
                return Ok(new { exito = true, mensaje, data = mantenimiento });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento
        [HttpGet]
        public async Task<IActionResult> ListarMantenimientos()
        {
            var (exito, mensaje, mantenimientos) = await _mantenimientoDao.ListarMantenimientosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarMantenimientosPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, mantenimientos, totalRegistros) =
                await _mantenimientoDao.ListarMantenimientosPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = mantenimientos,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarMantenimientos(
            [FromQuery] int? idPropiedad = null,
            [FromQuery] int? idEstado = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] int? idUsuario = null)
        {
            var (exito, mensaje, mantenimientos) =
                await _mantenimientoDao.BuscarMantenimientosAsync(idPropiedad, idEstado, tipo, fechaInicio, fechaFin, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/propiedad/{idPropiedad}
        [HttpGet("propiedad/{idPropiedad}")]
        public async Task<IActionResult> ObtenerMantenimientosPorPropiedad(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, mantenimientos) = await _mantenimientoDao.ObtenerMantenimientosPorPropiedadAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/pendientes
        [HttpGet("pendientes")]
        public async Task<IActionResult> ObtenerMantenimientosPendientes()
        {
            var (exito, mensaje, mantenimientos) = await _mantenimientoDao.ObtenerMantenimientosPendientesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/vencidos
        [HttpGet("vencidos")]
        public async Task<IActionResult> ObtenerMantenimientosVencidos()
        {
            var (exito, mensaje, mantenimientos) = await _mantenimientoDao.ObtenerMantenimientosVencidosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/proximos?dias=7
        [HttpGet("proximos")]
        public async Task<IActionResult> ObtenerMantenimientosProximos([FromQuery] int dias = 7)
        {
            if (dias <= 0 || dias > 365)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 365" });
            }

            var (exito, mensaje, mantenimientos) = await _mantenimientoDao.ObtenerMantenimientosProximosAsync(dias);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientos, total = mantenimientos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/propiedad/{idPropiedad}/costo-total
        [HttpGet("propiedad/{idPropiedad}/costo-total")]
        public async Task<IActionResult> CalcularCostoTotalPorPropiedad(
            int idPropiedad,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, total) =
                await _mantenimientoDao.CalcularCostoTotalPorPropiedadAsync(idPropiedad, fechaInicio, fechaFin);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, costoTotal = total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Mantenimiento/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas([FromQuery] int? idPropiedad = null)
        {
            var (exito, mensaje, estadisticas) = await _mantenimientoDao.ObtenerEstadisticasAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Mantenimiento/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMantenimiento(int id, [FromBody] MantenimientoUpdateDto dto)
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

            var (existeExito, _, mantenimientoExistente) = await _mantenimientoDao.ObtenerMantenimientoPorIdAsync(id);
            if (!existeExito || mantenimientoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Mantenimiento no encontrado" });
            }

            mantenimientoExistente.Tipo = dto.Tipo;
            mantenimientoExistente.Descripcion = dto.Descripcion ?? string.Empty;
            mantenimientoExistente.Costo = dto.Costo;
            mantenimientoExistente.FechaProgramada = dto.FechaProgramada;
            mantenimientoExistente.FechaRealizada = dto.FechaRealizada;
            mantenimientoExistente.IdEstadoMantenimiento = dto.IdEstadoMantenimiento;
            mantenimientoExistente.IdUsuario = dto.IdUsuario;
            mantenimientoExistente.ActualizadoAt = DateTime.Now;

            var validationContext = new ValidationContext(mantenimientoExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(mantenimientoExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje) = await _mantenimientoDao.ActualizarMantenimientoAsync(mantenimientoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = mantenimientoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/Mantenimiento/{id}/completar
        [HttpPatch("{id}/completar")]
        public async Task<IActionResult> CompletarMantenimiento(int id, [FromBody] CompletarMantenimientoDto dto)
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

            var (exito, mensaje) = await _mantenimientoDao.CompletarMantenimientoAsync(id, dto.Costo, dto.IdUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Mantenimiento/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMantenimiento(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _mantenimientoDao.EliminarMantenimientoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class MantenimientoCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdPropiedad { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$")]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [Range(0, 999999.99)]
        public decimal? Costo { get; set; }

        public DateTime? FechaProgramada { get; set; }

        public DateTime? FechaRealizada { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoMantenimiento { get; set; }

        public int? IdUsuario { get; set; }
    }

    public class MantenimientoUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$")]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Descripcion { get; set; }

        [Range(0, 999999.99)]
        public decimal? Costo { get; set; }

        public DateTime? FechaProgramada { get; set; }

        public DateTime? FechaRealizada { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoMantenimiento { get; set; }

        public int? IdUsuario { get; set; }
    }

    public class CompletarMantenimientoDto
    {
        [Required]
        [Range(0.01, 999999.99)]
        public decimal Costo { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }
    }
}
