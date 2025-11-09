using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("mantenimiento")]
    public class Mantenimiento : IValidatableObject
    {
        [Key]
        [Column("idMantenimiento")]
        public int IdMantenimiento { get; set; }

        [Required(ErrorMessage = "El ID de propiedad es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de propiedad debe ser mayor a 0")]
        [Column("idPropiedad")]
        public int IdPropiedad { get; set; }

        [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El tipo debe tener entre 3 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$", ErrorMessage = "El tipo contiene caracteres no permitidos")]
        [Column("tipo")]
        public string Tipo { get; set; }

        [StringLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres")]
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El costo debe estar entre 0 y 999,999.99")]
        [Column("costo")]
        public decimal? Costo { get; set; }

        [Column("fechaProgramada")]
        public DateTime? FechaProgramada { get; set; }

        [Column("fechaRealizada")]
        public DateTime? FechaRealizada { get; set; }

        [Required(ErrorMessage = "El ID de estado de mantenimiento es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de estado debe ser mayor a 0")]
        [Column("idEstadoMantenimiento")]
        public int IdEstadoMantenimiento { get; set; }

        [Column("idUsuario")]
        public int? IdUsuario { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        [ForeignKey("IdPropiedad")]
        public virtual Propiedad Propiedad { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdEstadoMantenimiento")]
        public virtual EstadoMantenimiento EstadoMantenimiento { get; set; }

        [NotMapped]
        public string NombreUsuario => Usuario?.Nombre;

        [NotMapped]
        public string EstadoNombre => EstadoMantenimiento?.Nombre;

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Tipo))
            {
                if (Tipo.Trim().Length < 3)
                {
                    yield return new ValidationResult("El tipo debe tener al menos 3 caracteres significativos", new[] { nameof(Tipo) });
                }

                if (Regex.IsMatch(Tipo, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El tipo no puede iniciar o terminar con espacios", new[] { nameof(Tipo) });
                }

                if (Regex.IsMatch(Tipo, @"\s{2,}"))
                {
                    yield return new ValidationResult("El tipo no puede contener múltiples espacios consecutivos", new[] { nameof(Tipo) });
                }

                if (Regex.IsMatch(Tipo, @"(.)\1{4,}"))
                {
                    yield return new ValidationResult("El tipo contiene caracteres repetidos sospechosos", new[] { nameof(Tipo) });
                }

                if (Tipo.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El tipo no puede estar completamente en mayúsculas", new[] { nameof(Tipo) });
                }

                if (Tipo.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El tipo no puede estar completamente en minúsculas", new[] { nameof(Tipo) });
                }

                if (Regex.IsMatch(Tipo, @"[<>{}[\]\\\/\|`~@#$%^&*]"))
                {
                    yield return new ValidationResult("El tipo contiene caracteres especiales no permitidos", new[] { nameof(Tipo) });
                }

                if (!Regex.IsMatch(Tipo, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{3,}"))
                {
                    yield return new ValidationResult("El tipo debe contener al menos una palabra válida", new[] { nameof(Tipo) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx" };
                if (palabrasProhibidas.Any(p => Tipo.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El tipo contiene palabras no permitidas", new[] { nameof(Tipo) });
                }

                var tiposValidos = new[] { "preventivo", "correctivo", "predictivo", "emergencia", "inspeccion",
                                          "limpieza", "reparacion", "instalacion", "revision", "renovacion",
                                          "plomeria", "electricidad", "pintura", "jardineria", "fumigacion" };

                if (!tiposValidos.Any(t => Tipo.ToLower().Contains(t)))
                {
                    yield return new ValidationResult("El tipo de mantenimiento no parece ser válido", new[] { nameof(Tipo) });
                }

                if (Regex.IsMatch(Tipo, @"\d{3,}"))
                {
                    yield return new ValidationResult("El tipo no debe contener números excesivos", new[] { nameof(Tipo) });
                }
            }

            if (!string.IsNullOrEmpty(Descripcion))
            {
                if (Descripcion.Trim().Length < 10)
                {
                    yield return new ValidationResult("La descripción debe tener al menos 10 caracteres significativos", new[] { nameof(Descripcion) });
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

                var palabras = Descripcion.Split(new[] { ' ', '\n', '\r', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 3)
                {
                    yield return new ValidationResult("La descripción debe contener al menos 3 palabras", new[] { nameof(Descripcion) });
                }

                if (palabras.Any(p => p.Length > 100))
                {
                    yield return new ValidationResult("La descripción contiene palabras excesivamente largas", new[] { nameof(Descripcion) });
                }
            }

            if (Costo.HasValue)
            {
                if (Costo < 0)
                {
                    yield return new ValidationResult("El costo no puede ser negativo", new[] { nameof(Costo) });
                }

                if (Costo == 0)
                {
                    yield return new ValidationResult("El costo no puede ser cero", new[] { nameof(Costo) });
                }

                if (Costo > 999999.99m)
                {
                    yield return new ValidationResult("El costo excede el límite permitido", new[] { nameof(Costo) });
                }

                if (Costo < 10 && !string.IsNullOrEmpty(Tipo) && !Tipo.ToLower().Contains("inspeccion"))
                {
                    yield return new ValidationResult("El costo parece ser demasiado bajo para este tipo de mantenimiento", new[] { nameof(Costo) });
                }

                var decimales = (Costo.Value - Math.Floor(Costo.Value)) * 100;
                if (decimales > 0 && decimales % 1 != 0)
                {
                    yield return new ValidationResult("El costo tiene más de 2 decimales", new[] { nameof(Costo) });
                }

                if (Costo > 100000 && string.IsNullOrEmpty(Descripcion))
                {
                    yield return new ValidationResult("Los mantenimientos costosos requieren una descripción detallada", new[] { nameof(Descripcion) });
                }

                if (!string.IsNullOrEmpty(Tipo))
                {
                    if (Tipo.ToLower().Contains("limpieza") && Costo > 5000)
                    {
                        yield return new ValidationResult("El costo parece excesivo para mantenimiento de limpieza", new[] { nameof(Costo) });
                    }

                    if (Tipo.ToLower().Contains("inspeccion") && Costo > 3000)
                    {
                        yield return new ValidationResult("El costo parece excesivo para una inspección", new[] { nameof(Costo) });
                    }

                    if ((Tipo.ToLower().Contains("emergencia") || Tipo.ToLower().Contains("correctivo")) && Costo < 50)
                    {
                        yield return new ValidationResult("El costo parece bajo para mantenimiento de emergencia o correctivo", new[] { nameof(Costo) });
                    }
                }
            }

            if (FechaProgramada.HasValue)
            {
                if (FechaProgramada < new DateTime(2000, 1, 1))
                {
                    yield return new ValidationResult("La fecha programada no es válida", new[] { nameof(FechaProgramada) });
                }

                if (FechaProgramada > DateTime.Now.AddYears(5))
                {
                    yield return new ValidationResult("La fecha programada es demasiado lejana en el futuro", new[] { nameof(FechaProgramada) });
                }

                if (FechaProgramada < CreadoAt.AddMinutes(-5))
                {
                    yield return new ValidationResult("La fecha programada no puede ser anterior a la fecha de creación", new[] { nameof(FechaProgramada), nameof(CreadoAt) });
                }

                if (!string.IsNullOrEmpty(Tipo) && Tipo.ToLower().Contains("emergencia") && FechaProgramada > DateTime.Now.AddDays(7))
                {
                    yield return new ValidationResult("Los mantenimientos de emergencia deben programarse dentro de los próximos 7 días", new[] { nameof(FechaProgramada) });
                }
            }

            if (FechaRealizada.HasValue)
            {
                if (FechaRealizada > DateTime.Now.AddHours(1))
                {
                    yield return new ValidationResult("La fecha realizada no puede ser futura", new[] { nameof(FechaRealizada) });
                }

                if (FechaRealizada < new DateTime(2000, 1, 1))
                {
                    yield return new ValidationResult("La fecha realizada no es válida", new[] { nameof(FechaRealizada) });
                }

                if (FechaRealizada < CreadoAt.AddMinutes(-5))
                {
                    yield return new ValidationResult("La fecha realizada no puede ser anterior a la fecha de creación", new[] { nameof(FechaRealizada), nameof(CreadoAt) });
                }

                if (!Costo.HasValue || Costo == 0)
                {
                    yield return new ValidationResult("Un mantenimiento realizado debe tener un costo registrado", new[] { nameof(Costo) });
                }

                if (FechaProgramada.HasValue && FechaRealizada < FechaProgramada.Value.AddDays(-30))
                {
                    yield return new ValidationResult("La fecha realizada es significativamente anterior a la fecha programada", new[] { nameof(FechaRealizada), nameof(FechaProgramada) });
                }

                if (FechaProgramada.HasValue && FechaRealizada > FechaProgramada.Value.AddDays(90))
                {
                    yield return new ValidationResult("La fecha realizada es significativamente posterior a la fecha programada", new[] { nameof(FechaRealizada), nameof(FechaProgramada) });
                }

                if (!IdUsuario.HasValue)
                {
                    yield return new ValidationResult("Un mantenimiento realizado debe tener un usuario responsable asignado", new[] { nameof(IdUsuario) });
                }
            }

            if (FechaProgramada.HasValue && FechaRealizada.HasValue)
            {
                if (FechaRealizada < FechaProgramada.Value.AddDays(-1) && !string.IsNullOrEmpty(Tipo) && !Tipo.ToLower().Contains("emergencia"))
                {
                    yield return new ValidationResult("El mantenimiento se realizó mucho antes de la fecha programada", new[] { nameof(FechaRealizada) });
                }

                var diasDiferencia = (FechaRealizada.Value - FechaProgramada.Value).TotalDays;
                if (Math.Abs(diasDiferencia) > 365)
                {
                    yield return new ValidationResult("La diferencia entre fechas programada y realizada es excesiva", new[] { nameof(FechaProgramada), nameof(FechaRealizada) });
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
            if (diferenciaDias > 365 * 3)
            {
                yield return new ValidationResult("El rango entre las fechas parece inconsistente", new[] { nameof(ActualizadoAt), nameof(CreadoAt) });
            }

            if ((DateTime.Now - CreadoAt).TotalMinutes < -5)
            {
                yield return new ValidationResult("La fecha de creación parece ser inconsistente con la hora actual", new[] { nameof(CreadoAt) });
            }

            if (!string.IsNullOrEmpty(Tipo) && !string.IsNullOrEmpty(Descripcion))
            {
                if (Tipo.ToLower() == Descripcion.ToLower())
                {
                    yield return new ValidationResult("El tipo y la descripción no pueden ser idénticos", new[] { nameof(Tipo), nameof(Descripcion) });
                }
            }

            if (IdUsuario.HasValue && IdUsuario.Value <= 0)
            {
                yield return new ValidationResult("El ID de usuario no es válido", new[] { nameof(IdUsuario) });
            }

            if (FechaRealizada.HasValue && FechaProgramada.HasValue && string.IsNullOrEmpty(Descripcion))
            {
                yield return new ValidationResult("Los mantenimientos realizados requieren una descripción", new[] { nameof(Descripcion) });
            }
        }
    }
}
