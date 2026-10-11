
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    public class DonacionesModel : PageModel
    {
        private readonly AsiloDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public DonacionesModel(
            AsiloDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // Datos recibidos desde el microservicio para la tabla.
        public List<DonacionApiDto> Donaciones { get; set; } = new();

        // Los donantes siguen cargándose desde SQL Server.
        public List<Donante> Donantes { get; set; } = new();

        [BindProperty]
        public Donacion NuevaDonacion { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NuevaDonacion.IdDonante <= 0)
            {
                ModelState.AddModelError(
                    nameof(NuevaDonacion.IdDonante),
                    "Debe seleccionar un donante.");
            }

            if (NuevaDonacion.Monto <= 0)
            {
                ModelState.AddModelError(
                    nameof(NuevaDonacion.Monto),
                    "El monto de la donación debe ser mayor que cero.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            NuevaDonacion.FechaDonacion =
                NuevaDonacion.FechaDonacion == default
                    ? DateTime.Now
                    : NuevaDonacion.FechaDonacion;

            // El registro continúa guardándose directamente en SQL Server.
            _context.Donaciones.Add(NuevaDonacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La donación se registró correctamente.";

            return RedirectToPage();
        }

        private async Task CargarDatosAsync()
        {
            // El desplegable del formulario sigue usando la base de datos.
            Donantes = await _context.Donantes
                .OrderBy(d => d.Nombre)
                .ToListAsync();

            // La tabla consulta la API del microservicio.
            try
            {
                var cliente = _httpClientFactory.CreateClient();

                Donaciones = await cliente.GetFromJsonAsync
                    <List<DonacionApiDto>>(
                        "http://localhost:8080/api/Donaciones")
                    ?? new List<DonacionApiDto>();
            }
            catch (HttpRequestException)
            {
                TempData["ErrorApi"] =
                    "No se pudo conectar con el microservicio de donaciones. " +
                    "Verifica que el contenedor esté funcionando.";
            }
            catch (System.Text.Json.JsonException)
            {
                TempData["ErrorApi"] =
                    "La respuesta del microservicio de donaciones no tiene " +
                    "el formato esperado.";
            }
            catch (TaskCanceledException)
            {
                TempData["ErrorApi"] =
                    "La consulta al microservicio de donaciones tardó demasiado.";
            }
        }
    }

    // DTO: representa la respuesta JSON de la API.
    // No es una entidad ni una tabla de la base de datos.
    public class DonacionApiDto
    {
        [JsonPropertyName("id")]
        public int IdDonacion { get; set; }

        [JsonPropertyName("donante")]
        public string Donante { get; set; } = "";

        [JsonPropertyName("tipo")]
        public string TipoDonacion { get; set; } = "";

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }

        [JsonPropertyName("fecha")]
        public DateTime FechaDonacion { get; set; }
    }
}
