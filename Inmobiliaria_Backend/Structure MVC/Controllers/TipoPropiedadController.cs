using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPropiedadController : ControllerBase
    {
        private readonly TipoPropiedadDao _tipoPropiedadDao;

        public TipoPropiedadController()
        {
            _tipoPropiedadDao = new TipoPropiedadDao();
        }

        // POST: api/TipoPropiedad
        [HttpPost]
        public async Task<IActionResult> CrearTipoPropiedad([FromBody] TipoPropiedadCreateDto dto)
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

            var tipoPropiedad = new TipoPropiedad
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion ?? string.Empty,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var validationContext = new ValidationContext(tipoPropiedad);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(tipoPropiedad, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _tipoPropiedadDao.CrearTipoPropiedadAsync(tipoPropiedad);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerTipoPropiedad),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTipoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, tipoPropiedad) = await _tipoPropiedadDao.ObtenerTipoPropiedadPorIdAsync(id);

            if (exito && tipoPropiedad != null)
            {
                return Ok(new { exito = true, mensaje, data = tipoPropiedad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerTipoPropiedadPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, tipoPropiedad) = await _tipoPropiedadDao.ObtenerTipoPropiedadPorNombreAsync(nombre);

            if (exito && tipoPropiedad != null)
            {
                return Ok(new { exito = true, mensaje, data = tipoPropiedad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad
        [HttpGet]
        public async Task<IActionResult> ListarTiposPropiedad()
        {
            var (exito, mensaje, tiposPropiedad) = await _tipoPropiedadDao.ListarTiposPropiedadAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tiposPropiedad, total = tiposPropiedad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarTiposPropiedadPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, tiposPropiedad, totalRegistros) =
                await _tipoPropiedadDao.ListarTiposPropiedadPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = tiposPropiedad,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/buscar?termino=casa
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarTiposPropiedad([FromQuery] string? termino = null)
        {
            var (exito, mensaje, tiposPropiedad) = await _tipoPropiedadDao.BuscarTiposPropiedadAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tiposPropiedad, total = tiposPropiedad?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/{id}/propiedades/contar
        [HttpGet("{id}/propiedades/contar")]
        public async Task<IActionResult> ContarPropiedadesPorTipo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _tipoPropiedadDao.ContarPropiedadesPorTipoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _tipoPropiedadDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/mas-usados?limite=5
        [HttpGet("mas-usados")]
        public async Task<IActionResult> ObtenerTiposMasUsados([FromQuery] int limite = 5)
        {
            if (limite <= 0 || limite > 50)
            {
                return BadRequest(new { exito = false, mensaje = "El límite debe estar entre 1 y 50" });
            }

            var (exito, mensaje, tipos) = await _tipoPropiedadDao.ObtenerTiposMasUsadosAsync(limite);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipos, total = tipos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/TipoPropiedad/sin-uso
        [HttpGet("sin-uso")]
        public async Task<IActionResult> ObtenerTiposSinUso()
        {
            var (exito, mensaje, tipos) = await _tipoPropiedadDao.ObtenerTiposSinUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipos, total = tipos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/TipoPropiedad/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarTiposPropiedadPredeterminados()
        {
            var (exito, mensaje) = await _tipoPropiedadDao.InicializarTiposPropiedadPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/TipoPropiedad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarTipoPropiedad(int id, [FromBody] TipoPropiedadUpdateDto dto)
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

            var (existeExito, _, tipoExistente) = await _tipoPropiedadDao.ObtenerTipoPropiedadPorIdAsync(id);
            if (!existeExito || tipoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Tipo de propiedad no encontrado" });
            }

            tipoExistente.Nombre = dto.Nombre;
            tipoExistente.Descripcion = dto.Descripcion ?? string.Empty;
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

            var (exito, mensaje) = await _tipoPropiedadDao.ActualizarTipoPropiedadAsync(tipoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = tipoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/TipoPropiedad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTipoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _tipoPropiedadDao.EliminarTipoPropiedadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class TipoPropiedadCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }

    public class TipoPropiedadUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
