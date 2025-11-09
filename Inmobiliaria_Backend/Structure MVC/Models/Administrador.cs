using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("administrador")]
    public class Administrador
    {
        [Key]
        [Column("idAdministrador")]
        public int IdAdministrador { get; set; }

        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }


        [InverseProperty("Administrador")]
        public virtual ICollection<Cita>? Citas { get; set; }

    }
}
