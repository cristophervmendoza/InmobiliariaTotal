using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("testimonio")]
    public class Testimonio : IValidatableObject
    {
        [Key]
        [Column("idTestimonio")]
        public int IdTestimonio { get; set; }

        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "El contenido debe tener entre 20 y 2000 caracteres")]
        [Column("contenido")]
        public string Contenido { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5")]
        [Column("valoracion")]
        public int? Valoracion { get; set; }

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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Contenido))
            {
                if (Contenido.Trim().Length < 20)
                {
                    yield return new ValidationResult("El contenido debe tener al menos 20 caracteres significativos", new[] { nameof(Contenido) });
                }

                if (Regex.IsMatch(Contenido, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El contenido no puede iniciar o terminar con espacios", new[] { nameof(Contenido) });
                }

                if (Regex.IsMatch(Contenido, @"\s{3,}"))
                {
                    yield return new ValidationResult("El contenido contiene demasiados espacios consecutivos", new[] { nameof(Contenido) });
                }

                if (Regex.IsMatch(Contenido, @"(.)\1{6,}"))
                {
                    yield return new ValidationResult("El contenido contiene caracteres repetidos sospechosos", new[] { nameof(Contenido) });
                }

                if (Contenido.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El contenido no puede estar completamente en mayúsculas", new[] { nameof(Contenido) });
                }

                if (Contenido.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El contenido no puede estar completamente en minúsculas", new[] { nameof(Contenido) });
                }

                if (Regex.IsMatch(Contenido, @"[<>{}[\]\\|`~]"))
                {
                    yield return new ValidationResult("El contenido contiene caracteres de código no permitidos", new[] { nameof(Contenido) });
                }

                if (!Regex.IsMatch(Contenido, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{5,}"))
                {
                    yield return new ValidationResult("El contenido debe contener palabras válidas", new[] { nameof(Contenido) });
                }

                var palabras = Contenido.Split(new[] { ' ', '\n', '\r', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 5)
                {
                    yield return new ValidationResult("El contenido debe contener al menos 5 palabras", new[] { nameof(Contenido) });
                }

                if (palabras.Any(p => p.Length > 100))
                {
                    yield return new ValidationResult("El contenido contiene palabras excesivamente largas", new[] { nameof(Contenido) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "spam", "fake", "bot" };
                if (palabrasProhibidas.Any(p => Contenido.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El contenido contiene palabras no permitidas", new[] { nameof(Contenido) });
                }

                var urlPattern = @"(http|https|ftp|www\.)";
                if (Regex.IsMatch(Contenido, urlPattern, RegexOptions.IgnoreCase))
                {
                    yield return new ValidationResult("El contenido no debe contener URLs", new[] { nameof(Contenido) });
                }

                var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                if (Regex.IsMatch(Contenido, emailPattern))
                {
                    yield return new ValidationResult("El contenido no debe contener correos electrónicos", new[] { nameof(Contenido) });
                }

                var telefonoPattern = @"\b\d{9}\b|\b\d{3}[-\s]?\d{3}[-\s]?\d{3}\b";
                if (Regex.IsMatch(Contenido, telefonoPattern))
                {
                    yield return new ValidationResult("El contenido no debe contener números de teléfono", new[] { nameof(Contenido) });
                }

                var palabrasOffensivas = new[] { "idiota", "estupido", "tonto", "basura", "horrible", "pesimo", "mierda" };
                var palabrasOffensivasEncontradas = palabrasOffensivas.Count(p => Contenido.ToLower().Contains(p));
                if (palabrasOffensivasEncontradas >= 2)
                {
                    yield return new ValidationResult("El contenido contiene lenguaje ofensivo excesivo", new[] { nameof(Contenido) });
                }

                var mayusculasConsecutivas = Regex.Matches(Contenido, @"[A-ZÁÉÍÓÚÑ]{10,}").Count;
                if (mayusculasConsecutivas > 2)
                {
                    yield return new ValidationResult("El contenido contiene demasiadas palabras en mayúsculas consecutivas", new[] { nameof(Contenido) });
                }

                var signos = Contenido.Count(c => c == '!' || c == '?');
                if (signos > 10)
                {
                    yield return new ValidationResult("El contenido contiene demasiados signos de exclamación o interrogación", new[] { nameof(Contenido) });
                }

                if (Regex.IsMatch(Contenido, @"[!?]{3,}"))
                {
                    yield return new ValidationResult("El contenido contiene signos de puntuación repetidos excesivamente", new[] { nameof(Contenido) });
                }

                var longitudPromedioPalabras = palabras.Length > 0 ? palabras.Average(p => p.Length) : 0;
                if (longitudPromedioPalabras < 2)
                {
                    yield return new ValidationResult("El contenido parece contener palabras demasiado cortas o sin sentido", new[] { nameof(Contenido) });
                }

                var vocales = Contenido.Count(c => "aeiouAEIOUáéíóúÁÉÍÓÚ".Contains(c));
                var consonantes = Contenido.Count(c => char.IsLetter(c) && !"aeiouAEIOUáéíóúÁÉÍÓÚ".Contains(c));
                if (consonantes > 0 && vocales / (double)consonantes < 0.2)
                {
                    yield return new ValidationResult("El contenido parece no tener sentido lingüístico", new[] { nameof(Contenido) });
                }
            }

            if (Valoracion.HasValue)
            {
                if (Valoracion < 1 || Valoracion > 5)
                {
                    yield return new ValidationResult("La valoración debe estar entre 1 y 5", new[] { nameof(Valoracion) });
                }

                if (!string.IsNullOrEmpty(Contenido))
                {
                    var palabrasPositivas = new[] { "excelente", "bueno", "genial", "increible", "perfecto", "recomiendo", "feliz", "satisfecho" };
                    var palabrasNegativas = new[] { "malo", "pesimo", "terrible", "horrible", "deficiente", "decepcion", "insatisfecho" };

                    var conteoPositivas = palabrasPositivas.Count(p => Contenido.ToLower().Contains(p));
                    var conteoNegativas = palabrasNegativas.Count(p => Contenido.ToLower().Contains(p));

                    if (Valoracion >= 4 && conteoNegativas > conteoPositivas && conteoNegativas >= 2)
                    {
                        yield return new ValidationResult("La valoración alta no coincide con el tono negativo del contenido", new[] { nameof(Valoracion), nameof(Contenido) });
                    }

                    if (Valoracion <= 2 && conteoPositivas > conteoNegativas && conteoPositivas >= 2)
                    {
                        yield return new ValidationResult("La valoración baja no coincide con el tono positivo del contenido", new[] { nameof(Valoracion), nameof(Contenido) });
                    }
                }
            }

            if (Fecha > DateTime.Now)
            {
                yield return new ValidationResult("La fecha no puede ser futura", new[] { nameof(Fecha) });
            }

            if (Fecha < new DateTime(2000, 1, 1))
            {
                yield return new ValidationResult("La fecha no es válida", new[] { nameof(Fecha) });
            }

            if ((DateTime.Now - Fecha).TotalDays > 365 * 5)
            {
                yield return new ValidationResult("La fecha del testimonio parece ser demasiado antigua", new[] { nameof(Fecha) });
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

            if (Fecha < CreadoAt.AddMinutes(-5))
            {
                yield return new ValidationResult("La fecha del testimonio no puede ser anterior a la fecha de creación", new[] { nameof(Fecha), nameof(CreadoAt) });
            }

            if (Fecha > CreadoAt.AddMinutes(5))
            {
                yield return new ValidationResult("La fecha del testimonio no puede ser posterior a la fecha de creación", new[] { nameof(Fecha), nameof(CreadoAt) });
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

            if (Fecha.Hour == 0 && Fecha.Minute == 0 && Fecha.Second == 0 && CreadoAt.Hour != 0)
            {
                yield return new ValidationResult("La fecha del testimonio parece haber sido manipulada", new[] { nameof(Fecha) });
            }
        }
    }
}
