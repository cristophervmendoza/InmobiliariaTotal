using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("propiedad")]
    public class Propiedad
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPropiedad { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [ForeignKey("Usuario")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdUsuario debe ser un número válido mayor que cero.")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de propiedad.")]
        [ForeignKey("TipoPropiedad")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdTipoPropiedad debe ser un número válido mayor que cero.")]
        public int IdTipoPropiedad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado de propiedad.")]
        [ForeignKey("EstadoPropiedad")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdEstadoPropiedad debe ser un número válido mayor que cero.")]
        public int IdEstadoPropiedad { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-]+$", ErrorMessage = "El título contiene caracteres no válidos.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 500 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-#]+$", ErrorMessage = "La dirección contiene caracteres no válidos.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 999999999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999,999,999.99.")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal Precio { get; set; }

        [StringLength(5000, ErrorMessage = "La descripción no puede superar los 5000 caracteres.")]
        public string? Descripcion { get; set; }


        [Range(0.0, 1000000.0, ErrorMessage = "El área del terreno debe ser un valor válido.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? AreaTerreno { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Debe especificar el tipo de moneda.")]
        [RegularExpression("^(PEN|USD|EUR)$", ErrorMessage = "El tipo de moneda debe ser 'PEN', 'USD' o 'EUR'.")]
        public string TipoMoneda { get; set; } = "PEN";

        [Range(0, 50, ErrorMessage = "Número de habitaciones inválido (máximo 50).")]
        public int? Habitacion { get; set; }

        [Range(0, 50, ErrorMessage = "Número de baños inválido (máximo 50).")]
        public int? Bano { get; set; }

        [Range(0, 50, ErrorMessage = "Número de estacionamientos inválido (máximo 50).")]
        public int? Estacionamiento { get; set; }

        [StringLength(255, ErrorMessage = "La ruta de la foto no puede exceder los 255 caracteres.")]
        [RegularExpression(@"^[\w,\s\-\/\\]+\.(jpg|jpeg|png|webp)$",
            ErrorMessage = "El formato de imagen no es válido (solo JPG, JPEG, PNG o WEBP).")]
        public string? FotoPropiedad { get; set; }


        [Required]
        public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ActualizadoAt { get; set; } = DateTime.UtcNow;


        [InverseProperty("Propiedades")]
        public virtual Usuario? Usuario { get; set; }

        [InverseProperty("Propiedades")]
        public virtual TipoPropiedad? TipoPropiedad { get; set; }

        [InverseProperty("Propiedades")]
        public virtual EstadoPropiedad? EstadoPropiedad { get; set; }

        [InverseProperty("Propiedad")]
        public virtual ICollection<FotoPropiedad>? FotosPropiedad { get; set; }


        public void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(Titulo))
                throw new ArgumentException("El título no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(Direccion))
                throw new ArgumentException("La dirección no puede estar vacía.");

            if (Precio <= 0)
                throw new ArgumentException("El precio debe ser mayor que cero.");

            if (!Regex.IsMatch(TipoMoneda, "^(PEN|USD|EUR)$"))
                throw new ArgumentException("Tipo de moneda no válido.");

            if (Habitacion < 0 || Habitacion > 50)
                throw new ArgumentException("Número de habitaciones fuera de rango.");

            if (Bano < 0 || Bano > 50)
                throw new ArgumentException("Número de baños fuera de rango.");

            if (Estacionamiento < 0 || Estacionamiento > 50)
                throw new ArgumentException("Número de estacionamientos fuera de rango.");

            if (CreadoAt > DateTime.UtcNow)
                throw new ArgumentException("La fecha de creación no puede ser futura.");
        }


        public void ActualizarTiempos()
        {
            ActualizadoAt = DateTime.UtcNow;
        }
    }
}
