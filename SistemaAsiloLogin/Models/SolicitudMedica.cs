using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class SolicitudMedica
    {
        [Key]
        public int IdSolicitud { get; set; }

        public int IdPaciente { get; set; }

        public int IdMedicoGeneral { get; set; }

        public int IdEspecialidad { get; set; }

        public int? IdEnfermero { get; set; }

        public DateTime FechaSolicitud { get; set; }

        [Required(ErrorMessage = "El motivo de la solicitud es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
        public string Motivo { get; set; } = "";

        public string Estado { get; set; } = "Pendiente";

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("IdMedicoGeneral")]
        public Medico? MedicoGeneral { get; set; }

        [ForeignKey("IdEspecialidad")]
        public Especialidad? Especialidad { get; set; }

        [ForeignKey("IdEnfermero")]
        public Enfermero? Enfermero { get; set; }
    }
}