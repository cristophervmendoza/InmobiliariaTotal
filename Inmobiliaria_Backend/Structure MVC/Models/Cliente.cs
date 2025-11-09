using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("cliente")]
    public class Cliente
    {
        [Key]
        [Column("idCliente")]
        public int IdCliente { get; set; }

        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        // Navegación
        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [InverseProperty("Cliente")]
        public virtual ICollection<Cita>? Citas { get; set; }
    }
}
