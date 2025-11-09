using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoUsuarioController : ControllerBase
    {
        private readonly EstadoUsuarioDao _estadoUsuarioDao;

        public EstadoUsuarioController()
        {
            _estadoUsuarioDao = new EstadoUsuarioDao();
        }

        // POST: api/EstadoUsuario
        [HttpPost]
        public async Task<IActionResult> CrearEstadoUsuario([FromBody] EstadoUsuarioCreateDto dto)
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

            var estadoUsuario = new EstadoUsuario
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo
            };

            var (exito, mensaje, id) = await _estadoUsuarioDao.CrearEstadoUsuarioAsync(estadoUsuario);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerEstadoUsuario),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEstadoUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, estadoUsuario) = await _estadoUsuarioDao.ObtenerEstadoUsuarioPorIdAsync(id);

            if (exito && estadoUsuario != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoUsuario });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/nombre/{nombre}
        [HttpGet("nombre/{nombre}")]
        public async Task<IActionResult> ObtenerEstadoUsuarioPorNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new { exito = false, mensaje = "Nombre inválido" });
            }

            var (exito, mensaje, estadoUsuario) = await _estadoUsuarioDao.ObtenerEstadoUsuarioPorNombreAsync(nombre);

            if (exito && estadoUsuario != null)
            {
                return Ok(new { exito = true, mensaje, data = estadoUsuario });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario
        [HttpGet]
        public async Task<IActionResult> ListarEstadosUsuario()
        {
            var (exito, mensaje, estadosUsuario) = await _estadoUsuarioDao.ListarEstadosUsuarioAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosUsuario, total = estadosUsuario?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/activos
        [HttpGet("activos")]
        public async Task<IActionResult> ListarEstadosActivos()
        {
            var (exito, mensaje, estadosUsuario) = await _estadoUsuarioDao.ListarEstadosActivosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosUsuario, total = estadosUsuario?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/inactivos
        [HttpGet("inactivos")]
        public async Task<IActionResult> ListarEstadosInactivos()
        {
            var (exito, mensaje, estadosUsuario) = await _estadoUsuarioDao.ListarEstadosInactivosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosUsuario, total = estadosUsuario?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarEstadosUsuarioPaginados([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, estadosUsuario, totalRegistros) =
                await _estadoUsuarioDao.ListarEstadosUsuarioPaginadosAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = estadosUsuario,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/buscar?termino=activo
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarEstadosUsuario([FromQuery] string? termino = null)
        {
            var (exito, mensaje, estadosUsuario) = await _estadoUsuarioDao.BuscarEstadosUsuarioAsync(termino);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadosUsuario, total = estadosUsuario?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/{id}/usuarios/contar
        [HttpGet("{id}/usuarios/contar")]
        public async Task<IActionResult> ContarUsuariosPorEstado(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, total) = await _estadoUsuarioDao.ContarUsuariosPorEstadoAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, total });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _estadoUsuarioDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/EstadoUsuario/estadisticas/uso
        [HttpGet("estadisticas/uso")]
        public async Task<IActionResult> ObtenerEstadisticasUso()
        {
            var (exito, mensaje, estadisticas) = await _estadoUsuarioDao.ObtenerEstadisticasUsoAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // POST: api/EstadoUsuario/inicializar
        [HttpPost("inicializar")]
        public async Task<IActionResult> InicializarEstadosPredeterminados()
        {
            var (exito, mensaje) = await _estadoUsuarioDao.InicializarEstadosPredeterminadosAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/EstadoUsuario/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEstadoUsuario(int id, [FromBody] EstadoUsuarioUpdateDto dto)
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

            var (existeExito, _, estadoExistente) = await _estadoUsuarioDao.ObtenerEstadoUsuarioPorIdAsync(id);
            if (!existeExito || estadoExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Estado de usuario no encontrado" });
            }

            estadoExistente.Nombre = dto.Nombre;
            estadoExistente.Descripcion = dto.Descripcion;
            estadoExistente.Activo = dto.Activo;

            var (exito, mensaje) = await _estadoUsuarioDao.ActualizarEstadoUsuarioAsync(estadoExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadoExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/EstadoUsuario/{id}/activar
        [HttpPatch("{id}/activar")]
        public async Task<IActionResult> ActivarEstadoUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoUsuarioDao.ActivarEstadoUsuarioAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PATCH: api/EstadoUsuario/{id}/desactivar
        [HttpPatch("{id}/desactivar")]
        public async Task<IActionResult> DesactivarEstadoUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoUsuarioDao.DesactivarEstadoUsuarioAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/EstadoUsuario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEstadoUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _estadoUsuarioDao.EliminarEstadoUsuarioAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return BadRequest(new { exito = false, mensaje });
        }
    }

    // DTOs
    public class EstadoUsuarioCreateDto
    {
        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class EstadoUsuarioUpdateDto
    {
        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres")]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
