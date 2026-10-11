using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaAsiloLogin.Models
{
    public class PacienteFamiliar
    {
        public int IdPaciente { get; set; }

        public int IdFamiliar { get; set; }

        public string Parentesco { get; set; } = "";

        public bool EsResponsable { get; set; }

        [ForeignKey("IdPaciente")]
        public Paciente? Paciente { get; set; }

        [ForeignKey("IdFamiliar")]
        public Familiar? Familiar { get; set; }
    }
}