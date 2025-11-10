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
        private readonly IPasswordService _passwordService;

        public AgenteInmobiliarioController(
            AgenteInmobiliarioDao agenteDao,
            UsuarioDao usuarioDao,
            IPasswordService passwordService)
        {
            _agenteDao = agenteDao;
            _usuarioDao = usuarioDao;
            _passwordService = passwordService;
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
                IdEstadoUsuario = dto.IdEstadoUsuario ?? 1, // Default: activo
                CreadoAt = DateTime.UtcNow,
                ActualizadoAt = DateTime.UtcNow
            };

            usuario.LimpiarNombre();
            usuario.NormalizarEmail();

            // ✅ Usar BCrypt con IPasswordService
            usuario.Password = _passwordService.Hash(dto.Password);

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

        // PUT: api/AgenteInmobiliario/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarAgenteInmobiliario(int id, [FromBody] UsuarioUpdateDto dto)
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

            // Obtener el agente actual
            var (exitoGet, mensajeGet, agenteActual) = await _agenteDao.ObtenerAgentePorIdAsync(id);

            if (!exitoGet || agenteActual == null)
            {
                return NotFound(new { exito = false, mensaje = "Agente no encontrado" });
            }

            // Obtener el usuario asociado
            var (exitoUsuario, mensajeUsuario, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(agenteActual.IdUsuario);

            if (!exitoUsuario || usuario == null)
            {
                return NotFound(new { exito = false, mensaje = "Usuario asociado no encontrado" });
            }

            // Actualizar campos
            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email.ToLower();
            usuario.Telefono = dto.Telefono;
            usuario.IdEstadoUsuario = dto.IdEstadoUsuario ?? usuario.IdEstadoUsuario;
            usuario.ActualizadoAt = DateTime.UtcNow;

            usuario.LimpiarNombre();
            usuario.NormalizarEmail();

            try
            {
                usuario.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _usuarioDao.ActualizarUsuarioAsync(usuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/AgenteInmobiliario/{id}/cambiar-password
        [HttpPut("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPasswordAgente(int id, [FromBody] CambiarPasswordDto dto)
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

            // Obtener el agente
            var (exitoGet, mensajeGet, agente) = await _agenteDao.ObtenerAgentePorIdAsync(id);

            if (!exitoGet || agente == null)
            {
                return NotFound(new { exito = false, mensaje = "Agente no encontrado" });
            }

            // Obtener el usuario asociado
            var (exitoUsuario, mensajeUsuario, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(agente.IdUsuario);

            if (!exitoUsuario || usuario == null)
            {
                return NotFound(new { exito = false, mensaje = "Usuario asociado no encontrado" });
            }

            // ✅ Verificar contraseña actual con BCrypt
            if (!_passwordService.Verify(dto.PasswordActual, usuario.Password))
            {
                return BadRequest(new { exito = false, mensaje = "La contraseña actual es incorrecta" });
            }

            // Validar fuerza de la nueva contraseña
            if (!usuario.EsPasswordSegura(dto.NuevaPassword))
            {
                return BadRequest(new { exito = false, mensaje = "La nueva contraseña no cumple los requisitos de seguridad" });
            }

            // ✅ Hashear nueva contraseña con BCrypt
            var nuevoHash = _passwordService.Hash(dto.NuevaPassword);
            var (exito, mensaje) = await _usuarioDao.CambiarPasswordAsync(usuario.IdUsuario, nuevoHash);

            if (exito)
            {
                return Ok(new { exito = true, mensaje = "Contraseña actualizada correctamente" });
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
        // GET: api/AgenteInmobiliario
        [HttpGet]
        public async Task<IActionResult> ListarAgentesInmobiliarios()
        {
            var (exito, mensaje, agentes) = await _agenteDao.ListarAgentesAsync();

            if (exito && agentes != null)
            {
                // ✅ Aplanar los datos aquí
                var agentesDto = agentes.Select(a => new
                {
                    idAgenteInmobiliario = a.IdAgente,
                    idUsuario = a.IdUsuario,
                    nombre = a.Usuario?.Nombre ?? string.Empty,
                    dni = a.Usuario?.Dni ?? string.Empty,
                    email = a.Usuario?.Email ?? string.Empty,
                    telefono = a.Usuario?.Telefono,
                    fechaIngreso = a.Usuario?.CreadoAt ?? DateTime.MinValue,
                    idEstadoUsuario = a.Usuario?.IdEstadoUsuario ?? 1
                }).ToList();

                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = agentesDto,
                    total = agentesDto.Count
                });
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

        // PUT: api/AgenteInmobiliario/{id}/cambiar-estado
        [HttpPut("{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoAgente(int id, [FromBody] CambiarUsuarioEstadoDto dto)
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

            var (exito, mensaje) = await _agenteDao.CambiarEstadoAgenteAsync(id, dto.IdEstadoUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }










    }
}
