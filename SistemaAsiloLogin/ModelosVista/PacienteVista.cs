namespace SistemaAsiloLogin.ModelosVista
{
    public class PacienteVista
    {
        public int IdPaciente { get; set; }

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }

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
