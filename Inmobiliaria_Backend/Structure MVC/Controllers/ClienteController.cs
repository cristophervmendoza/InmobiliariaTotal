using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;
using Inmobiliaria_Backend.Services;


namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteDao _clienteDao;
        private readonly UsuarioDao _usuarioDao;

        public ClienteController(ClienteDao clienteDao, UsuarioDao usuarioDao)
        {
            _clienteDao = clienteDao;
            _usuarioDao = usuarioDao;
        }

        // POST: api/Cliente
        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] UsuarioRegistroDto dto)
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

            var (exito, mensaje, idCliente, idUsuario) = await _clienteDao.CrearClienteAsync(usuario);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerCliente),
                    new { id = idCliente },
                    new { exito = true, mensaje, idCliente, idUsuario });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cliente/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, cliente) = await _clienteDao.ObtenerClientePorIdAsync(id);

            if (exito && cliente != null)
            {
                return Ok(new { exito = true, mensaje, data = cliente });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Cliente/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerClientePorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, cliente) = await _clienteDao.ObtenerClientePorIdUsuarioAsync(idUsuario);

            if (exito && cliente != null)
            {
                return Ok(new { exito = true, mensaje, data = cliente });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Cliente
        [HttpGet]
        public async Task<IActionResult> ListarClientes()
        {
            var (exito, mensaje, clientes) = await _clienteDao.ListarClientesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = clientes, total = clientes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cliente/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarClientesPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, clientes, totalRegistros) =
                await _clienteDao.ListarClientesPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = clientes,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cliente/buscar?termino=juan
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarClientes([FromQuery] string? termino = null)
        {
            var (exito, mensaje, clientes) = await _clienteDao.BuscarClientesAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = clientes, total = clientes?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Cliente/verificar/{idUsuario}
        [HttpGet("verificar/{idUsuario}")]
        public async Task<IActionResult> VerificarEsCliente(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var esCliente = await _clienteDao.EsClienteAsync(idUsuario);

            return Ok(new
            {
                exito = true,
                esCliente,
                mensaje = esCliente ? "El usuario es cliente" : "El usuario no es cliente"
            });
        }

        // GET: api/Cliente/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _clienteDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Cliente/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _clienteDao.EliminarClienteAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }
}
