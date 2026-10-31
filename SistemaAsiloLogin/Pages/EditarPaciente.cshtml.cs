
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SistemaAsiloLogin.ModelosVista;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico,Enfermero")]
    public class EditarPacienteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EditarPacienteModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public PacienteVista Paciente { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cliente = _httpClientFactory.CreateClient("Ancianos");

            try
            {
                var respuesta = await cliente.GetAsync(
                    "api/Ancianos");

                if (!respuesta.IsSuccessStatusCode)
                {
                    MensajeError = "No fue posible consultar los pacientes.";
                    return Page();
                }

                var pacientes = await respuesta.Content
                    .ReadFromJsonAsync<List<PacienteVista>>()
                    ?? new List<PacienteVista>();

                var paciente = pacientes.FirstOrDefault(p => p.IdPaciente == id);

                if (paciente == null)
                    return NotFound();

                Paciente = paciente;
                return Page();
            }
            catch (HttpRequestException)
            {
                MensajeError = "No se pudo conectar con el microservicio de Ancianos.";
                return Page();
            }
            catch (System.Text.Json.JsonException)
            {
                MensajeError = "La respuesta del microservicio no tiene el formato esperado.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var cliente = _httpClientFactory.CreateClient("Ancianos");

            try
            {
                var respuesta = await cliente.PutAsJsonAsync(
                      $"api/Ancianos/{Paciente.IdPaciente}",
                    Paciente);

                if (!respuesta.IsSuccessStatusCode)
                {
                    var contenido = await respuesta.Content.ReadAsStringAsync();

                    MensajeError = $"Error al guardar (HTTP {(int)respuesta.StatusCode}): {contenido}";

                    return Page();
                }

                TempData["MensajePaciente"] = "Paciente actualizado correctamente.";

                return RedirectToPage("/PacientesRegistrados");
            }
            catch (HttpRequestException)
            {
                MensajeError = "No se pudo conectar con el microservicio de Ancianos.";
                return Page();
            }
        }
    }
}
