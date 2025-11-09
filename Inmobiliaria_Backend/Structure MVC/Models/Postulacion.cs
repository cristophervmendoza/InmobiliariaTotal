using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("postulacion")]
    public class Postulacion : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPostulacion { get; set; }

        [Required(ErrorMessage = "El cliente es obligatorio.")]
        [ForeignKey("Cliente")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdCliente debe ser un número válido mayor que cero.")]
        public int IdCliente { get; set; }

        [StringLength(10000, MinimumLength = 50, ErrorMessage = "La descripción debe tener entre 50 y 10000 caracteres.")]
        [DataType(DataType.MultilineText)]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-():\n\r¿?¡!@#%&*+=;""']+$",
            ErrorMessage = "La descripción contiene caracteres no válidos.")]
        public string? Descripcion { get; set; }

        [StringLength(255, MinimumLength = 5, ErrorMessage = "La ruta del CV debe tener entre 5 y 255 caracteres.")]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(pdf|doc|docx))$",
            ErrorMessage = "El formato del CV no es válido. Solo se permiten archivos PDF, DOC o DOCX.")]
        public string? CvFile { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ActualizadoAt { get; set; } = DateTime.UtcNow;

        [InverseProperty("Postulaciones")]
        public virtual Cliente? Cliente { get; set; }

        [NotMapped]
        public string EstadoPostulacion
        {
            get
            {
                var diasDesdeCreacion = (DateTime.UtcNow - CreadoAt).Days;
                if (diasDesdeCreacion <= 7) return "Reciente";
                if (diasDesdeCreacion <= 30) return "En Proceso";
                if (diasDesdeCreacion <= 90) return "En Revisión";
                return "Antigua";
            }
        }

        [NotMapped]
        public bool TieneCV => !string.IsNullOrWhiteSpace(CvFile);

        [NotMapped]
        public string ExtensionArchivo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CvFile))
                    return string.Empty;

                return Path.GetExtension(CvFile)?.ToLower() ?? string.Empty;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (ActualizadoAt < CreadoAt)
            {
                results.Add(new ValidationResult(
                    "La fecha de actualización no puede ser anterior a la fecha de creación.",
                    new[] { nameof(ActualizadoAt) }
                ));
            }

            if (CreadoAt > DateTime.UtcNow.AddMinutes(1))
            {
                results.Add(new ValidationResult(
                    "La fecha de creación no puede ser futura.",
                    new[] { nameof(CreadoAt) }
                ));
            }

            if (ActualizadoAt > DateTime.UtcNow.AddMinutes(1))
            {
                results.Add(new ValidationResult(
                    "La fecha de actualización no puede ser futura.",
                    new[] { nameof(ActualizadoAt) }
                ));
            }

            if (!string.IsNullOrEmpty(Descripcion))
            {
                var descripcionLimpia = Regex.Replace(Descripcion, @"[\s\W_]+", "");
                if (descripcionLimpia.Length < 20)
                {
                    results.Add(new ValidationResult(
                        "La descripción debe contener al menos 20 caracteres alfanuméricos válidos.",
                        new[] { nameof(Descripcion) }
                    ));
                }

                if (Regex.IsMatch(Descripcion, @"(http|https|www\.|\.com|\.net|\.org)", RegexOptions.IgnoreCase))
                {
                    results.Add(new ValidationResult(
                        "La descripción no debe contener enlaces o URLs.",
                        new[] { nameof(Descripcion) }
                    ));
                }

                if (Regex.IsMatch(Descripcion, @"(.)\1{9,}"))
                {
                    results.Add(new ValidationResult(
                        "La descripción contiene caracteres repetidos excesivamente.",
                        new[] { nameof(Descripcion) }
                    ));
                }

                var letras = Descripcion.Where(char.IsLetter).ToList();
                if (letras.Count > 0)
                {
                    var proporcionMayusculas = (double)letras.Count(char.IsUpper) / letras.Count;
                    if (proporcionMayusculas > 0.7)
                    {
                        results.Add(new ValidationResult(
                            "La descripción tiene demasiadas letras en mayúsculas. Use un formato apropiado.",
                            new[] { nameof(Descripcion) }
                        ));
                    }
                }

                var palabras = Descripcion.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 10)
                {
                    results.Add(new ValidationResult(
                        "La descripción debe contener al menos 10 palabras significativas.",
                        new[] { nameof(Descripcion) }
                    ));
                }
            }

            if (!string.IsNullOrEmpty(CvFile))
            {
                var extension = Path.GetExtension(CvFile)?.ToLower();
                var extensionesValidas = new[] { ".pdf", ".doc", ".docx" };

                if (!extensionesValidas.Contains(extension))
                {
                    results.Add(new ValidationResult(
                        "El archivo CV debe ser de tipo PDF, DOC o DOCX.",
                        new[] { nameof(CvFile) }
                    ));
                }

                if (Regex.IsMatch(CvFile, @"[<>:""|?*]"))
                {
                    results.Add(new ValidationResult(
                        "El nombre del archivo contiene caracteres no permitidos.",
                        new[] { nameof(CvFile) }
                    ));
                }

                var nombreSinExtension = Path.GetFileNameWithoutExtension(CvFile);
                if (!string.IsNullOrEmpty(nombreSinExtension) && nombreSinExtension.Contains('.'))
                {
                    results.Add(new ValidationResult(
                        "El archivo no debe tener doble extensión por seguridad.",
                        new[] { nameof(CvFile) }
                    ));
                }
            }

            if (CreadoAt < DateTime.UtcNow.AddYears(-2))
            {
                results.Add(new ValidationResult(
                    "La postulación no puede tener más de 2 años de antigüedad.",
                    new[] { nameof(CreadoAt) }
                ));
            }

            return results;
        }

        public void ValidarDatos()
        {
            if (IdCliente <= 0)
                throw new ArgumentException("El IdCliente debe ser mayor que cero.");

            if (!string.IsNullOrEmpty(Descripcion))
            {
                if (Descripcion.Length < 50 || Descripcion.Length > 10000)
                    throw new ArgumentException("La descripción debe tener entre 50 y 10000 caracteres.");

                if (string.IsNullOrWhiteSpace(Descripcion))
                    throw new ArgumentException("La descripción no puede contener solo espacios en blanco.");

                var descripcionLimpia = Regex.Replace(Descripcion, @"[\s\W_]+", "");
                if (descripcionLimpia.Length < 20)
                    throw new ArgumentException("La descripción debe contener al menos 20 caracteres alfanuméricos válidos.");

                if (Regex.IsMatch(Descripcion, @"(http|https|www\.|\.com|\.net|\.org)", RegexOptions.IgnoreCase))
                    throw new ArgumentException("La descripción no debe contener enlaces o URLs.");

                if (Regex.IsMatch(Descripcion, @"(.)\1{9,}"))
                    throw new ArgumentException("La descripción contiene caracteres repetidos excesivamente.");

                var palabras = Descripcion.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 10)
                    throw new ArgumentException("La descripción debe contener al menos 10 palabras significativas.");

                if (!Regex.IsMatch(Descripcion, @"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-():\n\r¿?¡!@#%&*+=;""']+$"))
                    throw new ArgumentException("La descripción contiene caracteres no válidos.");
            }

            if (!string.IsNullOrEmpty(CvFile))
            {
                if (CvFile.Length < 5 || CvFile.Length > 255)
                    throw new ArgumentException("La ruta del CV debe tener entre 5 y 255 caracteres.");

                var extension = Path.GetExtension(CvFile)?.ToLower();
                var extensionesValidas = new[] { ".pdf", ".doc", ".docx" };

                if (string.IsNullOrEmpty(extension) || !extensionesValidas.Contains(extension))
                    throw new ArgumentException("El archivo CV debe ser de tipo PDF, DOC o DOCX.");

                if (!Regex.IsMatch(CvFile, @"^[\w,\s\-\/\\]+(\.(pdf|doc|docx))$", RegexOptions.IgnoreCase))
                    throw new ArgumentException("El formato de ruta del CV no es válido.");

                if (Regex.IsMatch(CvFile, @"[<>:""|?*]"))
                    throw new ArgumentException("El nombre del archivo contiene caracteres no permitidos.");

                var nombreSinExtension = Path.GetFileNameWithoutExtension(CvFile);
                if (!string.IsNullOrEmpty(nombreSinExtension) && nombreSinExtension.Contains('.'))
                    throw new ArgumentException("El archivo no debe tener doble extensión por seguridad.");
            }

            if (CreadoAt > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de creación no puede ser futura.");

            if (ActualizadoAt < CreadoAt)
                throw new ArgumentException("La fecha de actualización no puede ser anterior a la fecha de creación.");

            if (ActualizadoAt > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de actualización no puede ser futura.");

            if (CreadoAt < DateTime.UtcNow.AddYears(-2))
                throw new ArgumentException("La postulación no puede tener más de 2 años de antigüedad.");
        }

        public void ActualizarTiempos()
        {
            ActualizadoAt = DateTime.UtcNow;
        }

        public bool TieneCVValido()
        {
            if (string.IsNullOrWhiteSpace(CvFile))
                return false;

            var extension = Path.GetExtension(CvFile)?.ToLower();
            var extensionesValidas = new[] { ".pdf", ".doc", ".docx" };

            return !string.IsNullOrEmpty(extension) && extensionesValidas.Contains(extension);
        }

        public static int TamanoMaximoCV => 5;

        public bool EsPostulacionReciente()
        {
            return (DateTime.UtcNow - CreadoAt).Days <= 7;
        }

        public bool EstaCompleta()
        {
            return !string.IsNullOrWhiteSpace(Descripcion) &&
                   Descripcion.Length >= 50 &&
                   TieneCVValido();
        }

        public int DiasDesdePostulacion()
        {
            return (DateTime.UtcNow - CreadoAt).Days;
        }

        public string ObtenerResumen(int maxCaracteres = 100)
        {
            if (string.IsNullOrWhiteSpace(Descripcion))
                return string.Empty;

            if (Descripcion.Length <= maxCaracteres)
                return Descripcion;

            return Descripcion.Substring(0, maxCaracteres) + "...";
        }

        public void LimpiarDescripcion()
        {
            if (!string.IsNullOrEmpty(Descripcion))
            {
                Descripcion = Regex.Replace(Descripcion, @"\s+", " ");
                Descripcion = Descripcion.Trim();
            }
        }
    }
}
