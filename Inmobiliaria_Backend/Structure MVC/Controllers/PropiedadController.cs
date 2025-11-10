using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropiedadController : ControllerBase
    {
        private readonly PropiedadDao _propiedadDao;
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "propiedades");

        public PropiedadController()
        {
            _propiedadDao = new PropiedadDao();

            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        // POST: api/Propiedad
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearPropiedad([FromForm] PropiedadCreateDto dto)
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

            string? fotoPath = null;

            if (dto.FotoPropiedad != null && dto.FotoPropiedad.Length > 0)
            {
                var validacion = ValidarImagen(dto.FotoPropiedad);
                if (!validacion.esValida)
                {
                    return BadRequest(new { exito = false, mensaje = validacion.mensaje });
                }

                try
                {
                    fotoPath = await GuardarImagenAsync(dto.FotoPropiedad);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar imagen: {ex.Message}" });
                }
            }

            var propiedad = new Propiedad
            {
                IdUsuario = dto.IdUsuario,
                IdTipoPropiedad = dto.IdTipoPropiedad,
                IdEstadoPropiedad = dto.IdEstadoPropiedad,
                Titulo = dto.Titulo,
                Direccion = dto.Direccion,
                Precio = dto.Precio,
                Descripcion = dto.Descripcion,
                AreaTerreno = dto.AreaTerreno,
                TipoMoneda = dto.TipoMoneda,
                Habitacion = dto.Habitacion,
                Bano = dto.Bano,
                Estacionamiento = dto.Estacionamiento,
                FotoPropiedad = fotoPath,
                CreadoAt = DateTime.UtcNow,
                ActualizadoAt = DateTime.UtcNow
            };

            try
            {
                propiedad.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                if (!string.IsNullOrEmpty(fotoPath) && System.IO.File.Exists(fotoPath))
                {
                    System.IO.File.Delete(fotoPath);
                }
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, id) = await _propiedadDao.CrearPropiedadAsync(propiedad);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerPropiedadPorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            if (!string.IsNullOrEmpty(fotoPath) && System.IO.File.Exists(fotoPath))
            {
                System.IO.File.Delete(fotoPath);
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPropiedadPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, propiedad) = await _propiedadDao.ObtenerPropiedadPorIdAsync(id);

            if (exito && propiedad != null)
            {
                return Ok(new { exito = true, mensaje, data = propiedad });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Propiedad
        [HttpGet]
        public async Task<IActionResult> ListarPropiedades()
        {
            var (exito, mensaje, propiedades) = await _propiedadDao.ListarPropiedadesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = propiedades, total = propiedades?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarPropiedadesPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos"
                });
            }

            var (exito, mensaje, propiedades, totalRegistros) =
                await _propiedadDao.ListarPropiedadesPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = propiedades,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/buscar
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPropiedades(
            [FromQuery] string? termino = null,
            [FromQuery] int? idTipoPropiedad = null,
            [FromQuery] int? idEstadoPropiedad = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] int? habitacionesMin = null,
            [FromQuery] string? tipoMoneda = null,
            [FromQuery] int? idUsuario = null)
        {
            var (exito, mensaje, propiedades) =
                await _propiedadDao.BuscarPropiedadesAsync(termino, idTipoPropiedad, idEstadoPropiedad,
                                                          precioMin, precioMax, habitacionesMin,
                                                          tipoMoneda, idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = propiedades, total = propiedades?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/usuario/{idUsuario}
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObtenerPropiedadesPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de usuario inválido" });
            }

            var (exito, mensaje, propiedades) =
                await _propiedadDao.ObtenerPropiedadesPorUsuarioAsync(idUsuario);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = propiedades, total = propiedades?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/precio?precioMin=100000&precioMax=500000&tipoMoneda=PEN
        [HttpGet("precio")]
        public async Task<IActionResult> ObtenerPropiedadesPorRangoPrecio(
            [FromQuery] decimal precioMin,
            [FromQuery] decimal precioMax,
            [FromQuery] string? tipoMoneda = null)
        {
            if (precioMin < 0 || precioMax < 0 || precioMin > precioMax)
            {
                return BadRequest(new { exito = false, mensaje = "Rango de precios inválido" });
            }

            var (exito, mensaje, propiedades) =
                await _propiedadDao.ObtenerPropiedadesPorRangoPrecioAsync(precioMin, precioMax, tipoMoneda);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = propiedades, total = propiedades?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Propiedad/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _propiedadDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // PUT: api/Propiedad/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ActualizarPropiedad(int id, [FromForm] PropiedadUpdateDto dto)
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

            // Obtener propiedad existente
            var (existeExito, _, propiedadExistente) = await _propiedadDao.ObtenerPropiedadPorIdAsync(id);
            if (!existeExito || propiedadExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Propiedad no encontrada" });
            }

            string? fotoPath = propiedadExistente.FotoPropiedad;
            string? archivoAnterior = null;

            // Manejo de nueva foto
            if (dto.FotoPropiedad != null && dto.FotoPropiedad.Length > 0)
            {
                var validacion = ValidarImagen(dto.FotoPropiedad);
                if (!validacion.esValida)
                {
                    return BadRequest(new { exito = false, mensaje = validacion.mensaje });
                }

                try
                {
                    archivoAnterior = fotoPath;
                    fotoPath = await GuardarImagenAsync(dto.FotoPropiedad);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar imagen: {ex.Message}" });
                }
            }

            // ✅ ACTUALIZAR: Cambiar IdUsuario si se proporciona
            if (dto.IdUsuario.HasValue && dto.IdUsuario.Value > 0)
            {
                propiedadExistente.IdUsuario = dto.IdUsuario.Value;
                Console.WriteLine($"✅ Actualizando IdUsuario a: {dto.IdUsuario.Value}");
            }

            // Actualizar otros campos
            propiedadExistente.IdTipoPropiedad = dto.IdTipoPropiedad;
            propiedadExistente.IdEstadoPropiedad = dto.IdEstadoPropiedad;
            propiedadExistente.Titulo = dto.Titulo;
            propiedadExistente.Direccion = dto.Direccion;
            propiedadExistente.Precio = dto.Precio;
            propiedadExistente.Descripcion = dto.Descripcion;
            propiedadExistente.AreaTerreno = dto.AreaTerreno;
            propiedadExistente.TipoMoneda = dto.TipoMoneda;
            propiedadExistente.Habitacion = dto.Habitacion;
            propiedadExistente.Bano = dto.Bano;
            propiedadExistente.Estacionamiento = dto.Estacionamiento;
            propiedadExistente.FotoPropiedad = fotoPath;
            propiedadExistente.ActualizarTiempos(); // o propiedadExistente.ActualizadoAt = DateTime.UtcNow;

            // Validar datos
            try
            {
                propiedadExistente.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                // Revertir foto nueva si falló validación
                if (archivoAnterior != null && !string.IsNullOrEmpty(fotoPath) && System.IO.File.Exists(fotoPath))
                {
                    System.IO.File.Delete(fotoPath);
                }
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            // Actualizar en BD
            var (exito, mensaje) = await _propiedadDao.ActualizarPropiedadAsync(propiedadExistente);

            if (exito)
            {
                // Eliminar foto antigua si todo salió bien
                if (archivoAnterior != null && !string.IsNullOrEmpty(archivoAnterior) && System.IO.File.Exists(archivoAnterior))
                {
                    System.IO.File.Delete(archivoAnterior);
                }
                return Ok(new { exito = true, mensaje, data = propiedadExistente });
            }

            // Si falló el update, eliminar la nueva foto
            if (archivoAnterior != null && !string.IsNullOrEmpty(fotoPath) && System.IO.File.Exists(fotoPath))
            {
                System.IO.File.Delete(fotoPath);
            }

            return BadRequest(new { exito = false, mensaje });
        }


        // DELETE: api/Propiedad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPropiedad(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (existeExito, _, propiedad) = await _propiedadDao.ObtenerPropiedadPorIdAsync(id);

            var (exito, mensaje) = await _propiedadDao.EliminarPropiedadAsync(id);

            if (exito)
            {
                if (existeExito && propiedad != null &&
                    !string.IsNullOrEmpty(propiedad.FotoPropiedad) &&
                    System.IO.File.Exists(propiedad.FotoPropiedad))
                {
                    try
                    {
                        System.IO.File.Delete(propiedad.FotoPropiedad);
                    }
                    catch { }
                }
                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // Métodos auxiliares
        private (bool esValida, string mensaje) ValidarImagen(IFormFile imagen)
        {
            const long tamanoMaximo = 5 * 1024 * 1024; // 5MB
            if (imagen.Length > tamanoMaximo)
            {
                return (false, "La imagen no debe superar 5 MB");
            }

            if (imagen.Length == 0)
            {
                return (false, "La imagen está vacía");
            }

            var extension = Path.GetExtension(imagen.FileName)?.ToLower();
            var extensionesValidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(extension) || !extensionesValidas.Contains(extension))
            {
                return (false, "Solo se permiten imágenes JPG, JPEG, PNG o WEBP");
            }

            return (true, string.Empty);
        }

        private async Task<string> GuardarImagenAsync(IFormFile imagen)
        {
            var nombreUnico = $"{Guid.NewGuid()}{Path.GetExtension(imagen.FileName)}";
            var rutaCompleta = Path.Combine(_uploadFolder, nombreUnico);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return rutaCompleta;
        }
    }

    // DTOs
    public class PropiedadCreateDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Range(1, int.MaxValue)]
        public int IdUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdTipoPropiedad { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoPropiedad { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999999.99)]
        public decimal Precio { get; set; }

        [StringLength(5000)]
        public string? Descripcion { get; set; }

        [Range(0.0, 1000000.0)]
        public decimal? AreaTerreno { get; set; }

        [Required]
        [RegularExpression("^(PEN|USD|EUR)$")]
        public string TipoMoneda { get; set; } = "PEN";

        [Range(0, 50)]
        public int? Habitacion { get; set; }

        [Range(0, 50)]
        public int? Bano { get; set; }

        [Range(0, 50)]
        public int? Estacionamiento { get; set; }

        public IFormFile? FotoPropiedad { get; set; }
    }

    public class PropiedadUpdateDto
    {
        // ✅ AGREGADO: Permitir cambiar el agente
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        public int? IdUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdTipoPropiedad { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IdEstadoPropiedad { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999999.99)]
        public decimal Precio { get; set; }

        [StringLength(5000)]
        public string? Descripcion { get; set; }

        [Range(0.0, 1000000.0)]
        public decimal? AreaTerreno { get; set; }

        [Required]
        [RegularExpression("^(PEN|USD|EUR)$")]
        public string TipoMoneda { get; set; } = "PEN";

        [Range(0, 50)]
        public int? Habitacion { get; set; }

        [Range(0, 50)]
        public int? Bano { get; set; }

        [Range(0, 50)]
        public int? Estacionamiento { get; set; }

        public IFormFile? FotoPropiedad { get; set; }
    }

}
