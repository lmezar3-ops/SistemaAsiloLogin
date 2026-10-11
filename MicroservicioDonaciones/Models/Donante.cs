using System.ComponentModel.DataAnnotations;

namespace MicroservicioDonaciones.Models
{
    public class Donante
    {
        [Key]
        public int IdDonante { get; set; }

        public string Nombre { get; set; } = "";

        public string TipoDonante { get; set; } = "";

        public string? Telefono { get; set; }

        public string? CorreoElectronico { get; set; }

        public string? Direccion { get; set; }

        public ICollection<Donacion> Donaciones { get; set; }
            = new List<Donacion>();
    }
}