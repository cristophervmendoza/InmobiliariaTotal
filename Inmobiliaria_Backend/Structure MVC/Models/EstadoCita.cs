using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("estadocita")]
    public class EstadoCita : IValidatableObject
    {
        [Key]
        [Column("idEstadoCita")]
        public int IdEstadoCita { get; set; }

        [Required(ErrorMessage = "El nombre del estado es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        public virtual System.Collections.Generic.ICollection<Cita> Citas { get; set; }

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                if (Nombre.Trim().Length < 3)
                {
                    yield return new ValidationResult("El nombre debe tener al menos 3 caracteres significativos", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El nombre no puede iniciar o terminar con espacios", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"\s{2,}"))
                {
                    yield return new ValidationResult("El nombre no puede contener múltiples espacios consecutivos", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"(.)\1{3,}"))
                {
                    yield return new ValidationResult("El nombre contiene caracteres repetidos sospechosos", new[] { nameof(Nombre) });
                }

                if (Nombre.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El nombre no puede estar completamente en mayúsculas", new[] { nameof(Nombre) });
                }

                if (Nombre.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El nombre no puede estar completamente en minúsculas", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"[0-9]"))
                {
                    yield return new ValidationResult("El nombre no debe contener números", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"[<>{}[\]\\\/\|`~@#$%^&*()_+=!?.,;:""-]"))
                {
                    yield return new ValidationResult("El nombre contiene caracteres especiales no permitidos", new[] { nameof(Nombre) });
                }

                var estadosValidos = new[]
                {
                    "pendiente",
                    "confirmada",
                    "cancelada",
                    "completada",
                    "reprogramada",
                    "en proceso",
                    "no asistio",
                    "rechazada"
                };

                if (!estadosValidos.Any(e => e.Equals(Nombre.Trim().ToLower())))
                {
                    yield return new ValidationResult("El estado de la cita no es válido. Estados permitidos: Pendiente, Confirmada, Cancelada, Completada, Reprogramada, En Proceso, No Asistio, Rechazada", new[] { nameof(Nombre) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "temporal", "temp" };
                if (palabrasProhibidas.Any(p => Nombre.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El nombre contiene palabras no permitidas", new[] { nameof(Nombre) });
                }

                if (Nombre.Length < 5)
                {
                    yield return new ValidationResult("El nombre del estado es demasiado corto", new[] { nameof(Nombre) });
                }

                var palabras = Nombre.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length > 3)
                {
                    yield return new ValidationResult("El nombre del estado no debe tener más de 3 palabras", new[] { nameof(Nombre) });
                }

                if (palabras.Any(p => p.Length > 20))
                {
                    yield return new ValidationResult("El nombre contiene palabras excesivamente largas", new[] { nameof(Nombre) });
                }

                if (!Regex.IsMatch(Nombre, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{3,}"))
                {
                    yield return new ValidationResult("El nombre debe contener al menos una palabra válida de 3 o más letras", new[] { nameof(Nombre) });
                }

                var primeraLetra = Nombre.Trim()[0];
                if (!char.IsUpper(primeraLetra))
                {
                    yield return new ValidationResult("El nombre debe iniciar con mayúscula", new[] { nameof(Nombre) });
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

            if (CreadoAt > new DateTime(2020, 1, 1))
            {
                yield return new ValidationResult("Los estados de cita son datos de catálogo que deberían existir desde el inicio del sistema", new[] { nameof(CreadoAt) });
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
            if (diferenciaDias > 365 * 2)
            {
                yield return new ValidationResult("El rango entre las fechas parece inconsistente para un estado de catálogo", new[] { nameof(ActualizadoAt), nameof(CreadoAt) });
            }

            if ((DateTime.Now - CreadoAt).TotalMinutes < -5)
            {
                yield return new ValidationResult("La fecha de creación parece ser inconsistente con la hora actual", new[] { nameof(CreadoAt) });
            }

            if (CreadoAt.Year > DateTime.Now.Year)
            {
                yield return new ValidationResult("El año de creación no puede ser mayor al año actual", new[] { nameof(CreadoAt) });
            }

            if (ActualizadoAt.Year > DateTime.Now.Year)
            {
                yield return new ValidationResult("El año de actualización no puede ser mayor al año actual", new[] { nameof(ActualizadoAt) });
            }
        }
    }
}
