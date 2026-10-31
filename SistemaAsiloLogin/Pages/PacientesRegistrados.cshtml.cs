
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaAsiloLogin.ModelosVista;
using System.Net.Http.Json;
using static System.Collections.Specialized.BitVector32;


namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Enfermero")]
    public class PacientesRegistradosModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PacientesRegistradosModel(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<PacienteVista> Pacientes { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task OnGetAsync(string? buscar)
        {
            Response.Headers["Cache-Control"] =
                "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            try
            {
                var cliente = _httpClientFactory.CreateClient("Ancianos");

                var respuesta = await cliente.GetAsync(
                    "api/Ancianos");

                if (!respuesta.IsSuccessStatusCode)
                {
                    MensajeError =
                        "No fue posible consultar los pacientes. " +
                        "Verifique que el microservicio esté funcionando.";
                    return;
                }

                Pacientes = await respuesta.Content
                    .ReadFromJsonAsync<List<PacienteVista>>()
                    ?? new List<PacienteVista>();

                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    buscar = buscar.Trim();

                    Pacientes = Pacientes
                        .Where(p =>
                            p.Nombres.Contains(
                                buscar,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            p.Apellidos.Contains(
                                buscar,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            (p.DPI != null &&
                             p.DPI.Contains(buscar,
                                StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
            }
            catch (HttpRequestException)
            {
                MensajeError =
                    "No se pudo conectar con el microservicio de Ancianos. " +
                    "Compruebe que esté iniciado en Docker.";
            }
            catch (System.Text.Json.JsonException)
            {
                MensajeError =
                    "La respuesta del microservicio no tiene el formato esperado.";
            }
        }
    }
}
