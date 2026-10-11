using System.ComponentModel.DataAnnotations;

namespace MicroservicioAncianos.Models
{
    public class Paciente
    {
        [Key]
        public int IdPaciente { get; set; }

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El DPI debe contener exactamente 13 dígitos.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El DPI debe contener únicamente números.")]
        public string? DPI { get; set; }

        public string Sexo { get; set; } = "";

        public string Direccion { get; set; } = "";

        public string Telefono { get; set; } = "";

        public DateTime FechaIngreso { get; set; }

        public bool Estado { get; set; }

        public string? MotivoIngreso { get; set; }
        public string? Psicopatologia { get; set; }
    }
}