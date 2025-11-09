using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPrioridadController : ControllerBase
    {
        private readonly TipoPrioridadDao _tipoPrioridadDao;

        public TipoPrioridadController()
        {
            _tipoPrioridadDao = new TipoPrioridadDao();
        }

        // POST: api/TipoPrioridad
        [HttpPost]
        public async Task<IActionResult> CrearTipoPrioridad([FromBody] TipoPrioridadCreateDto dto)
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

            var tipoPrioridad = new TipoPrioridad
            {
                Nombre = dto.Nombre,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(tipoPrioridad);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(tipoPrioridad, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _tipoPrioridadDao.CrearTipoPrioridadAsync(tipoPrioridad);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerTipoPrioridad),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTipoPrioridad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, tipoPrioridad) = await _tipoPrioridadDao.ObtenerTipoPrioridadPorIdAsync(id);

            if (exito && tipoPrioridad != null)
            {
                return Ok(new { exito = true, mensaje, data = tipoPrioridad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerTipoPrioridadPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, tipoPrioridad) = await _tipoPrioridadDao.ObtenerTipoPrioridadPorNombreAsync(nombre);

            if (exito && tipoPrioridad != null)
            {
                return Ok(new { exito = true, mensaje, data = tipoPrioridad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad
        [HttpGet]
        public async Task<IActionResult> ListarTiposPrioridad()
        {
            var (exito, mensaje, tiposPrioridad) = await _tipoPrioridadDao.ListarTiposPrioridadAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tiposPrioridad, total = tiposPrioridad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarTiposPrioridadPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, tiposPrioridad, totalRegistros) =
                await _tipoPrioridadDao.ListarTiposPrioridadPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = tiposPrioridad,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/buscar?termino=alta
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarTiposPrioridad([FromQuery] string? termino = null)
        {
            var (exito, mensaje, tiposPrioridad) = await _tipoPrioridadDao.BuscarTiposPrioridadAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tiposPrioridad, total = tiposPrioridad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/{id}/agendas/contar
        [HttpGet("{id}/agendas/contar")]
        public async Task<IActionResult> ContarAgendasPorTipoPrioridad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _tipoPrioridadDao.ContarAgendasPorTipoPrioridadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _tipoPrioridadDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/mas-usados?limite=5
        [HttpGet("mas-usados")]
        public async Task<IActionResult> ObtenerTiposMasUsados([FromQuery] int limite = 5)
        {
            if (limite <= 0 || limite > 50)
            {
                return BadRequest(new { exito = false, mensaje = "El límite debe estar entre 1 y 50" });
            }

            var (exito, mensaje, tipos) = await _tipoPrioridadDao.ObtenerTiposMasUsadosAsync(limite);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipos, total = tipos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPrioridad/sin-uso
        [HttpGet("sin-uso")]
        public async Task<IActionResult> ObtenerTiposSinUso()
        {
            var (exito, mensaje, tipos) = await _tipoPrioridadDao.ObtenerTiposSinUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipos, total = tipos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/TipoPrioridad/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarTiposPrioridadPredeterminados()
        {
            var (exito, mensaje) = await _tipoPrioridadDao.InicializarTiposPrioridadPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/TipoPrioridad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarTipoPrioridad(int id, [FromBody] TipoPrioridadUpdateDto dto)
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

            var (existeExito, _, tipoExistente) = await _tipoPrioridadDao.ObtenerTipoPrioridadPorIdAsync(id);
            if (!existeExito || tipoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Tipo de prioridad no encontrado" });
            }

            tipoExistente.Nombre = dto.Nombre;
            tipoExistente.ActualizadoAt = DateTime.Now;

            var validationContext = new ValidationContext(tipoExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(tipoExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje) = await _tipoPrioridadDao.ActualizarTipoPrioridadAsync(tipoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/TipoPrioridad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTipoPrioridad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _tipoPrioridadDao.EliminarTipoPrioridadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class TipoPrioridadCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }

    public class TipoPrioridadUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;
    }
}
