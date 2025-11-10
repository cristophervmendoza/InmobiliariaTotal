using backend_csharpcd_inmo.Structure_MVC.DAO;
using backend_csharpcd_inmo.Structure_MVC.Models;
using Inmobiliaria_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdministradorController : ControllerBase
    {
        private readonly AdministradorDao _administradorDao;
        private readonly UsuarioDao _usuarioDao;
        private readonly IPasswordService _passwordService;

        public AdministradorController(
            AdministradorDao administradorDao,
            UsuarioDao usuarioDao,
            IPasswordService passwordService)
        {
            _administradorDao = administradorDao;
            _usuarioDao = usuarioDao;
            _passwordService = passwordService;
        }

        // POST: api/Administrador
        [HttpPost]
        public async Task<IActionResult> CrearAdministrador([FromBody] UsuarioRegistroDto dto)
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

            var (exito, mensaje, idAdministrador, idUsuario) = await _administradorDao.CrearAdministradorAsync(usuario);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerAdministrador),
                    new { id = idAdministrador },
                    new { exito = true, mensaje, idAdministrador, idUsuario });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Administrador/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarAdministrador(int id, [FromBody] UsuarioUpdateDto dto)
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

            // Obtener el administrador actual
            var (exitoGet, mensajeGet, adminActual) = await _administradorDao.ObtenerAdministradorPorIdAsync(id);

            if (!exitoGet || adminActual == null)
            {
                return NotFound(new { exito = false, mensaje = "Administrador no encontrado" });
            }

            // Obtener el usuario asociado
            var (exitoUsuario, mensajeUsuario, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(adminActual.IdUsuario);

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

        // PUT: api/Administrador/{id}/cambiar-password
        [HttpPut("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPasswordAdministrador(int id, [FromBody] CambiarPasswordDto dto)
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

            // Obtener el administrador
            var (exitoGet, mensajeGet, admin) = await _administradorDao.ObtenerAdministradorPorIdAsync(id);

            if (!exitoGet || admin == null)
            {
                return NotFound(new { exito = false, mensaje = "Administrador no encontrado" });
            }

            // Obtener el usuario asociado
            var (exitoUsuario, mensajeUsuario, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(admin.IdUsuario);

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

        // GET: api/Administrador/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerAdministrador(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, administrador) = await _administradorDao.ObtenerAdministradorPorIdAsync(id);

            if (exito && administrador != null)
            {
                return Ok(new { exito = true, mensaje, data = administrador });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Administrador/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerAdministradorPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, administrador) = await _administradorDao.ObtenerAdministradorPorIdUsuarioAsync(idUsuario);

            if (exito && administrador != null)
            {
                return Ok(new { exito = true, mensaje, data = administrador });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Administrador
        [HttpGet]
        public async Task<IActionResult> ListarAdministradores()
        {
            var (exito, mensaje, administradores) = await _administradorDao.ListarAdministradoresAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = administradores, total = administradores?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Administrador/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarAdministradoresPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, administradores, totalRegistros) =
                await _administradorDao.ListarAdministradoresPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = administradores,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Administrador/verificar/{idUsuario}
        [HttpGet("verificar/{idUsuario}")]
        public async Task<IActionResult> VerificarEsAdministrador(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var esAdministrador = await _administradorDao.EsAdministradorAsync(idUsuario);

            return Ok(new
            {
                exito = true,
                esAdministrador,
                mensaje = esAdministrador ? "El usuario es administrador" : "El usuario no es administrador"
            });
        }

        // GET: api/Administrador/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _administradorDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Administrador/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarAdministrador(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _administradorDao.EliminarAdministradorAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }
}
