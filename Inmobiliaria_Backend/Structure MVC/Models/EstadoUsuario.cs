using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("estado_usuario")]
    public class EstadoUsuario
    {
        [Key]
        [Column("idEstadoUsuario")]
        public int IdEstadoUsuario { get; set; }

        [Required(ErrorMessage = "El nombre del estado es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del estado debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede superar los 200 caracteres.")]
        public string? Descripcion { get; set; }

        [Required]
        public bool Activo { get; set; } = true;

        // Relación inversa (un estado puede tener varios usuarios)
        [InverseProperty("EstadoUsuario")]
        public virtual ICollection<Usuario>? Usuarios { get; set; }
    }
}
