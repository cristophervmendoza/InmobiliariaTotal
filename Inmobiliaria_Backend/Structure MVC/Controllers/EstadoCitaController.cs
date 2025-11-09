using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoCitaController : ControllerBase
    {
        private readonly EstadoCitaDao _estadoCitaDao;

        public EstadoCitaController()
        {
            _estadoCitaDao = new EstadoCitaDao();
        }

        // POST: api/EstadoCita
        [HttpPost]
        public async Task<IActionResult> CrearEstadoCita([FromBody] EstadoCitaCreateDto dto)
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

            var estadoCita = new EstadoCita
            {
                Nombre = dto.Nombre,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(estadoCita);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(estadoCita, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _estadoCitaDao.CrearEstadoCitaAsync(estadoCita);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerEstadoCita),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEstadoCita(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, estadoCita) = await _estadoCitaDao.ObtenerEstadoCitaPorIdAsync(id);

            if (exito && estadoCita != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoCita });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerEstadoCitaPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, estadoCita) = await _estadoCitaDao.ObtenerEstadoCitaPorNombreAsync(nombre);

            if (exito && estadoCita != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoCita });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita
        [HttpGet]
        public async Task<IActionResult> ListarEstadosCita()
        {
            var (exito, mensaje, estadosCita) = await _estadoCitaDao.ListarEstadosCitaAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosCita, total = estadosCita?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarEstadosCitaPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, estadosCita, totalRegistros) =
                await _estadoCitaDao.ListarEstadosCitaPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = estadosCita,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/buscar?termino=pendiente
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarEstadosCita([FromQuery] string? termino = null)
        {
            var (exito, mensaje, estadosCita) = await _estadoCitaDao.BuscarEstadosCitaAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosCita, total = estadosCita?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/{id}/citas/contar
        [HttpGet("{id}/citas/contar")]
        public async Task<IActionResult> ContarCitasPorEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _estadoCitaDao.ContarCitasPorEstadoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _estadoCitaDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/mas-usados?limite=5
        [HttpGet("mas-usados")]
        public async Task<IActionResult> ObtenerEstadosMasUsados([FromQuery] int limite = 5)
        {
            if (limite <= 0 || limite > 50)
            {
                return BadRequest(new { exito = false, mensaje = "El límite debe estar entre 1 y 50" });
            }

            var (exito, mensaje, estados) = await _estadoCitaDao.ObtenerEstadosMasUsadosAsync(limite);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estados, total = estados?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoCita/sin-uso
        [HttpGet("sin-uso")]
        public async Task<IActionResult> ObtenerEstadosSinUso()
        {
            var (exito, mensaje, estados) = await _estadoCitaDao.ObtenerEstadosSinUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estados, total = estados?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/EstadoCita/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarEstadosPredeterminados()
        {
            var (exito, mensaje) = await _estadoCitaDao.InicializarEstadosPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/EstadoCita/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEstadoCita(int id, [FromBody] EstadoCitaUpdateDto dto)
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

            var (existeExito, _, estadoExistente) = await _estadoCitaDao.ObtenerEstadoCitaPorIdAsync(id);
            if (!existeExito || estadoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Estado de cita no encontrado" });
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

            var (exito, mensaje) = await _estadoCitaDao.ActualizarEstadoCitaAsync(estadoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/EstadoCita/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEstadoCita(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoCitaDao.EliminarEstadoCitaAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class EstadoCitaCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class EstadoCitaUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }
}