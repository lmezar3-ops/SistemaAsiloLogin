using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = "";

        // Aquí se guardará el hash de la contraseña, no la contraseña legible.
        [Required]
        [StringLength(100)]
        public string Contraseña { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = "";

        public bool Estado { get; set; } = true;

        [Column("contrasenaHasheada")]
        public bool ContrasenaHasheada { get; set; } = false;

        [StringLength(150)]
        public string? NombreCompleto { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(100)]
        public string? Puesto { get; set; }

        [StringLength(150)]
        public string? CorreoElectronico { get; set; }
    }
}