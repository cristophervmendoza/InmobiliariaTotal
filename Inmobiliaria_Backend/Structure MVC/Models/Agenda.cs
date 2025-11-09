using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("agenda")]
    public class Agenda : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAgenda { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [ForeignKey("Usuario")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdUsuario debe ser un número válido mayor que cero.")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-()]+$", ErrorMessage = "El título contiene caracteres no válidos.")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El tipo debe tener entre 2 y 100 caracteres.")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s.\-]*$", ErrorMessage = "El tipo solo puede contener letras, espacios, puntos y guiones.")]
        public string? Tipo { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 caracteres.")]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y contener exactamente 9 dígitos numéricos.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "La fecha y hora del evento es obligatoria.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaHora { get; set; }

        [StringLength(5000, MinimumLength = 10, ErrorMessage = "La descripción del evento debe tener entre 10 y 5000 caracteres.")]
        [DataType(DataType.MultilineText)]
        public string? DescripcionEvento { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El estado debe tener entre 3 y 50 caracteres.")]
        [RegularExpression(@"^(Pendiente|Confirmado|Cancelado|Completado|En Proceso|Reagendado)$",
            ErrorMessage = "El estado debe ser: Pendiente, Confirmado, Cancelado, Completado, En Proceso o Reagendado.")]
        public string? Estado { get; set; } = "Pendiente";

        [StringLength(200, MinimumLength = 5, ErrorMessage = "La ubicación debe tener entre 5 y 200 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-#°/]+$", ErrorMessage = "La ubicación contiene caracteres no válidos.")]
        public string? Ubicacion { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ActualizadoAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TipoPrioridad")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdTipoPrioridad debe ser un número válido mayor que cero.")]
        public int? IdTipoPrioridad { get; set; }

        [InverseProperty("Agendas")]
        public virtual Usuario? Usuario { get; set; }

        [InverseProperty("Agendas")]
        public virtual TipoPrioridad? TipoPrioridad { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (FechaHora < DateTime.UtcNow.AddYears(-1))
            {
                results.Add(new ValidationResult(
                    "La fecha del evento no puede ser anterior a un año desde hoy.",
                    new[] { nameof(FechaHora) }
                ));
            }

            if (FechaHora > DateTime.UtcNow.AddYears(5))
            {
                results.Add(new ValidationResult(
                    "La fecha del evento no puede ser posterior a 5 años desde hoy.",
                    new[] { nameof(FechaHora) }
                ));
            }

            if (ActualizadoAt < CreadoAt)
            {
                results.Add(new ValidationResult(
                    "La fecha de actualización no puede ser anterior a la fecha de creación.",
                    new[] { nameof(ActualizadoAt) }
                ));
            }

            if (!string.IsNullOrEmpty(Titulo) && string.IsNullOrWhiteSpace(Titulo))
            {
                results.Add(new ValidationResult(
                    "El título no puede contener solo espacios en blanco.",
                    new[] { nameof(Titulo) }
                ));
            }

            if (!string.IsNullOrEmpty(Titulo))
            {
                var palabras = Titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var palabrasRepetidas = palabras.GroupBy(p => p.ToLower())
                    .Where(g => g.Count() > 5)
                    .Select(g => g.Key);

                if (palabrasRepetidas.Any())
                {
                    results.Add(new ValidationResult(
                        "El título contiene palabras repetidas excesivamente.",
                        new[] { nameof(Titulo) }
                    ));
                }
            }

            if (!string.IsNullOrEmpty(DescripcionEvento))
            {
                var descripcionLimpia = Regex.Replace(DescripcionEvento, @"[\s\W_]+", "");
                if (descripcionLimpia.Length < 5)
                {
                    results.Add(new ValidationResult(
                        "La descripción debe contener al menos 5 caracteres alfanuméricos válidos.",
                        new[] { nameof(DescripcionEvento) }
                    ));
                }
            }

            if (!string.IsNullOrEmpty(Telefono))
            {
                if (!Telefono.StartsWith("9"))
                {
                    results.Add(new ValidationResult(
                        "El teléfono debe iniciar con 9 (formato peruano).",
                        new[] { nameof(Telefono) }
                    ));
                }
            }

            if (Estado == "Cancelado" && string.IsNullOrWhiteSpace(DescripcionEvento))
            {
                results.Add(new ValidationResult(
                    "Un evento cancelado debe incluir una descripción del motivo.",
                    new[] { nameof(DescripcionEvento), nameof(Estado) }
                ));
            }

            return results;
        }

        public void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
                throw new ArgumentException("El título no puede estar vacío o contener solo espacios.");

            if (Titulo.Length < 3 || Titulo.Length > 200)
                throw new ArgumentException("El título debe tener entre 3 y 200 caracteres.");

            if (!Regex.IsMatch(Titulo, @"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-()]+$"))
                throw new ArgumentException("El título contiene caracteres no válidos.");

            if (FechaHora == default(DateTime))
                throw new ArgumentException("Debe especificar una fecha y hora válida para el evento.");

            if (FechaHora < DateTime.UtcNow.AddMinutes(-5))
                throw new ArgumentException("La fecha del evento no puede ser anterior a la fecha actual (tolerancia de 5 minutos).");

            if (FechaHora > DateTime.UtcNow.AddYears(5))
                throw new ArgumentException("La fecha del evento no puede ser posterior a 5 años desde ahora.");

            if (!string.IsNullOrEmpty(Telefono))
            {
                if (!Regex.IsMatch(Telefono, @"^9[0-9]{8}$"))
                    throw new ArgumentException("El teléfono debe iniciar con 9 y contener exactamente 9 dígitos numéricos.");
            }

            if (!string.IsNullOrEmpty(Tipo))
            {
                if (Tipo.Length < 2 || Tipo.Length > 100)
                    throw new ArgumentException("El tipo debe tener entre 2 y 100 caracteres.");

                if (!Regex.IsMatch(Tipo, @"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s.\-]+$"))
                    throw new ArgumentException("El tipo solo puede contener letras, espacios, puntos y guiones.");
            }

            if (!string.IsNullOrEmpty(Estado))
            {
                var estadosValidos = new[] { "Pendiente", "Confirmado", "Cancelado", "Completado", "En Proceso", "Reagendado" };
                if (!estadosValidos.Contains(Estado))
                    throw new ArgumentException($"El estado debe ser uno de: {string.Join(", ", estadosValidos)}.");
            }

            if (!string.IsNullOrEmpty(Ubicacion))
            {
                if (Ubicacion.Length < 5 || Ubicacion.Length > 200)
                    throw new ArgumentException("La ubicación debe tener entre 5 y 200 caracteres.");

                if (!Regex.IsMatch(Ubicacion, @"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑüÜ\s,.\-#°/]+$"))
                    throw new ArgumentException("La ubicación contiene caracteres no válidos.");
            }

            if (!string.IsNullOrEmpty(DescripcionEvento))
            {
                if (DescripcionEvento.Length < 10 || DescripcionEvento.Length > 5000)
                    throw new ArgumentException("La descripción debe tener entre 10 y 5000 caracteres.");
            }

            if (CreadoAt > DateTime.UtcNow)
                throw new ArgumentException("La fecha de creación no puede ser futura.");

            if (ActualizadoAt < CreadoAt)
                throw new ArgumentException("La fecha de actualización no puede ser anterior a la fecha de creación.");

            if (ActualizadoAt > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de actualización no puede ser futura.");

            if (IdUsuario <= 0)
                throw new ArgumentException("El IdUsuario debe ser mayor que cero.");

            if (IdTipoPrioridad.HasValue && IdTipoPrioridad.Value <= 0)
                throw new ArgumentException("El IdTipoPrioridad debe ser mayor que cero.");
        }

        public void ActualizarTiempos()
        {
            ActualizadoAt = DateTime.UtcNow;
        }

        public bool EsEventoProximo()
        {
            var diferenciaHoras = (FechaHora - DateTime.UtcNow).TotalHours;
            return diferenciaHoras >= 0 && diferenciaHoras <= 24;
        }

        public bool EsEventoPasado()
        {
            return FechaHora < DateTime.UtcNow;
        }

        public TimeSpan TiempoRestante()
        {
            return FechaHora - DateTime.UtcNow;
        }

        public bool PuedeCancelar()
        {
            return !EsEventoPasado() && Estado != "Cancelado" && Estado != "Completado";
        }


        public bool PuedeCompletar()
        {
            return !EsEventoPasado() && Estado != "Cancelado" && Estado != "Completado";
        }
    }
}
