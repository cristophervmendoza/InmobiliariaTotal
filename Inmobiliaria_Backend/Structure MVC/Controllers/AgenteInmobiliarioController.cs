using backend_csharpcd_inmo.Structure_MVC.DAO;
using backend_csharpcd_inmo.Structure_MVC.Models;
using Inmobiliaria_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenteInmobiliarioController : ControllerBase
    {
        private readonly AgenteInmobiliarioDao _agenteDao;
        private readonly UsuarioDao _usuarioDao;

        public AgenteInmobiliarioController(AgenteInmobiliarioDao agenteDao, UsuarioDao usuarioDao)
        {
            _agenteDao = agenteDao;
            _usuarioDao = usuarioDao;
        }

        // POST: api/AgenteInmobiliario
        [HttpPost]
        public async Task<IActionResult> CrearAgenteInmobiliario([FromBody] UsuarioRegistroDto dto)
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

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Dni = dto.Dni,
                Email = dto.Email.ToLower(),
                Telefono = dto.Telefono,
                IdEstadoUsuario = dto.IdEstadoUsuario,
                CreadoAt = DateTime.UtcNow,
                ActualizadoAt = DateTime.UtcNow
            };

            usuario.LimpiarNombre();
            usuario.NormalizarEmail();
            usuario.Password = usuario.HashPassword(dto.Password);

            var validationContext = new ValidationContext(usuario);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(usuario, validationContext, validationResults, true))
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
                usuario.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, idAgente, idUsuario) = await _agenteDao.CrearAgenteAsync(usuario);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerAgenteInmobiliario),
                    new { id = idAgente },
                    new { exito = true, mensaje, idAgente, idUsuario });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerAgenteInmobiliario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, agente) = await _agenteDao.ObtenerAgentePorIdAsync(id);

            if (exito && agente != null)
            {
                return Ok(new { exito = true, mensaje, data = agente });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerAgenteInmobiliarioPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, agente) = await _agenteDao.ObtenerAgentePorIdUsuarioAsync(idUsuario);

            if (exito && agente != null)
            {
                return Ok(new { exito = true, mensaje, data = agente });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario
        [HttpGet]
        public async Task<IActionResult> ListarAgentesInmobiliarios()
        {
            var (exito, mensaje, agentes) = await _agenteDao.ListarAgentesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agentes, total = agentes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarAgentesInmobiliariosPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, agentes, totalRegistros) =
                await _agenteDao.ListarAgentesPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = agentes,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario/buscar?termino=juan
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarAgentesInmobiliarios([FromQuery] string? termino = null)
        {
            var (exito, mensaje, agentes) = await _agenteDao.BuscarAgentesAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = agentes, total = agentes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/AgenteInmobiliario/verificar/{idUsuario}
        [HttpGet("verificar/{idUsuario}")]
        public async Task<IActionResult> VerificarEsAgenteInmobiliario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var esAgente = await _agenteDao.EsAgenteAsync(idUsuario);

            return Ok(new
            {
                exito = true,
                esAgente,
                mensaje = esAgente ? "El usuario es agente inmobiliario" : "El usuario no es agente inmobiliario"
            });
        }

        // GET: api/AgenteInmobiliario/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _agenteDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/AgenteInmobiliario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAgenteInmobiliario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _agenteDao.EliminarAgenteAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }
}
