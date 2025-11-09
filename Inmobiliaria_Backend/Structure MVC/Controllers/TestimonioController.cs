using backend_csharpcd_inmo.Structure_MVC.DAO;

using backend_csharpcd_inmo.Structure_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestimonioController : ControllerBase
    {
        private readonly TestimonioDao _testimonioDao;

        public TestimonioController()
        {
            _testimonioDao = new TestimonioDao();
        }

        // POST: api/Testimonio
        [HttpPost]
        public async Task<IActionResult> CrearTestimonio([FromBody] TestimonioCreateDto dto)
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

            var testimonio = new Testimonio
            {
                IdUsuario = dto.IdUsuario,
                Contenido = dto.Contenido,
                Fecha = DateTime.Now,
                Valoracion = dto.Valoracion,
                CreadoAt = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            // Validar con IValidatableObject
            var validationContext = new ValidationContext(testimonio);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(testimonio, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje, id) = await _testimonioDao.CrearTestimonioAsync(testimonio);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerTestimonioPorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Testimonio/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTestimonioPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, testimonio) = await _testimonioDao.ObtenerTestimonioPorIdAsync(id);

            if (exito && testimonio != null)
            {
                return Ok(new { exito = true, mensaje, data = testimonio });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Testimonio
        [HttpGet]
        public async Task<IActionResult> ListarTestimonios()
        {
            var (exito, mensaje, testimonios) = await _testimonioDao.ListarTestimoniosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = testimonios, total = testimonios?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Testimonio/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarTestimoniosPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos. Página debe ser > 0 y tamaño entre 1 y 100"
                });
            }

            var (exito, mensaje, testimonios, totalRegistros) =
                await _testimonioDao.ListarTestimoniosPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = testimonios,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Testimonio/buscar?termino=excelente&valoracion=5
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarTestimonios(
            [FromQuery] string? termino = null,
            [FromQuery] int? valoracion = null,
            [FromQuery] int? idUsuario = null)
        {
            if (valoracion.HasValue && (valoracion < 1 || valoracion > 5))
            {
                return BadRequest(new { exito = false, mensaje = "La valoración debe estar entre 1 y 5" });
            }

            var (exito, mensaje, testimonios) =
                await _testimonioDao.BuscarTestimoniosAsync(termino, valoracion, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = testimonios, total = testimonios?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Testimonio/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerTestimoniosPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, testimonios) =
                await _testimonioDao.ObtenerTestimoniosPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = testimonios, total = testimonios?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Testimonio/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _testimonioDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Testimonio/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarTestimonio(int id, [FromBody] TestimonioUpdateDto dto)
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

            // Verificar si existe
            var (existeExito, _, testimonioExistente) = await _testimonioDao.ObtenerTestimonioPorIdAsync(id);
            if (!existeExito || testimonioExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Testimonio no encontrado" });
            }

            // Actualizar campos
            testimonioExistente.Contenido = dto.Contenido;
            testimonioExistente.Valoracion = dto.Valoracion;
            testimonioExistente.ActualizadoAt = DateTime.Now;

            // Validar
            var validationContext = new ValidationContext(testimonioExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(testimonioExistente, validationContext, validationResults, true))
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            var (exito, mensaje) = await _testimonioDao.ActualizarTestimonioAsync(testimonioExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = testimonioExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Testimonio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTestimonio(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _testimonioDao.EliminarTestimonioAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // DTOs para las peticiones
    public class TestimonioCreateDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "El contenido debe tener entre 20 y 2000 caracteres")]
        public string Contenido { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5")]
        public int? Valoracion { get; set; }
    }

    public class TestimonioUpdateDto
    {
        [Required(ErrorMessage = "El contenido es obligatorio")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "El contenido debe tener entre 20 y 2000 caracteres")]
        public string Contenido { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5")]
        public int? Valoracion { get; set; }
    }
}
