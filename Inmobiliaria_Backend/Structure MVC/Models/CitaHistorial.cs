using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("citahistorial")]
    public class CitaHistorial : IValidatableObject
    {
        [Key]
        [Column("idHistorial")]
        public int IdHistorial { get; set; }

        [Required(ErrorMessage = "El ID de cita es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de cita debe ser mayor a 0")]
        [Column("idCita")]
        public int IdCita { get; set; }

        [Required(ErrorMessage = "El ID de estado de cita es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de estado debe ser mayor a 0")]
        [Column("citaEstadoId")]
        public int CitaEstadoId { get; set; }

        [StringLength(2000, ErrorMessage = "La observación no puede exceder los 2000 caracteres")]
        [Column("observacion")]
        public string Observacion { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [ForeignKey("IdCita")]
        public virtual Cita Cita { get; set; }

        [ForeignKey("CitaEstadoId")]
        public virtual EstadoCita EstadoCita { get; set; }

        [NotMapped]
        public string EstadoNombre => EstadoCita?.Nombre;

        [NotMapped]
        public DateTime? FechaCita => Cita?.Fecha;

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Observacion))
            {
                if (Observacion.Trim().Length < 5)
                {
                    yield return new ValidationResult("La observación debe tener al menos 5 caracteres significativos", new[] { nameof(Observacion) });
                }

                if (Regex.IsMatch(Observacion, @"^\s|\s$"))
                {
                    yield return new ValidationResult("La observación no puede iniciar o terminar con espacios", new[] { nameof(Observacion) });
                }

                if (Regex.IsMatch(Observacion, @"\s{3,}"))
                {
                    yield return new ValidationResult("La observación contiene demasiados espacios consecutivos", new[] { nameof(Observacion) });
                }

                if (Regex.IsMatch(Observacion, @"(.)\1{6,}"))
                {
                    yield return new ValidationResult("La observación contiene caracteres repetidos sospechosos", new[] { nameof(Observacion) });
                }

                if (Observacion.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("La observación no puede estar completamente en mayúsculas", new[] { nameof(Observacion) });
                }

                if (Observacion.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("La observación no puede estar completamente en minúsculas", new[] { nameof(Observacion) });
                }

                if (Regex.IsMatch(Observacion, @"[<>{}[\]\\|`~]"))
                {
                    yield return new ValidationResult("La observación contiene caracteres de código no permitidos", new[] { nameof(Observacion) });
                }

                if (!Regex.IsMatch(Observacion, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{3,}"))
                {
                    yield return new ValidationResult("La observación debe contener palabras válidas", new[] { nameof(Observacion) });
                }

                var palabras = Observacion.Split(new[] { ' ', '\n', '\r', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 2)
                {
                    yield return new ValidationResult("La observación debe contener al menos 2 palabras", new[] { nameof(Observacion) });
                }

                if (palabras.Any(p => p.Length > 100))
                {
                    yield return new ValidationResult("La observación contiene palabras excesivamente largas", new[] { nameof(Observacion) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "spam", "fake", "bot" };
                if (palabrasProhibidas.Any(p => Observacion.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("La observación contiene palabras no permitidas", new[] { nameof(Observacion) });
                }

                var urlPattern = @"(http|https|ftp|www\.)";
                if (Regex.IsMatch(Observacion, urlPattern, RegexOptions.IgnoreCase))
                {
                    yield return new ValidationResult("La observación no debe contener URLs", new[] { nameof(Observacion) });
                }

                var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                if (Regex.IsMatch(Observacion, emailPattern))
                {
                    yield return new ValidationResult("La observación no debe contener correos electrónicos", new[] { nameof(Observacion) });
                }

                var telefonoPattern = @"\b\d{9}\b|\b\d{3}[-\s]?\d{3}[-\s]?\d{3}\b";
                if (Regex.IsMatch(Observacion, telefonoPattern))
                {
                    yield return new ValidationResult("La observación no debe contener números de teléfono", new[] { nameof(Observacion) });
                }

                var mayusculasConsecutivas = Regex.Matches(Observacion, @"[A-ZÁÉÍÓÚÑ]{10,}").Count;
                if (mayusculasConsecutivas > 2)
                {
                    yield return new ValidationResult("La observación contiene demasiadas palabras en mayúsculas consecutivas", new[] { nameof(Observacion) });
                }

                var signos = Observacion.Count(c => c == '!' || c == '?');
                if (signos > 10)
                {
                    yield return new ValidationResult("La observación contiene demasiados signos de exclamación o interrogación", new[] { nameof(Observacion) });
                }

                if (Regex.IsMatch(Observacion, @"[!?]{3,}"))
                {
                    yield return new ValidationResult("La observación contiene signos de puntuación repetidos excesivamente", new[] { nameof(Observacion) });
                }

                var longitudPromedioPalabras = palabras.Length > 0 ? palabras.Average(p => p.Length) : 0;
                if (longitudPromedioPalabras < 2)
                {
                    yield return new ValidationResult("La observación parece contener palabras demasiado cortas o sin sentido", new[] { nameof(Observacion) });
                }

                var vocales = Observacion.Count(c => "aeiouAEIOUáéíóúÁÉÍÓÚ".Contains(c));
                var consonantes = Observacion.Count(c => char.IsLetter(c) && !"aeiouAEIOUáéíóúÁÉÍÓÚ".Contains(c));
                if (consonantes > 0 && vocales / (double)consonantes < 0.2)
                {
                    yield return new ValidationResult("La observación parece no tener sentido lingüístico", new[] { nameof(Observacion) });
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

            if ((DateTime.Now - CreadoAt).TotalMinutes < -5)
            {
                yield return new ValidationResult("La fecha de creación parece ser inconsistente con la hora actual", new[] { nameof(CreadoAt) });
            }

            if (CreadoAt.Hour < 8 || CreadoAt.Hour >= 20)
            {
                yield return new ValidationResult("Los registros de historial no pueden ser creados fuera del horario de oficina", new[] { nameof(CreadoAt) });
            }

            if (CreadoAt.Year > DateTime.Now.Year)
            {
                yield return new ValidationResult("El año de creación no puede ser mayor al año actual", new[] { nameof(CreadoAt) });
            }

            if (Cita != null)
            {
                if (CreadoAt < Cita.CreadoAt.AddMinutes(-5))
                {
                    yield return new ValidationResult("El historial no puede ser creado antes de la fecha de creación de la cita", new[] { nameof(CreadoAt) });
                }

                if (CreadoAt > Cita.Fecha.Add(Cita.Hora).AddDays(30))
                {
                    yield return new ValidationResult("El historial no puede ser creado más de 30 días después de la cita", new[] { nameof(CreadoAt) });
                }

                var fechaHoraCita = Cita.Fecha.Add(Cita.Hora);
                if (CreadoAt < fechaHoraCita.AddHours(-24) && EstadoCita?.Nombre?.ToLower() == "completada")
                {
                    yield return new ValidationResult("No se puede marcar como completada una cita que aún no ha ocurrido", new[] { nameof(CitaEstadoId) });
                }
            }

            if (IdCita <= 0)
            {
                yield return new ValidationResult("El ID de cita no es válido", new[] { nameof(IdCita) });
            }

            if (CitaEstadoId <= 0)
            {
                yield return new ValidationResult("El ID de estado de cita no es válido", new[] { nameof(CitaEstadoId) });
            }

            if (EstadoCita != null)
            {
                var estadosCriticos = new[] { "cancelada", "rechazada", "no asistio" };
                if (estadosCriticos.Any(e => EstadoCita.Nombre?.ToLower().Contains(e) == true))
                {
                    if (string.IsNullOrWhiteSpace(Observacion))
                    {
                        yield return new ValidationResult("Los cambios de estado críticos requieren una observación", new[] { nameof(Observacion) });
                    }
                    else if (Observacion.Length < 20)
                    {
                        yield return new ValidationResult("La observación para cambios críticos debe ser más detallada (mínimo 20 caracteres)", new[] { nameof(Observacion) });
                    }
                }
            }
        }
    }
}
