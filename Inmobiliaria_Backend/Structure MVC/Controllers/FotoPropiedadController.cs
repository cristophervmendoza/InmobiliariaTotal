using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FotoPropiedadController : ControllerBase
    {
        private readonly FotoPropiedadDao _fotoPropiedadDao;

        public FotoPropiedadController()
        {
            _fotoPropiedadDao = new FotoPropiedadDao();
        }

        // POST: api/FotoPropiedad
        [HttpPost]
        public async Task<IActionResult> CrearFotoPropiedad([FromBody] FotoPropiedadCreateDto dto)
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

            var fotoPropiedad = new FotoPropiedad
            {
                IdPropiedad = dto.IdPropiedad,
                RutaFoto = dto.RutaFoto,
                EsPrincipal = dto.EsPrincipal,
                Descripcion = dto.Descripcion,
                CreadoAt = DateTime.UtcNow
            };

            try
            {
                fotoPropiedad.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, id) = await _fotoPropiedadDao.CrearFotoPropiedadAsync(fotoPropiedad);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerFotoPropiedad),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerFotoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, foto) = await _fotoPropiedadDao.ObtenerFotoPropiedadPorIdAsync(id);

            if (exito && foto != null)
            {
                return Ok(new { exito = true, mensaje, data = foto });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad
        [HttpGet]
        public async Task<IActionResult> ListarFotosPropiedad()
        {
            var (exito, mensaje, fotos) = await _fotoPropiedadDao.ListarFotosPropiedadAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = fotos, total = fotos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/propiedad/{idPropiedad}
        [HttpGet("propiedad/{idPropiedad}")]
        public async Task<IActionResult> ObtenerFotosPorPropiedad(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, fotos) = await _fotoPropiedadDao.ObtenerFotosPorPropiedadAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = fotos, total = fotos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/propiedad/{idPropiedad}/principal
        [HttpGet("propiedad/{idPropiedad}/principal")]
        public async Task<IActionResult> ObtenerFotoPrincipal(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, foto) = await _fotoPropiedadDao.ObtenerFotoPrincipalAsync(idPropiedad);

            if (exito && foto != null)
            {
                return Ok(new { exito = true, mensaje, data = foto });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/propiedad/{idPropiedad}/secundarias
        [HttpGet("propiedad/{idPropiedad}/secundarias")]
        public async Task<IActionResult> ObtenerFotosSecundarias(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, fotos) = await _fotoPropiedadDao.ObtenerFotosSecundariasAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = fotos, total = fotos?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/propiedad/{idPropiedad}/contar
        [HttpGet("propiedad/{idPropiedad}/contar")]
        public async Task<IActionResult> ContarFotosPorPropiedad(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, total) = await _fotoPropiedadDao.ContarFotosPorPropiedadAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/FotoPropiedad/propiedad/{idPropiedad}/tiene-principal
        [HttpGet("propiedad/{idPropiedad}/tiene-principal")]
        public async Task<IActionResult> TieneFotoPrincipal(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var tienePrincipal = await _fotoPropiedadDao.TieneFotoPrincipalAsync(idPropiedad);

            return Ok(new
            {
                exito = true,
                tienePrincipal,
                mensaje = tienePrincipal ? "La propiedad tiene foto principal" : "La propiedad no tiene foto principal"
            });
        }

        // GET: api/FotoPropiedad/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _fotoPropiedadDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/FotoPropiedad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarFotoPropiedad(int id, [FromBody] FotoPropiedadUpdateDto dto)
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

            var (existeExito, _, fotoExistente) = await _fotoPropiedadDao.ObtenerFotoPropiedadPorIdAsync(id);
            if (!existeExito || fotoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Foto de propiedad no encontrada" });
            }

            fotoExistente.RutaFoto = dto.RutaFoto;
            fotoExistente.EsPrincipal = dto.EsPrincipal;
            fotoExistente.Descripcion = dto.Descripcion;

            try
            {
                fotoExistente.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _fotoPropiedadDao.ActualizarFotoPropiedadAsync(fotoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = fotoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/FotoPropiedad/{id}/marcar-principal
        [HttpPatch("{id}/marcar-principal")]
        public async Task<IActionResult> MarcarComoPrincipal(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _fotoPropiedadDao.MarcarComoPrincipalAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/FotoPropiedad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarFotoPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _fotoPropiedadDao.EliminarFotoPropiedadAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // DELETE: api/FotoPropiedad/propiedad/{idPropiedad}
        [HttpDelete("propiedad/{idPropiedad}")]
        public async Task<IActionResult> EliminarFotosPorPropiedad(int idPropiedad)
        {
            if (idPropiedad <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de propiedad inválido" });
            }

            var (exito, mensaje, cantidad) = await _fotoPropiedadDao.EliminarFotosPorPropiedadAsync(idPropiedad);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, fotosEliminadas = cantidad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // POST: api/FotoPropiedad/multiple
        [HttpPost("multiple")]
        public async Task<IActionResult> CrearMultiplesFotos([FromBody] FotoPropiedadMultipleCreateDto dto)
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

            if (dto.Fotos == null || !dto.Fotos.Any())
            {
                return BadRequest(new { exito = false, mensaje = "Debe proporcionar al menos una foto" });
            }

            var resultados = new List<object>();
            int exitosas = 0;
            int fallidas = 0;

            foreach (var fotoDto in dto.Fotos)
            {
                var fotoPropiedad = new FotoPropiedad
                {
                    IdPropiedad = dto.IdPropiedad,
                    RutaFoto = fotoDto.RutaFoto,
                    EsPrincipal = fotoDto.EsPrincipal,
                    Descripcion = fotoDto.Descripcion,
                    CreadoAt = DateTime.UtcNow
                };

                try
                {
                    fotoPropiedad.ValidarDatos();
                    var (exito, mensaje, id) = await _fotoPropiedadDao.CrearFotoPropiedadAsync(fotoPropiedad);

                    if (exito)
                    {
                        exitosas++;
                        resultados.Add(new { exito = true, ruta = fotoDto.RutaFoto, id, mensaje });
                    }
                    else
                    {
                        fallidas++;
                        resultados.Add(new { exito = false, ruta = fotoDto.RutaFoto, mensaje });
                    }
                }
                catch (ArgumentException ex)
                {
                    fallidas++;
                    resultados.Add(new { exito = false, ruta = fotoDto.RutaFoto, mensaje = ex.Message });
                }
            }

            return Ok(new
            {
                exito = true,
                mensaje = $"{exitosas} foto(s) creada(s) exitosamente, {fallidas} fallida(s)",
                exitosas,
                fallidas,
                resultados
            });
        }
    }

    // DTOs
    public class FotoPropiedadCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdPropiedad { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 5)]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(jpg|jpeg|png|webp))$")]
        public string RutaFoto { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; } = false;

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-]*$")]
        public string? Descripcion { get; set; }
    }

    public class FotoPropiedadUpdateDto
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(jpg|jpeg|png|webp))$")]
        public string RutaFoto { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; } = false;

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-]*$")]
        public string? Descripcion { get; set; }
    }

    public class FotoItemDto
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(jpg|jpeg|png|webp))$")]
        public string RutaFoto { get; set; } = string.Empty;

        public bool EsPrincipal { get; set; } = false;

        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-]*$")]
        public string? Descripcion { get; set; }
    }

    public class FotoPropiedadMultipleCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int IdPropiedad { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe proporcionar al menos una foto")]
        public List<FotoItemDto> Fotos { get; set; } = new List<FotoItemDto>();
    }
}
