using System.ComponentModel.DataAnnotations;

namespace SistemaAsiloLogin.Models
{
    public class Especialidad
    {
        [Key]
        public int IdEspecialidad { get; set; }

        public string Nombre { get; set; } = "";

        public string Descripcion { get; set; } = "";
    }
}