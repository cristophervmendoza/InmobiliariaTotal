using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("tipopropiedad")]
    public class TipoPropiedad : IValidatableObject
    {
        [Key]
        [Column("idTipoPropiedad")]
        public int IdTipoPropiedad { get; set; }

        [Required(ErrorMessage = "El nombre del tipo de propiedad es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        [Column("nombre")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de creación es obligatoria")]
        [Column("creadoAt")]
        public DateTime CreadoAt { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        [InverseProperty("TipoPropiedad")]
        public virtual System.Collections.Generic.ICollection<Propiedad> Propiedades { get; set; }

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

                var tiposValidos = new[]
                {
                    "casa",
                    "departamento",
                    "terreno",
                    "local comercial",
                    "oficina",
                    "edificio",
                    "quinta",
                    "cochera",
                    "deposito",
                    "almacen",
                    "campo",
                    "granja",
                    "hacienda",
                    "casa de playa",
                    "casa de campo",
                    "penthouse",
                    "duplex",
                    "triplex",
                    "loft",
                    "estudio"
                };

                if (!tiposValidos.Any(t => t.Equals(Nombre.Trim().ToLower())))
                {
                    yield return new ValidationResult("El tipo de propiedad no es válido. Tipos permitidos: Casa, Departamento, Terreno, Local Comercial, Oficina, Edificio, Quinta, Cochera, Deposito, Almacen, Campo, Granja, Hacienda, Casa de Playa, Casa de Campo, Penthouse, Duplex, Triplex, Loft, Estudio", new[] { nameof(Nombre) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "temporal", "temp", "demo", "fake" };
                if (palabrasProhibidas.Any(p => Nombre.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El nombre contiene palabras no permitidas", new[] { nameof(Nombre) });
                }

                if (Nombre.Length < 4)
                {
                    yield return new ValidationResult("El nombre del tipo de propiedad es demasiado corto", new[] { nameof(Nombre) });
                }

                var palabras = Nombre.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length > 4)
                {
                    yield return new ValidationResult("El nombre del tipo de propiedad no debe tener más de 4 palabras", new[] { nameof(Nombre) });
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

            if (!string.IsNullOrEmpty(Descripcion))
            {
                if (Descripcion.Trim().Length < 5)
                {
                    yield return new ValidationResult("La descripción debe tener al menos 5 caracteres significativos", new[] { nameof(Descripcion) });
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

                if (Descripcion.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("La descripción no puede estar completamente en minúsculas", new[] { nameof(Descripcion) });
                }

                if (Regex.IsMatch(Descripcion, @"[<>{}[\]\\|`~]"))
                {
                    yield return new ValidationResult("La descripción contiene caracteres de código no permitidos", new[] { nameof(Descripcion) });
                }

                if (!Regex.IsMatch(Descripcion, @"[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]{3,}"))
                {
                    yield return new ValidationResult("La descripción debe contener palabras válidas", new[] { nameof(Descripcion) });
                }

                var palabras = Descripcion.Split(new[] { ' ', '\n', '\r', '.', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length < 2)
                {
                    yield return new ValidationResult("La descripción debe contener al menos 2 palabras", new[] { nameof(Descripcion) });
                }

                if (palabras.Any(p => p.Length > 100))
                {
                    yield return new ValidationResult("La descripción contiene palabras excesivamente largas", new[] { nameof(Descripcion) });
                }

                if (!string.IsNullOrEmpty(Nombre) && Nombre.ToLower() == Descripcion.ToLower())
                {
                    yield return new ValidationResult("El nombre y la descripción no pueden ser idénticos", new[] { nameof(Nombre), nameof(Descripcion) });
                }

                if (!string.IsNullOrEmpty(Nombre) && Descripcion.ToLower().StartsWith(Nombre.ToLower() + "."))
                {
                    yield return new ValidationResult("La descripción no debe ser solo el nombre seguido de un punto", new[] { nameof(Descripcion) });
                }

                var longitudPromedioPalabras = palabras.Length > 0 ? palabras.Average(p => p.Length) : 0;
                if (longitudPromedioPalabras < 2)
                {
                    yield return new ValidationResult("La descripción parece contener palabras demasiado cortas o sin sentido", new[] { nameof(Descripcion) });
                }

                if (Descripcion.Length > 200)
                {
                    var cantidadPuntos = Descripcion.Count(c => c == '.');
                    if (cantidadPuntos == 0)
                    {
                        yield return new ValidationResult("Las descripciones largas deben contener al menos un punto", new[] { nameof(Descripcion) });
                    }
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
                yield return new ValidationResult("Los tipos de propiedad son datos de catálogo que deberían existir desde el inicio del sistema", new[] { nameof(CreadoAt) });
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
                yield return new ValidationResult("El rango entre las fechas parece inconsistente para un tipo de catálogo", new[] { nameof(ActualizadoAt), nameof(CreadoAt) });
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

            if (CreadoAt.Year < 2015)
            {
                yield return new ValidationResult("La fecha de creación parece ser demasiado antigua para un sistema moderno", new[] { nameof(CreadoAt) });
            }
        }
    }
}
