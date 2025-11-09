using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("agenteinmobiliario")]
    public class AgenteInmobiliario
    {
        [Key]
        [Column("idAgente")]
        public int IdAgente { get; set; }

        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [InverseProperty("AgenteInmobiliario")]
        public virtual ICollection<Cita>? Citas { get; set; }

    }
}
