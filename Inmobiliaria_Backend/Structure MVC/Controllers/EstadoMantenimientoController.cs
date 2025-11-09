using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoMantenimientoController : ControllerBase
    {
        private readonly EstadoMantenimientoDao _estadoMantenimientoDao;

        public EstadoMantenimientoController()
        {
            _estadoMantenimientoDao = new EstadoMantenimientoDao();
        }

        // POST: api/EstadoMantenimiento
        [HttpPost]
        public async Task<IActionResult> CrearEstadoMantenimiento([FromBody] EstadoMantenimientoCreateDto dto)
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

            var estadoMantenimiento = new EstadoMantenimiento
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion ?? string.Empty,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(estadoMantenimiento);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(estadoMantenimiento, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _estadoMantenimientoDao.CrearEstadoMantenimientoAsync(estadoMantenimiento);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerEstadoMantenimiento),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEstadoMantenimiento(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, estadoMantenimiento) = await _estadoMantenimientoDao.ObtenerEstadoMantenimientoPorIdAsync(id);

            if (exito && estadoMantenimiento != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoMantenimiento });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerEstadoMantenimientoPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, estadoMantenimiento) = await _estadoMantenimientoDao.ObtenerEstadoMantenimientoPorNombreAsync(nombre);

            if (exito && estadoMantenimiento != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoMantenimiento });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento
        [HttpGet]
        public async Task<IActionResult> ListarEstadosMantenimiento()
        {
            var (exito, mensaje, estadosMantenimiento) = await _estadoMantenimientoDao.ListarEstadosMantenimientoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosMantenimiento, total = estadosMantenimiento?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarEstadosMantenimientoPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, estadosMantenimiento, totalRegistros) =
                await _estadoMantenimientoDao.ListarEstadosMantenimientoPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = estadosMantenimiento,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/buscar?termino=pendiente
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarEstadosMantenimiento([FromQuery] string? termino = null)
        {
            var (exito, mensaje, estadosMantenimiento) = await _estadoMantenimientoDao.BuscarEstadosMantenimientoAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosMantenimiento, total = estadosMantenimiento?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/{id}/mantenimientos/contar
        [HttpGet("{id}/mantenimientos/contar")]
        public async Task<IActionResult> ContarMantenimientosPorEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _estadoMantenimientoDao.ContarMantenimientosPorEstadoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoMantenimiento/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _estadoMantenimientoDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/EstadoMantenimiento/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarEstadosPredeterminados()
        {
            var (exito, mensaje) = await _estadoMantenimientoDao.InicializarEstadosPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/EstadoMantenimiento/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEstadoMantenimiento(int id, [FromBody] EstadoMantenimientoUpdateDto dto)
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

            var (existeExito, _, estadoExistente) = await _estadoMantenimientoDao.ObtenerEstadoMantenimientoPorIdAsync(id);
            if (!existeExito || estadoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Estado de mantenimiento no encontrado" });
            }

            estadoExistente.Nombre = dto.Nombre;
            estadoExistente.Descripcion = dto.Descripcion ?? string.Empty;
            estadoExistente.ActualizadoAt = DateTime.Now;

            var validationContext = new ValidationContext(estadoExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(estadoExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje) = await _estadoMantenimientoDao.ActualizarEstadoMantenimientoAsync(estadoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/EstadoMantenimiento/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEstadoMantenimiento(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoMantenimientoDao.EliminarEstadoMantenimientoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class EstadoMantenimientoCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    public class EstadoMantenimientoUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
