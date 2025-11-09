using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoPropiedadController : ControllerBase
    {
        private readonly EstadoPropiedadDao _estadoPropiedadDao;

        public EstadoPropiedadController()
        {
            _estadoPropiedadDao = new EstadoPropiedadDao();
        }

        // POST: api/EstadoPropiedad
        [HttpPost]
        public async Task<IActionResult> CrearEstadoPropiedad([FromBody] EstadoPropiedadCreateDto dto)
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

            var estadoPropiedad = new EstadoPropiedad
            {
                Nombre = dto.Nombre,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(estadoPropiedad);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(estadoPropiedad, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _estadoPropiedadDao.CrearEstadoPropiedadAsync(estadoPropiedad);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerEstadoPropiedad),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEstadoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, estadoPropiedad) = await _estadoPropiedadDao.ObtenerEstadoPropiedadPorIdAsync(id);

            if (exito && estadoPropiedad != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoPropiedad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerEstadoPropiedadPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, estadoPropiedad) = await _estadoPropiedadDao.ObtenerEstadoPropiedadPorNombreAsync(nombre);

            if (exito && estadoPropiedad != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoPropiedad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad
        [HttpGet]
        public async Task<IActionResult> ListarEstadosPropiedad()
        {
            var (exito, mensaje, estadosPropiedad) = await _estadoPropiedadDao.ListarEstadosPropiedadAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosPropiedad, total = estadosPropiedad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarEstadosPropiedadPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, estadosPropiedad, totalRegistros) =
                await _estadoPropiedadDao.ListarEstadosPropiedadPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = estadosPropiedad,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/buscar?termino=disponible
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarEstadosPropiedad([FromQuery] string? termino = null)
        {
            var (exito, mensaje, estadosPropiedad) = await _estadoPropiedadDao.BuscarEstadosPropiedadAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosPropiedad, total = estadosPropiedad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/{id}/propiedades/contar
        [HttpGet("{id}/propiedades/contar")]
        public async Task<IActionResult> ContarPropiedadesPorEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _estadoPropiedadDao.ContarPropiedadesPorEstadoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _estadoPropiedadDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/mas-usados?limite=5
        [HttpGet("mas-usados")]
        public async Task<IActionResult> ObtenerEstadosMasUsados([FromQuery] int limite = 5)
        {
            if (limite <= 0 || limite > 50)
            {
                return BadRequest(new { exito = false, mensaje = "El límite debe estar entre 1 y 50" });
            }

            var (exito, mensaje, estados) = await _estadoPropiedadDao.ObtenerEstadosMasUsadosAsync(limite);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estados, total = estados?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoPropiedad/sin-uso
        [HttpGet("sin-uso")]
        public async Task<IActionResult> ObtenerEstadosSinUso()
        {
            var (exito, mensaje, estados) = await _estadoPropiedadDao.ObtenerEstadosSinUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estados, total = estados?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/EstadoPropiedad/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarEstadosPredeterminados()
        {
            var (exito, mensaje) = await _estadoPropiedadDao.InicializarEstadosPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/EstadoPropiedad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEstadoPropiedad(int id, [FromBody] EstadoPropiedadUpdateDto dto)
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

            var (existeExito, _, estadoExistente) = await _estadoPropiedadDao.ObtenerEstadoPropiedadPorIdAsync(id);
            if (!existeExito || estadoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Estado de propiedad no encontrado" });
            }

            estadoExistente.Nombre = dto.Nombre;
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

            var (exito, mensaje) = await _estadoPropiedadDao.ActualizarEstadoPropiedadAsync(estadoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/EstadoPropiedad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEstadoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoPropiedadDao.EliminarEstadoPropiedadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class EstadoPropiedadCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EstadoPropiedadUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }
}
