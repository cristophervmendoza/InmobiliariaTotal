using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("solicitudpropiedad")]
    public class SolicitudPropiedad : IValidatableObject
    {
        [Key]
        [Column("idSolicitud")]
        public int IdSolicitud { get; set; }

        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "El título debe tener entre 10 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$", ErrorMessage = "El título contiene caracteres no permitidos")]
        [Column("titulo")]
        public string Titulo { get; set; }

        [StringLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres")]
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [StringLength(255, ErrorMessage = "La ruta de la foto no puede exceder los 255 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\-_\.\/\\:]+\.(jpg|jpeg|png|gif|webp|bmp)$", ErrorMessage = "La ruta de la foto no es válida")]
        [Column("fotoPropiedad")]
        public string FotoPropiedad { get; set; }

        [StringLength(50, ErrorMessage = "El estado de solicitud no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El estado contiene caracteres no permitidos")]
        [Column("solicitudEstado")]
        public string SolicitudEstado { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [NotMapped]
        public string NombreUsuario => Usuario?.Nombre;

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Titulo))
            {
                if (Titulo.Trim().Length < 10)
                {
                    yield return new ValidationResult("El título debe tener al menos 10 caracteres significativos", new[] { nameof(Titulo) });
                }

                if (Regex.IsMatch(Titulo, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El título no puede iniciar o terminar con espacios", new[] { nameof(Titulo) });
                }

                if (Regex.IsMatch(Titulo, @"\s{2,}"))
                {
                    yield return new ValidationResult("El título no puede contener múltiples espacios consecutivos", new[] { nameof(Titulo) });
                }

                if (Regex.IsMatch(Titulo, @"(.)\1{4,}"))
                {
                    yield return new ValidationResult("El título contiene caracteres repetidos sospechosos", new[] { nameof(Titulo) });
                }

                if (Titulo.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El título no puede estar completamente en mayúsculas", new[] { nameof(Titulo) });
                }

                if (Titulo.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El título no puede estar completamente en minúsculas", new[] { nameof(Titulo) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "spam", "fake" };
                if (palabrasProhibidas.Any(p => Titulo.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El título contiene palabras no permitidas", new[] { nameof(Titulo) });
                }

                if (Regex.IsMatch(Titulo, @"[<>{}[\]\\\/\|`~@#$%^&*]"))
                {
                    yield return new ValidationResult("El título contiene caracteres especiales no permitidos", new[] { nameof(Titulo) });
                }

                if (!Regex.IsMatch(Titulo, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{3,}"))
                {
                    yield return new ValidationResult("El título debe contener al menos una palabra válida", new[] { nameof(Titulo) });
                }

                var palabras = Titulo.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 2)
                {
                    yield return new ValidationResult("El título debe contener al menos 2 palabras", new[] { nameof(Titulo) });
                }

                if (palabras.Any(p => p.Length > 50))
                {
                    yield return new ValidationResult("El título contiene palabras excesivamente largas", new[] { nameof(Titulo) });
                }
            }

            if (!string.IsNullOrEmpty(Descripcion))
            {
                if (Descripcion.Trim().Length < 20)
                {
                    yield return new ValidationResult("La descripción debe tener al menos 20 caracteres significativos", new[] { nameof(Descripcion) });
                }

                if (Regex.IsMatch(Descripcion, @"^\s|\s$"))
                {
                    yield return new ValidationResult("La descripción no puede iniciar o terminar con espacios", new[] { nameof(Descripcion) });
                }

                if (Regex.IsMatch(Descripcion, @"\s{3,}"))
                {
                    yield return new ValidationResult("La descripción contiene demasiados espacios consecutivos", new[] { nameof(Descripcion) });
                }

                if (Regex.IsMatch(Descripcion, @"(.)\1{6,}"))
                {
                    yield return new ValidationResult("La descripción contiene caracteres repetidos sospechosos", new[] { nameof(Descripcion) });
                }

                if (Descripcion.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("La descripción no puede estar completamente en mayúsculas", new[] { nameof(Descripcion) });
                }

                if (Regex.IsMatch(Descripcion, @"[<>{}[\]\\|`~]"))
                {
                    yield return new ValidationResult("La descripción contiene caracteres de código no permitidos", new[] { nameof(Descripcion) });
                }

                if (!Regex.IsMatch(Descripcion, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{5,}"))
                {
                    yield return new ValidationResult("La descripción debe contener palabras válidas", new[] { nameof(Descripcion) });
                }

                var palabras = Descripcion.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 5)
                {
                    yield return new ValidationResult("La descripción debe contener al menos 5 palabras", new[] { nameof(Descripcion) });
                }

                if (palabras.Any(p => p.Length > 100))
                {
                    yield return new ValidationResult("La descripción contiene palabras excesivamente largas", new[] { nameof(Descripcion) });
                }

                var urlPattern = @"(http|https|ftp|www\.)";
                if (Regex.IsMatch(Descripcion, urlPattern, RegexOptions.IgnoreCase))
                {
                    yield return new ValidationResult("La descripción no debe contener URLs", new[] { nameof(Descripcion) });
                }

                var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                if (Regex.IsMatch(Descripcion, emailPattern))
                {
                    yield return new ValidationResult("La descripción no debe contener correos electrónicos", new[] { nameof(Descripcion) });
                }

                var telefonoPattern = @"\b\d{9}\b|\b\d{3}[-\s]?\d{3}[-\s]?\d{3}\b";
                if (Regex.IsMatch(Descripcion, telefonoPattern))
                {
                    yield return new ValidationResult("La descripción no debe contener números de teléfono", new[] { nameof(Descripcion) });
                }
            }

            if (!string.IsNullOrEmpty(FotoPropiedad))
            {
                if (Regex.IsMatch(FotoPropiedad, @"^\s|\s$"))
                {
                    yield return new ValidationResult("La ruta de la foto no puede iniciar o terminar con espacios", new[] { nameof(FotoPropiedad) });
                }

                if (FotoPropiedad.Contains(".."))
                {
                    yield return new ValidationResult("La ruta de la foto contiene referencias de directorio no permitidas", new[] { nameof(FotoPropiedad) });
                }

                var extensionesValidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                if (!extensionesValidas.Any(ext => FotoPropiedad.ToLower().EndsWith(ext)))
                {
                    yield return new ValidationResult("La extensión del archivo no es válida", new[] { nameof(FotoPropiedad) });
                }

                if (Regex.IsMatch(FotoPropiedad, @"[<>:""|?*]"))
                {
                    yield return new ValidationResult("La ruta de la foto contiene caracteres no permitidos en nombres de archivo", new[] { nameof(FotoPropiedad) });
                }

                var nombreArchivo = System.IO.Path.GetFileNameWithoutExtension(FotoPropiedad);
                if (nombreArchivo.Length < 3)
                {
                    yield return new ValidationResult("El nombre del archivo es demasiado corto", new[] { nameof(FotoPropiedad) });
                }

                if (nombreArchivo.Length > 100)
                {
                    yield return new ValidationResult("El nombre del archivo es demasiado largo", new[] { nameof(FotoPropiedad) });
                }

                if (Regex.IsMatch(nombreArchivo, @"(.)\1{5,}"))
                {
                    yield return new ValidationResult("El nombre del archivo contiene caracteres repetidos sospechosos", new[] { nameof(FotoPropiedad) });
                }
            }

            if (!string.IsNullOrEmpty(SolicitudEstado))
            {
                if (SolicitudEstado.Trim().Length < 3)
                {
                    yield return new ValidationResult("El estado debe tener al menos 3 caracteres significativos", new[] { nameof(SolicitudEstado) });
                }

                if (Regex.IsMatch(SolicitudEstado, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El estado no puede iniciar o terminar con espacios", new[] { nameof(SolicitudEstado) });
                }

                if (Regex.IsMatch(SolicitudEstado, @"\s{2,}"))
                {
                    yield return new ValidationResult("El estado no puede contener múltiples espacios consecutivos", new[] { nameof(SolicitudEstado) });
                }

                var estadosValidos = new[] { "pendiente", "en revision", "aprobada", "rechazada", "cancelada", "en proceso", "completada" };
                if (!estadosValidos.Any(e => e.Equals(SolicitudEstado.Trim().ToLower())))
                {
                    yield return new ValidationResult("El estado de la solicitud no es válido", new[] { nameof(SolicitudEstado) });
                }

                if (Regex.IsMatch(SolicitudEstado, @"\d"))
                {
                    yield return new ValidationResult("El estado no debe contener números", new[] { nameof(SolicitudEstado) });
                }
            }

            if (CreadoAt > DateTime.Now)
            {
                yield return new ValidationResult("La fecha de creación no puede ser futura", new[] { nameof(CreadoAt) });
            }

            if (CreadoAt < new DateTime(2000, 1, 1))
            {
                yield return new ValidationResult("La fecha de creación no es válida", new[] { nameof(CreadoAt) });
            }

            if (ActualizadoAt > DateTime.Now.AddMinutes(5))
            {
                yield return new ValidationResult("La fecha de actualización no puede ser futura", new[] { nameof(ActualizadoAt) });
            }

            if (ActualizadoAt < CreadoAt)
            {
                yield return new ValidationResult("La fecha de actualización no puede ser anterior a la fecha de creación", new[] { nameof(ActualizadoAt) });
            }

            var diferenciaDias = (ActualizadoAt - CreadoAt).TotalDays;
            if (diferenciaDias > 365)
            {
                yield return new ValidationResult("El rango entre las fechas parece inconsistente", new[] { nameof(ActualizadoAt), nameof(CreadoAt) });
            }

            if ((DateTime.Now - CreadoAt).TotalMinutes < -5)
            {
                yield return new ValidationResult("La fecha de creación parece ser inconsistente con la hora actual", new[] { nameof(CreadoAt) });
            }

            if (!string.IsNullOrEmpty(Titulo) && !string.IsNullOrEmpty(Descripcion))
            {
                if (Titulo.ToLower() == Descripcion.ToLower())
                {
                    yield return new ValidationResult("El título y la descripción no pueden ser idénticos", new[] { nameof(Titulo), nameof(Descripcion) });
                }

                if (Descripcion.ToLower().Contains(Titulo.ToLower()) && Titulo.Length > 20)
                {
                    yield return new ValidationResult("La descripción no debe contener el título completo", new[] { nameof(Descripcion) });
                }
            }
        }
    }
}
