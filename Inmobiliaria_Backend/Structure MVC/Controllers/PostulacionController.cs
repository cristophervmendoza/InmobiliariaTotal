using Microsoft.AspNetCore.Mvc;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.DAO;
using System.ComponentModel.DataAnnotations;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostulacionController : ControllerBase
    {
        private readonly PostulacionDao _postulacionDao;
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "cvs");

        public PostulacionController()
        {
            _postulacionDao = new PostulacionDao();

            // Crear carpeta de uploads si no existe
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        // POST: api/Postulacion
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CrearPostulacion([FromForm] PostulacionCreateDto dto)
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

            string? cvFilePath = null;

            // Procesar archivo CV si existe
            if (dto.CvFile != null && dto.CvFile.Length > 0)
            {
                var validacionArchivo = ValidarArchivo(dto.CvFile);
                if (!validacionArchivo.esValido)
                {
                    return BadRequest(new { exito = false, mensaje = validacionArchivo.mensaje });
                }

                try
                {
                    cvFilePath = await GuardarArchivoAsync(dto.CvFile);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar archivo: {ex.Message}" });
                }
            }

            var postulacion = new Postulacion
            {
                IdCliente = dto.IdCliente,
                Descripcion = dto.Descripcion,
                CvFile = cvFilePath,
                CreadoAt = DateTime.UtcNow,
                ActualizadoAt = DateTime.UtcNow
            };

            // Validar con IValidatableObject
            var validationContext = new ValidationContext(postulacion);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(postulacion, validationContext, validationResults, true))
            {
                // Eliminar archivo si la validación falla
                if (!string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
                {
                    System.IO.File.Delete(cvFilePath);
                }

                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            try
            {
                postulacion.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                // Eliminar archivo si la validación falla
                if (!string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
                {
                    System.IO.File.Delete(cvFilePath);
                }

                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje, id) = await _postulacionDao.CrearPostulacionAsync(postulacion);

            if (exito)
            {
                return CreatedAtAction(nameof(ObtenerPostulacionPorId),
                    new { id = id },
                    new { exito = true, mensaje, id });
            }

            // Eliminar archivo si falla la creación
            if (!string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
            {
                System.IO.File.Delete(cvFilePath);
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPostulacionPorId(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, mensaje, postulacion) = await _postulacionDao.ObtenerPostulacionPorIdAsync(id);

            if (exito && postulacion != null)
            {
                var response = new
                {
                    exito = true,
                    mensaje,
                    data = postulacion,
                    estadoPostulacion = postulacion.EstadoPostulacion,
                    tieneCV = postulacion.TieneCV,
                    extensionArchivo = postulacion.ExtensionArchivo,
                    esReciente = postulacion.EsPostulacionReciente(),
                    estaCompleta = postulacion.EstaCompleta(),
                    diasDesdePostulacion = postulacion.DiasDesdePostulacion()
                };
                return Ok(response);
            }

            return NotFound(new { exito = false, mensaje });
        }

        // GET: api/Postulacion
        [HttpGet]
        public async Task<IActionResult> ListarPostulaciones()
        {
            var (exito, mensaje, postulaciones) = await _postulacionDao.ListarPostulacionesAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = postulaciones, total = postulaciones?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/paginado?pagina=1&tamanoPagina=10
        [HttpGet("paginado")]
        public async Task<IActionResult> ListarPostulacionesPaginadas([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10)
        {
            if (pagina <= 0 || tamanoPagina <= 0 || tamanoPagina > 100)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Parámetros de paginación inválidos. Página debe ser > 0 y tamaño entre 1 y 100"
                });
            }

            var (exito, mensaje, postulaciones, totalRegistros) =
                await _postulacionDao.ListarPostulacionesPaginadasAsync(pagina, tamanoPagina);

            if (exito)
            {
                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanoPagina);
                return Ok(new
                {
                    exito = true,
                    mensaje,
                    data = postulaciones,
                    paginaActual = pagina,
                    tamanoPagina,
                    totalRegistros,
                    totalPaginas
                });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/buscar?termino=desarrollador&tieneCV=true
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPostulaciones(
            [FromQuery] string? termino = null,
            [FromQuery] int? idCliente = null,
            [FromQuery] bool? tieneCV = null)
        {
            var (exito, mensaje, postulaciones) =
                await _postulacionDao.BuscarPostulacionesAsync(termino, idCliente, tieneCV);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = postulaciones, total = postulaciones?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/cliente/{idCliente}
        [HttpGet("cliente/{idCliente}")]
        public async Task<IActionResult> ObtenerPostulacionesPorCliente(int idCliente)
        {
            if (idCliente <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID de cliente inválido" });
            }

            var (exito, mensaje, postulaciones) =
                await _postulacionDao.ObtenerPostulacionesPorClienteAsync(idCliente);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = postulaciones, total = postulaciones?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/recientes?dias=7
        [HttpGet("recientes")]
        public async Task<IActionResult> ObtenerPostulacionesRecientes([FromQuery] int dias = 7)
        {
            if (dias <= 0 || dias > 365)
            {
                return BadRequest(new { exito = false, mensaje = "El parámetro días debe estar entre 1 y 365" });
            }

            var (exito, mensaje, postulaciones) =
                await _postulacionDao.ObtenerPostulacionesRecientesAsync(dias);

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = postulaciones, total = postulaciones?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/completas
        [HttpGet("completas")]
        public async Task<IActionResult> ObtenerPostulacionesCompletas()
        {
            var (exito, mensaje, postulaciones) =
                await _postulacionDao.ObtenerPostulacionesCompletasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = postulaciones, total = postulaciones?.Count ?? 0 });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/estadisticas
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var (exito, mensaje, estadisticas) = await _postulacionDao.ObtenerEstadisticasAsync();

            if (exito)
            {
                return Ok(new { exito = true, mensaje, data = estadisticas });
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // GET: api/Postulacion/{id}/descargar-cv
        [HttpGet("{id}/descargar-cv")]
        public async Task<IActionResult> DescargarCV(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            var (exito, _, postulacion) = await _postulacionDao.ObtenerPostulacionPorIdAsync(id);

            if (!exito || postulacion == null)
            {
                return NotFound(new { exito = false, mensaje = "Postulación no encontrada" });
            }

            if (string.IsNullOrEmpty(postulacion.CvFile) || !System.IO.File.Exists(postulacion.CvFile))
            {
                return NotFound(new { exito = false, mensaje = "Archivo CV no encontrado" });
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(postulacion.CvFile, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = postulacion.ExtensionArchivo switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            return File(memory, contentType, Path.GetFileName(postulacion.CvFile));
        }

        // PUT: api/Postulacion/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ActualizarPostulacion(int id, [FromForm] PostulacionUpdateDto dto)
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

            var (existeExito, _, postulacionExistente) = await _postulacionDao.ObtenerPostulacionPorIdAsync(id);
            if (!existeExito || postulacionExistente == null)
            {
                return NotFound(new { exito = false, mensaje = "Postulación no encontrada" });
            }

            string? cvFilePath = postulacionExistente.CvFile;
            string? archivoAnterior = null;

            // Procesar nuevo archivo CV si existe
            if (dto.CvFile != null && dto.CvFile.Length > 0)
            {
                var validacionArchivo = ValidarArchivo(dto.CvFile);
                if (!validacionArchivo.esValido)
                {
                    return BadRequest(new { exito = false, mensaje = validacionArchivo.mensaje });
                }

                try
                {
                    archivoAnterior = cvFilePath;
                    cvFilePath = await GuardarArchivoAsync(dto.CvFile);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { exito = false, mensaje = $"Error al guardar archivo: {ex.Message}" });
                }
            }

            postulacionExistente.Descripcion = dto.Descripcion;
            postulacionExistente.CvFile = cvFilePath;
            postulacionExistente.ActualizarTiempos();

            var validationContext = new ValidationContext(postulacionExistente);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(postulacionExistente, validationContext, validationResults, true))
            {
                // Revertir cambio de archivo si la validación falla
                if (archivoAnterior != null && !string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
                {
                    System.IO.File.Delete(cvFilePath);
                }

                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Validación fallida",
                    errores = validationResults.Select(r => r.ErrorMessage)
                });
            }

            try
            {
                postulacionExistente.ValidarDatos();
            }
            catch (ArgumentException ex)
            {
                // Revertir cambio de archivo
                if (archivoAnterior != null && !string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
                {
                    System.IO.File.Delete(cvFilePath);
                }

                return BadRequest(new { exito = false, mensaje = ex.Message });
            }

            var (exito, mensaje) = await _postulacionDao.ActualizarPostulacionAsync(postulacionExistente);

            if (exito)
            {
                // Eliminar archivo anterior si la actualización fue exitosa
                if (archivoAnterior != null && !string.IsNullOrEmpty(archivoAnterior) && System.IO.File.Exists(archivoAnterior))
                {
                    System.IO.File.Delete(archivoAnterior);
                }

                return Ok(new { exito = true, mensaje, data = postulacionExistente });
            }

            // Revertir si falla la actualización
            if (archivoAnterior != null && !string.IsNullOrEmpty(cvFilePath) && System.IO.File.Exists(cvFilePath))
            {
                System.IO.File.Delete(cvFilePath);
            }

            return BadRequest(new { exito = false, mensaje });
        }

        // DELETE: api/Postulacion/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPostulacion(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { exito = false, mensaje = "ID inválido" });
            }

            // Obtener postulación para eliminar archivo
            var (existeExito, _, postulacion) = await _postulacionDao.ObtenerPostulacionPorIdAsync(id);

            var (exito, mensaje) = await _postulacionDao.EliminarPostulacionAsync(id);

            if (exito)
            {
                // Eliminar archivo CV si existe
                if (existeExito && postulacion != null &&
                    !string.IsNullOrEmpty(postulacion.CvFile) &&
                    System.IO.File.Exists(postulacion.CvFile))
                {
                    try
                    {
                        System.IO.File.Delete(postulacion.CvFile);
                    }
                    catch
                    {
                        // Ignorar error al eliminar archivo
                    }
                }

                return Ok(new { exito = true, mensaje });
            }

            return NotFound(new { exito = false, mensaje });
        }

        // Métodos privados auxiliares
        private (bool esValido, string mensaje) ValidarArchivo(IFormFile archivo)
        {
            // Validar tamaño
            var tamanoMaximoMB = Postulacion.TamanoMaximoCV;
            var tamanoMaximoBytes = tamanoMaximoMB * 1024 * 1024;
            if (archivo.Length > tamanoMaximoBytes)
            {
                return (false, $"El archivo no debe superar {tamanoMaximoMB} MB");
            }

            if (archivo.Length == 0)
            {
                return (false, "El archivo está vacío");
            }

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName)?.ToLower();
            var extensionesValidas = new[] { ".pdf", ".doc", ".docx" };
            if (string.IsNullOrEmpty(extension) || !extensionesValidas.Contains(extension))
            {
                return (false, "Solo se permiten archivos PDF, DOC o DOCX");
            }

            // Validar nombre de archivo
            var nombreArchivo = Path.GetFileNameWithoutExtension(archivo.FileName);
            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                return (false, "El nombre del archivo no es válido");
            }

            return (true, string.Empty);
        }

        private async Task<string> GuardarArchivoAsync(IFormFile archivo)
        {
            var nombreUnico = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var rutaCompleta = Path.Combine(_uploadFolder, nombreUnico);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return rutaCompleta;
        }
    }

    // DTOs para las peticiones
    public class PostulacionCreateDto
    {
        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdCliente debe ser mayor que cero")]
        public int IdCliente { get; set; }

        [StringLength(10000, MinimumLength = 50, ErrorMessage = "La descripción debe tener entre 50 y 10000 caracteres")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        public IFormFile? CvFile { get; set; }
    }

    public class PostulacionUpdateDto
    {
        [StringLength(10000, MinimumLength = 50, ErrorMessage = "La descripción debe tener entre 50 y 10000 caracteres")]
        [DataType(DataType.MultilineText)]
        public string? Descripcion { get; set; }

        public IFormFile? CvFile { get; set; }
    }
}
