using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("fotopropiedad")]
    public class FotoPropiedad
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFoto { get; set; }


        [Required(ErrorMessage = "Debe especificar la propiedad.")]
        [ForeignKey("Propiedad")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdPropiedad debe ser un número mayor que cero.")]
        public int IdPropiedad { get; set; }


        [Required(ErrorMessage = "La ruta de la foto es obligatoria.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "La ruta de la foto debe tener entre 5 y 255 caracteres.")]
        [RegularExpression(@"^[\w,\s\-\/\\]+(\.(jpg|jpeg|png|webp))$",
            ErrorMessage = "El formato de la imagen no es válido. Solo se permiten JPG, JPEG, PNG o WEBP.")]
        public string RutaFoto { get; set; } = string.Empty;

        [Required]
        [Range(0, 1, ErrorMessage = "El valor de 'esPrincipal' debe ser 0 (no) o 1 (sí).")]
        public bool EsPrincipal { get; set; } = false;

        [StringLength(100, ErrorMessage = "La descripción no puede superar los 100 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9ÁÉÍÓÚáéíóúñÑ\s,.\-]*$", ErrorMessage = "La descripción contiene caracteres inválidos.")]
        public string? Descripcion { get; set; }

        [Required]
        public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

        public virtual Propiedad? Propiedad { get; set; }

        public void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(RutaFoto))
                throw new ArgumentException("La ruta de la foto no puede estar vacía.");

            if (!Regex.IsMatch(RutaFoto, @"^[\w,\s\-\/\\]+(\.(jpg|jpeg|png|webp))$", RegexOptions.IgnoreCase))
                throw new ArgumentException("Formato de imagen no permitido (solo .jpg, .jpeg, .png, .webp).");

            if (CreadoAt > DateTime.UtcNow)
                throw new ArgumentException("La fecha de creación no puede ser futura.");
        }
    }
}
