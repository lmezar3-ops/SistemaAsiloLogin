using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class ExamenLaboratorio
    {
        [Key]
        public int IdExamen { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una visita médica válida.")]
        public int IdVisita { get; set; }

        [Required(ErrorMessage = "El nombre del examen es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
        public string NombreExamen { get; set; } = "";

        [Required(ErrorMessage = "La fecha de solicitud es obligatoria.")]
        public DateTime FechaSolicitud { get; set; }

        public string Estado { get; set; } = "Pendiente";

        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El costo no puede ser negativo.")]
        public decimal? Costo { get; set; }

        [MaxLength(500)]
        public string? JustificacionRepeticion { get; set; }

        [MaxLength(500)]
        public string? MotivoCancelacion { get; set; }

        [ForeignKey("IdVisita")]
        public VisitaMedica? VisitaMedica { get; set; }
    
    }
}