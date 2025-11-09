using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;
using Inmobiliaria_Backend.Services;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioDao _usuarioDao;
        private readonly IPasswordService _pwd;

        public UsuarioController(UsuarioDao usuarioDao, IPasswordService pwd)
        {
            _usuarioDao = usuarioDao;
            _pwd = pwd;
        }

        // POST: api/Usuario/registro
        [HttpPost("registro")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] UsuarioRegistroDto dto)
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

            // Hash de contraseña con BCrypt
            usuario.Password = _pwd.Hash(dto.Password);

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

            var (exito, mensaje, id) = await _usuarioDao.CrearUsuarioAsync(usuario);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerUsuarioPorId),
                    new { id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/Usuario/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
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

            // El DAO trae el usuario (con Password hash) y rol
            var (exito, mensaje, usuario, rol) = await _usuarioDao.LoginAsync(dto.Email, dto.Password);

            if (usuario != null)
            {
                // Evita excepciones: solo verifica si el hash parece BCrypt
                var passwordOk = _pwd.Verify(dto.Password, usuario.Password);

                if (exito && passwordOk)
                {
                    // Rehash oportunista si aumentaste el cost
                    if (_pwd.NeedsRehash(usuario.Password))
                    {
                        var nuevoHash = _pwd.Hash(dto.Password);
                        await _usuarioDao.CambiarPasswordAsync(usuario.IdUsuario, nuevoHash);
                    }

                    await _usuarioDao.ActualizarLoginExitosoAsync(usuario.IdUsuario);

                    return Ok(new
                    {
                        exito = true,
                        mensaje = "Login exitoso",
                        usuario = new
                        {
                            usuario.IdUsuario,
                            usuario.Nombre,
                            usuario.Email,
                            usuario.Dni,
                            usuario.Telefono,
                            usuario.NombreCorto,
                            usuario.Iniciales,
                            usuario.IdEstadoUsuario,
                            rol
                        }
                    });
                }
                else
                {
                    await _usuarioDao.RegistrarIntentoFallidoAsync(usuario.IdUsuario);
                    var intentosRestantes = 5 - (usuario.IntentosLogin + 1);
                    return Unauthorized(new
                    {
                        exito = false,
                        mensaje = mensaje?.Contains("bloqueado") == true
                                  ? mensaje
                                  : $"Credenciales inválidas. Intentos restantes: {intentosRestantes}"
                    });
                }
            }

            return Unauthorized(new { exito = false, mensaje = mensaje ?? "Usuario no encontrado" });
        }

        // GET: api/Usuario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerUsuarioPorId(int id)
        {
            if (id <= 0)
                return BadRequest(new { exito = false, mensaje = "ID inválido" });

            var (exito, mensaje, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(id);

            if (exito && usuario != null)
            {
                var response = new
                {
                    exito = true,
                    mensaje,
                    data = new
                    {
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Dni,
                        usuario.Email,
                        usuario.Telefono,
                        usuario.IntentosLogin,
                        usuario.IdEstadoUsuario,
                        usuario.UltimoLoginAt,
                        usuario.CreadoAt,
                        usuario.ActualizadoAt,
                        usuario.EstaBloqueado,
                        usuario.RequiereCambioPassword,
                        usuario.EsNuevo,
                        usuario.DiasDesdeUltimoLogin,
                        usuario.NombreCorto,
                        usuario.Iniciales,
                        esActivo = usuario.EsUsuarioActivo(),
                        esInactivo = usuario.EsUsuarioInactivo()
                    }
                };
                return Ok(response);
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Usuario/email/{email}
        [HttpGet("email/{email}")]
        public async Task<IActionResult> ObtenerUsuarioPorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { exito = false, mensaje = "Email inválido" });

            var (exito, mensaje, usuario) = await _usuarioDao.ObtenerUsuarioPorEmailAsync(email);

            if (exito && usuario != null)
            {
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = new
                    {
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Email,
                        usuario.Telefono,
                        usuario.IdEstadoUsuario
                    }
                });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Usuario/dni/{dni}
        [HttpGet("dni/{dni}")]
        public async Task<IActionResult> ObtenerUsuarioPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni) || dni.Length != 8)
                return BadRequest(new { exito = false, mensaje = "DNI inválido" });

            var (exito, mensaje, usuario) = await _usuarioDao.ObtenerUsuarioPorDniAsync(dni);

            if (exito && usuario != null)
            {
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = new
                    {
                        usuario.IdUsuario,
                        usuario.Nombre,
                        usuario.Dni,
                        usuario.Email,
                        usuario.Telefono
                    }
                });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Usuario
        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var (exito, mensaje, usuarios) = await _usuarioDao.ListarUsuariosAsync();

            if (exito)
                return Ok(new { exito = true, mensaje, data = usuarios, total = usuarios?.Count ?? 0 });

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Usuario/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarUsuariosPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new { exito = false, mensaje = "Parámetros de paginación inválidos" });
            }

            var (exito, mensaje, usuarios, totalRegistros) =
                await _usuarioDao.ListarUsuariosPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = usuarios,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Usuario/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarUsuarios(
            [FromQuery] string? termino = null,
            [FromQuery] bool? bloqueados = null,
            [FromQuery] int? idEstado = null)
        {
            var (exito, mensaje, usuarios) =
                await _usuarioDao.BuscarUsuariosAsync(termino, bloqueados, idEstado);

            if (exito)
                return Ok(new { exito = true, mensaje, data = usuarios, total = usuarios?.Count ?? 0 });

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Usuario/bloqueados
        [HttpGet("bloqueados")]
        public async Task<IActionResult> ObtenerUsuariosBloqueados()
        {
            var (exito, mensaje, usuarios) = await _usuarioDao.ObtenerUsuariosBloqueadosAsync();

            if (exito)
                return Ok(new { exito = true, mensaje, data = usuarios, total = usuarios?.Count ?? 0 });

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Usuario/inactivos?diasInactividad=90
        [HttpGet("inactivos")]
        public async Task<IActionResult> ObtenerUsuariosInactivos([FromQuery] int diasInactividad = 90)
        {
            if (diasInactividad <= 0)
                return BadRequest(new { exito = false, mensaje = "Días de inactividad inválido" });

            var (exito, mensaje, usuarios) =
                await _usuarioDao.ObtenerUsuariosInactivosAsync(diasInactividad);

            if (exito)
                return Ok(new { exito = true, mensaje, data = usuarios, total = usuarios?.Count ?? 0 });

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Usuario/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _usuarioDao.ObtenerEstadisticasAsync();

            if (exito)
                return Ok(new { exito = true, mensaje, data = estadisticas });

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Usuario/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (id <= 0)
                return BadRequest(new { exito = false, mensaje = "ID inválido" });

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Datos inválidos",
                    errores = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var (existeExito, _, usuarioExistente) = await _usuarioDao.ObtenerUsuarioPorIdAsync(id);
            if (!existeExito || usuarioExistente == null)
                return NotFound(new { exito = false, mensaje = "Usuario no encontrado" });

            usuarioExistente.Nombre = dto.Nombre;
            usuarioExistente.Email = dto.Email.ToLower();
            usuarioExistente.Telefono = dto.Telefono;
            usuarioExistente.IdEstadoUsuario = dto.IdEstadoUsuario;
            usuarioExistente.ActualizarTiempos();

            usuarioExistente.LimpiarNombre();
            usuarioExistente.NormalizarEmail();

            try { usuarioExistente.ValidarDatos(); }
            catch (ArgumentException ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _usuarioDao.ActualizarUsuarioAsync(usuarioExistente);

            if (exito)
                return Ok(new { exito = true, mensaje, data = usuarioExistente });

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Usuario/{id}/cambiar-password
        [HttpPut("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordDto dto)
        {
            if (id <= 0)
                return BadRequest(new { exito = false, mensaje = "ID inválido" });

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Datos inválidos",
                    errores = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var (existeExito, _, usuario) = await _usuarioDao.ObtenerUsuarioPorIdAsync(id);
            if (!existeExito || usuario == null)
                return NotFound(new { exito = false, mensaje = "Usuario no encontrado" });

            // Verificar password actual (BCrypt) con validación defensiva
            var passwordActualOk = _pwd.Verify(dto.PasswordActual, usuario.Password);
            if (!passwordActualOk)
                return BadRequest(new { exito = false, mensaje = "Contraseña actual incorrecta" });

            if (!usuario.EsPasswordSegura(dto.NuevaPassword))
                return BadRequest(new { exito = false, mensaje = "La nueva contraseña no cumple los requisitos de seguridad" });

            var passwordHash = _pwd.Hash(dto.NuevaPassword);
            var (exito, mensaje) = await _usuarioDao.CambiarPasswordAsync(id, passwordHash);

            if (exito)
                return Ok(new { exito = true, mensaje });

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Usuario/{id}/desbloquear
        [HttpPut("{id}/desbloquear")]
        public async Task<IActionResult> DesbloquearUsuario(int id)
        {
            if (id <= 0)
                return BadRequest(new { exito = false, mensaje = "ID inválido" });

            var (exito, mensaje) = await _usuarioDao.DesbloquearUsuarioAsync(id);
            if (exito)
                return Ok(new { exito = true, mensaje });

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Usuario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (id <= 0)
                return BadRequest(new { exito = false, mensaje = "ID inválido" });

            var (exito, mensaje) = await _usuarioDao.EliminarUsuarioAsync(id);
            if (exito)
                return Ok(new { exito = true, mensaje });

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Usuario/validar-fuerza-password?password=ejemplo123
        [HttpGet("validar-fuerza-password")]
        public IActionResult ValidarFuerzaPassword([FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return BadRequest(new { exito = false, mensaje = "Password inválido" });

            var usuario = new Usuario();
            var fuerza = usuario.CalcularFuerzaPassword(password);
            var esSegura = usuario.EsPasswordSegura(password);

            return Ok(new
            {
                exito = true,
                fuerza,
                esSegura,
                nivel = fuerza < 40 ? "Débil" : fuerza < 70 ? "Media" : "Fuerte"
            });
        }
    }

    // DTOs (sin cambios)
    public class UsuarioRegistroDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(8, MinimumLength = 8)]
        [RegularExpression(@"^[0-9]{8}$")]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 5)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9)]
        [RegularExpression(@"^9[0-9]{8}$")]
        public string? Telefono { get; set; }

        [Range(1, int.MaxValue)]
        public int? IdEstadoUsuario { get; set; }
    }

    public class UsuarioUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 5)]
        public string Email { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9)]
        [RegularExpression(@"^9[0-9]{8}$")]
        public string? Telefono { get; set; }

        [Range(1, int.MaxValue)]
        public int? IdEstadoUsuario { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class CambiarPasswordDto
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required]
        [StringLength(255, MinimumLength = 8)]
        public string NuevaPassword { get; set; } = string.Empty;
    }
}
