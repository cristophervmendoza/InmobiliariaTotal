using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("cita")]
    public class Cita : IValidatableObject
    {
        [Key]
        [Column("idCita")]
        public int IdCita { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.Date)]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria")]
        [DataType(DataType.Time)]
        [Column("hora")]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "El ID de cliente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de cliente debe ser mayor a 0")]
        [Column("idCliente")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El ID de agente es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de agente debe ser mayor a 0")]
        [Column("idAgente")]
        public int IdAgente { get; set; }

        [Required(ErrorMessage = "El ID de estado de cita es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de estado debe ser mayor a 0")]
        [Column("idEstadoCita")]
        public int IdEstadoCita { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("IdAgente")]
        public virtual AgenteInmobiliario Agente { get; set; }

        [ForeignKey("IdEstadoCita")]
        public virtual EstadoCita EstadoCita { get; set; }

        [NotMapped]
        public string NombreCliente => Cliente?.Usuario?.Nombre;

        [NotMapped]
        public string NombreAgente => Agente?.Usuario?.Nombre;

        [NotMapped]
        public string EstadoNombre => EstadoCita?.Nombre;

        [NotMapped]
        public DateTime FechaHoraCompleta => Fecha.Date.Add(Hora);

        public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Fecha < new DateTime(2000, 1, 1))
            {
                yield return new ValidationResult("La fecha no es válida", new[] { nameof(Fecha) });
            }

            if (Fecha > DateTime.Now.AddYears(2))
            {
                yield return new ValidationResult("La fecha de la cita es demasiado lejana en el futuro", new[] { nameof(Fecha) });
            }

            if (Fecha < DateTime.Now.Date.AddDays(-1))
            {
                yield return new ValidationResult("No se pueden crear citas con fechas pasadas", new[] { nameof(Fecha) });
            }

            if (Fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                yield return new ValidationResult("No se permiten citas los domingos", new[] { nameof(Fecha) });
            }

            var fechasNoLaborables = new[]
            {
                new DateTime(DateTime.Now.Year, 1, 1),
                new DateTime(DateTime.Now.Year, 5, 1),
                new DateTime(DateTime.Now.Year, 7, 28),
                new DateTime(DateTime.Now.Year, 7, 29),
                new DateTime(DateTime.Now.Year, 8, 30),
                new DateTime(DateTime.Now.Year, 10, 8),
                new DateTime(DateTime.Now.Year, 11, 1),
                new DateTime(DateTime.Now.Year, 12, 8),
                new DateTime(DateTime.Now.Year, 12, 25)
            };

            if (fechasNoLaborables.Any(f => f.Date == Fecha.Date))
            {
                yield return new ValidationResult("No se permiten citas en días feriados", new[] { nameof(Fecha) });
            }

            if (Hora.TotalHours < 0 || Hora.TotalHours >= 24)
            {
                yield return new ValidationResult("La hora no es válida", new[] { nameof(Hora) });
            }

            if (Hora.TotalHours < 8)
            {
                yield return new ValidationResult("Las citas deben ser después de las 8:00 AM", new[] { nameof(Hora) });
            }

            if (Hora.TotalHours >= 20)
            {
                yield return new ValidationResult("Las citas deben ser antes de las 8:00 PM", new[] { nameof(Hora) });
            }

            if (Hora.TotalHours >= 13 && Hora.TotalHours < 14)
            {
                yield return new ValidationResult("No se permiten citas durante el horario de almuerzo (1:00 PM - 2:00 PM)", new[] { nameof(Hora) });
            }

            if (Hora.Seconds != 0)
            {
                yield return new ValidationResult("La hora no debe incluir segundos", new[] { nameof(Hora) });
            }

            if (Hora.Minutes % 15 != 0)
            {
                yield return new ValidationResult("Las citas deben programarse en intervalos de 15 minutos", new[] { nameof(Hora) });
            }

            var fechaHoraCompleta = Fecha.Date.Add(Hora);
            if (fechaHoraCompleta < DateTime.Now.AddHours(2))
            {
                yield return new ValidationResult("Las citas deben programarse con al menos 2 horas de anticipación", new[] { nameof(Fecha), nameof(Hora) });
            }

            if (fechaHoraCompleta > DateTime.Now.AddMonths(6))
            {
                yield return new ValidationResult("No se pueden programar citas con más de 6 meses de anticipación", new[] { nameof(Fecha), nameof(Hora) });
            }

            if (IdCliente == IdAgente)
            {
                yield return new ValidationResult("El cliente y el agente no pueden ser la misma persona", new[] { nameof(IdCliente), nameof(IdAgente) });
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

            if (Fecha < CreadoAt.Date.AddDays(-1))
            {
                yield return new ValidationResult("La fecha de la cita no puede ser anterior a la fecha de creación", new[] { nameof(Fecha), nameof(CreadoAt) });
            }

            if (fechaHoraCompleta < CreadoAt.AddMinutes(-5))
            {
                yield return new ValidationResult("La fecha y hora de la cita no puede ser anterior a la fecha de creación", new[] { nameof(Fecha), nameof(Hora), nameof(CreadoAt) });
            }

            if (Fecha == DateTime.Now.Date && Hora < TimeSpan.FromHours(DateTime.Now.Hour + 2))
            {
                yield return new ValidationResult("Las citas para hoy deben tener al menos 2 horas de anticipación", new[] { nameof(Hora) });
            }

            var duracionDesdeCreacion = (fechaHoraCompleta - CreadoAt).TotalDays;
            if (duracionDesdeCreacion > 180)
            {
                yield return new ValidationResult("La cita fue programada con demasiada anticipación desde su creación", new[] { nameof(Fecha) });
            }

            if (Fecha.DayOfWeek == DayOfWeek.Saturday && Hora.TotalHours >= 14)
            {
                yield return new ValidationResult("Los sábados solo se atiende hasta las 2:00 PM", new[] { nameof(Hora) });
            }

            if (fechaHoraCompleta.Date == DateTime.Now.Date && fechaHoraCompleta < DateTime.Now)
            {
                yield return new ValidationResult("No se puede crear una cita para una hora que ya pasó hoy", new[] { nameof(Hora) });
            }

            if (CreadoAt.Hour < 8 || CreadoAt.Hour >= 20)
            {
                yield return new ValidationResult("Las citas no pueden ser creadas fuera del horario de oficina", new[] { nameof(CreadoAt) });
            }

            if (ActualizadoAt < fechaHoraCompleta && fechaHoraCompleta < DateTime.Now)
            {
                yield return new ValidationResult("No se puede actualizar una cita que ya pasó sin cambiar su fecha", new[] { nameof(ActualizadoAt) });
            }
        }
    }
}
