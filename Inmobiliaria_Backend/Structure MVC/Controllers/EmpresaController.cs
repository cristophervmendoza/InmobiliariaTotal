using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresaController : ControllerBase
    {
        private readonly EmpresaDao _empresaDao;

        public EmpresaController()
        {
            _empresaDao = new EmpresaDao();
        }

        // POST: api/Empresa
        [HttpPost]
        public async Task<IActionResult> CrearEmpresa([FromBody] EmpresaCreateDto dto)
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

            var empresa = new Empresa
            {
                IdUsuario = dto.IdUsuario,
                Nombre = dto.Nombre,
                Ruc = dto.Ruc,
                Direccion = dto.Direccion ?? string.Empty,
                Email = dto.Email ?? string.Empty,
                Telefono = dto.Telefono ?? string.Empty,
                TipoEmpresa = dto.TipoEmpresa ?? string.Empty,
                FechaRegistro = DateTime.Now,
                ActualizadoAt = DateTime.Now
            };

            var (exito, mensaje, id) = await _empresaDao.CrearEmpresaAsync(empresa);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerEmpresaPorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerEmpresaPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, empresa) = await _empresaDao.ObtenerEmpresaPorIdAsync(id);

            if (exito && empresa != null)
            {
                return Ok(new { exito = true, mensaje, data = empresa });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Empresa/ruc/{ruc}
        [HttpGet("ruc/{ruc}")]
        public async Task<IActionResult> ObtenerEmpresaPorRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != 11)
            {
                return BadRequest(new { exito = false, mensaje = "RUC inválido" });
            }

            var (exito, mensaje, empresa) = await _empresaDao.ObtenerEmpresaPorRucAsync(ruc);

            if (exito && empresa != null)
            {
                return Ok(new { exito = true, mensaje, data = empresa });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Empresa
        [HttpGet]
        public async Task<IActionResult> ListarEmpresas()
        {
            var (exito, mensaje, empresas) = await _empresaDao.ListarEmpresasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = empresas, total = empresas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarEmpresasPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos. Página debe ser > 0 y tamaño entre 1 y 100"
                });
            }

            var (exito, mensaje, empresas, totalRegistros) =
                await _empresaDao.ListarEmpresasPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = empresas,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/buscar?termino=empresa&tipoEmpresa=SAC
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarEmpresas(
            [FromQuery] string? termino = null,
            [FromQuery] string? tipoEmpresa = null,
            [FromQuery] int? idUsuario = null)
        {
            var (exito, mensaje, empresas) =
                await _empresaDao.BuscarEmpresasAsync(termino, tipoEmpresa, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = empresas, total = empresas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerEmpresasPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, empresas) =
                await _empresaDao.ObtenerEmpresasPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = empresas, total = empresas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/tipo/{tipoEmpresa}
        [HttpGet("tipo/{tipoEmpresa}")]
        public async Task<IActionResult> ObtenerEmpresasPorTipo(string tipoEmpresa)
        {
            if (string.IsNullOrWhiteSpace(tipoEmpresa))
            {
                return BadRequest(new { exito = false, mensaje = "Tipo de empresa inválido" });
            }

            var (exito, mensaje, empresas) =
                await _empresaDao.ObtenerEmpresasPorTipoAsync(tipoEmpresa);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = empresas, total = empresas?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _empresaDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Empresa/validar-ruc/{ruc}
        [HttpGet("validar-ruc/{ruc}")]
        public async Task<IActionResult> ValidarRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != 11)
            {
                return BadRequest(new { exito = false, mensaje = "RUC inválido, debe tener 11 dígitos" });
            }

            var existe = await _empresaDao.VerificarRucExisteAsync(ruc);

            return Ok(new
            {
                exito = true,
                existe,
                mensaje = existe ? "El RUC ya está registrado" : "El RUC está disponible"
            });
        }


        // PUT: api/Empresa/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEmpresa(int id, [FromBody] EmpresaUpdateDto dto)
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
            var (existeExito, _, empresaExistente) = await _empresaDao.ObtenerEmpresaPorIdAsync(id);
            if (!existeExito || empresaExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Empresa no encontrada" });
            }

            empresaExistente.IdUsuario = dto.IdUsuario; 
            empresaExistente.Nombre = dto.Nombre;
            empresaExistente.Ruc = dto.Ruc;
            empresaExistente.Direccion = dto.Direccion ?? string.Empty;
            empresaExistente.Email = dto.Email ?? string.Empty;
            empresaExistente.Telefono = dto.Telefono ?? string.Empty;
            empresaExistente.TipoEmpresa = dto.TipoEmpresa ?? string.Empty;
            empresaExistente.ActualizadoAt = DateTime.Now;

            var (exito, mensaje) = await _empresaDao.ActualizarEmpresaAsync(empresaExistente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = empresaExistente });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Empresa/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarEmpresa(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje) = await _empresaDao.EliminarEmpresaAsync(id);

            if (exito)
            {
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }
    }

    // ✅ DTOs CORREGIDOS - Solo valida formato básico, sin dígito verificador
    public class EmpresaCreateDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s\.\,\-&]+$", ErrorMessage = "El nombre contiene caracteres no permitidos")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener exactamente 11 dígitos")]
        [RegularExpression(@"^(10|15|17|20)\d{9}$", ErrorMessage = "El RUC debe empezar con 10, 15, 17 o 20 y tener 11 dígitos")]
        public string Ruc { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La dirección no puede exceder los 500 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-\#°\/]+$", ErrorMessage = "La dirección contiene caracteres no permitidos")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El email debe tener entre 5 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 dígitos")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe empezar con 9 y tener 9 dígitos")]
        public string? Telefono { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "El tipo de empresa debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$", ErrorMessage = "El tipo de empresa contiene caracteres no permitidos")]
        public string? TipoEmpresa { get; set; }
    }

    public class EmpresaUpdateDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s\.\,\-&]+$", ErrorMessage = "El nombre contiene caracteres no permitidos")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener exactamente 11 dígitos")]
        [RegularExpression(@"^(10|15|17|20)\d{9}$", ErrorMessage = "El RUC debe empezar con 10, 15, 17 o 20 y tener 11 dígitos")]
        public string Ruc { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La dirección no puede exceder los 500 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-\#°\/]+$", ErrorMessage = "La dirección contiene caracteres no permitidos")]
        public string? Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El email debe tener entre 5 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 dígitos")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe empezar con 9 y tener 9 dígitos")]
        public string? Telefono { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "El tipo de empresa debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$", ErrorMessage = "El tipo contiene caracteres no permitidos")]
        public string? TipoEmpresa { get; set; }
    }

}
