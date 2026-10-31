using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico")]
    public class TratamientosModel : PageModel
    {
        private readonly AsiloDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public TratamientosModel(
            AsiloDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public Tratamiento NuevoTratamiento { get; set; } = new();

        public List<VisitaMedica> Visitas { get; set; } = new();

        public List<MedicamentoApi> Medicamentos { get; set; } = new();

        public class SolicitudEntregaApi
        {
            public int IdTratamiento { get; set; }
            public int IdMedicamento { get; set; }
            public int Cantidad { get; set; }
            public string OrigenTratamiento { get; set; } = "AplicacionPrincipal";
        }

        public List<Tratamiento> TratamientosRegistrados { get; set; } = new();

        public Dictionary<int, string> NombresMedicamentos { get; set; } = new();

        public string? MensajeErrorApi { get; set; }

        private bool InventarioDisponible { get; set; }

        public class MedicamentoApi
        {
            public int IdMedicamento { get; set; }
            public string Nombre { get; set; } = "";
            public int Existencia { get; set; }
            public bool Estado { get; set; }
        }


        private async Task CargarMedicamentosAsync()
        {
            InventarioDisponible = false;
            Medicamentos = new();
            NombresMedicamentos = new();
            MensajeErrorApi = null;

            try
            {
                var cliente = _httpClientFactory.CreateClient("Medicamentos");

                cliente.Timeout = TimeSpan.FromSeconds(15);
                using var respuesta = await cliente.GetAsync(
                    "api/Medicamentos");

                if (!respuesta.IsSuccessStatusCode)
                {
                    MensajeErrorApi =
                        "No fue posible consultar el inventario de farmacia.";
                    return;
                }

                var medicamentosRecibidos =
                    await respuesta.Content.ReadFromJsonAsync<List<MedicamentoApi>>();

                if (medicamentosRecibidos == null ||
                    medicamentosRecibidos.Any(m => m.IdMedicamento <= 0 ||
                        string.IsNullOrWhiteSpace(m.Nombre)))
                {
                    MensajeErrorApi =
                        "El inventario devolvió datos incompletos o inesperados.";
                    return;
                }

                if (medicamentosRecibidos
                    .GroupBy(m => m.IdMedicamento)
                    .Any(g => g.Count() > 1))
                {
                    MensajeErrorApi =
                        "El inventario devolvió identificadores duplicados.";
                    return;
                }

                Medicamentos = medicamentosRecibidos;

                NombresMedicamentos = Medicamentos.ToDictionary(
                    m => m.IdMedicamento,
                    m => m.Nombre);

                InventarioDisponible = true;
            }
            catch (HttpRequestException)
            {
                MensajeErrorApi =
                    "No se pudo conectar con el microservicio de medicamentos. " +
                    "Verifica que el contenedor de farmacia esté iniciado.";
            }
            catch (System.Text.Json.JsonException)
            {
                MensajeErrorApi =
                    "La respuesta del inventario tiene un formato inesperado.";
            }
        }

        private async Task CargarDatosAsync()
        {
            Visitas = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.Medico)
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();

            TratamientosRegistrados = await _context.Tratamientos
                .Include(t => t.VisitaMedica)
                    .ThenInclude(v => v!.Paciente)
                .OrderByDescending(t => t.IdTratamiento)
                .ToListAsync();

            await CargarMedicamentosAsync();
        }

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostReintentarAsync(int idTratamiento)
        {
            var tratamiento = await _context.Tratamientos
                .FirstOrDefaultAsync(t => t.IdTratamiento == idTratamiento);

            if (tratamiento == null)
            {
                TempData["Mensaje"] = "No se encontro el tratamiento solicitado.";
                return RedirectToPage();
            }

            if (tratamiento.EstadoEntrega == "ENTREGADO")
            {
                TempData["Mensaje"] = "Este tratamiento ya figura como entregado.";
                return RedirectToPage();
            }

            if (tratamiento.EstadoEntrega != "PENDIENTE")
            {
                TempData["Mensaje"] =
                    "El estado de esta entrega no permite reintentarla automaticamente. Verifica el caso primero.";
                return RedirectToPage();
            }

            try
            {
                var cliente = _httpClientFactory.CreateClient("Medicamentos");
                cliente.Timeout = TimeSpan.FromSeconds(15);

                var solicitud = new SolicitudEntregaApi
                {
                    IdTratamiento = tratamiento.IdTratamiento,
                    IdMedicamento = tratamiento.IdMedicamento,
                    Cantidad = tratamiento.Cantidad,
                    OrigenTratamiento = "AplicacionPrincipal"
                };

                using var respuesta = await cliente.PostAsJsonAsync(
                    "api/Medicamentos/entregas",
                    solicitud);

                if (respuesta.IsSuccessStatusCode)
                {
                    tratamiento.EstadoEntrega = "ENTREGADO";
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] =
                        "Entrega confirmada. El microservicio no debe descontar dos veces el mismo tratamiento.";
                }
                else
                {
                    var detalle = await respuesta.Content.ReadAsStringAsync();

                    TempData["Mensaje"] =
                        $"La entrega sigue pendiente. Respuesta del inventario: {(int)respuesta.StatusCode}. {detalle}";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                TempData["Mensaje"] =
                    "No se pudo contactar con el inventario. La entrega permanece pendiente.";
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (NuevoTratamiento.IdVisita <= 0)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.IdVisita",
                    "Selecciona una visita médica.");
            }

            if (NuevoTratamiento.IdMedicamento <= 0)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.IdMedicamento",
                    "Selecciona un medicamento.");
            }

            if (NuevoTratamiento.Cantidad <= 0)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Cantidad",
                    "La cantidad debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(NuevoTratamiento.Dosis) ||
                NuevoTratamiento.Dosis.Length > 100)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Dosis",
                    "La dosis es obligatoria y no debe superar 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(NuevoTratamiento.Frecuencia) ||
                NuevoTratamiento.Frecuencia.Length > 100)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Frecuencia",
                    "La frecuencia es obligatoria y no debe superar 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(NuevoTratamiento.Duracion) ||
                NuevoTratamiento.Duracion.Length > 100)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Duracion",
                    "La duración es obligatoria y no debe superar 100 caracteres.");
            }

            if (NuevoTratamiento.Indicaciones?.Length > 500)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Indicaciones",
                    "Las indicaciones no deben superar 500 caracteres.");
            }

            
            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            var visitaExiste = await _context.VisitasMedicas
                .AnyAsync(v => v.IdVisita == NuevoTratamiento.IdVisita);

            if (!visitaExiste)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.IdVisita",
                    "La visita médica seleccionada no existe.");

                await CargarDatosAsync();
                return Page();
            }


            await CargarMedicamentosAsync();

            if (!InventarioDisponible)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se puede registrar el tratamiento porque no fue posible verificar el inventario.");

                await CargarDatosAsync();
                return Page();
            }

            var medicamento = Medicamentos.FirstOrDefault(
                m => m.IdMedicamento == NuevoTratamiento.IdMedicamento);


            if (medicamento == null || !medicamento.Estado)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.IdMedicamento",
                    "El medicamento seleccionado no existe o está inactivo.");

                await CargarDatosAsync();
                return Page();
            }

            if (medicamento.Existencia < NuevoTratamiento.Cantidad)
            {
                ModelState.AddModelError(
                    "NuevoTratamiento.Cantidad",
                    $"La cantidad solicitada supera las existencias disponibles ({medicamento.Existencia}).");

                await CargarDatosAsync();
                return Page();
            }

            NuevoTratamiento.Dosis = NuevoTratamiento.Dosis.Trim();
            NuevoTratamiento.Frecuencia = NuevoTratamiento.Frecuencia.Trim();
            NuevoTratamiento.Duracion = NuevoTratamiento.Duracion.Trim();
            NuevoTratamiento.Indicaciones =
            NuevoTratamiento.Indicaciones?.Trim() ?? "";

            //aqui
            var yaExisteIgual = await _context.Tratamientos.AnyAsync(t =>
            t.IdVisita == NuevoTratamiento.IdVisita &&
            t.IdMedicamento == NuevoTratamiento.IdMedicamento &&
            t.Dosis == NuevoTratamiento.Dosis &&
            t.Frecuencia == NuevoTratamiento.Frecuencia &&
            t.Duracion == NuevoTratamiento.Duracion &&
            t.Cantidad == NuevoTratamiento.Cantidad);

            if (yaExisteIgual)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ya existe un tratamiento idéntico para esta visita. Si es otro, cambia la dosis, la frecuencia o la cantidad.");

                await CargarDatosAsync();
                return Page();
            }

            NuevoTratamiento.EstadoEntrega = "PENDIENTE";
            _context.Tratamientos.Add(NuevoTratamiento);
            await _context.SaveChangesAsync();

            // El tratamiento ya está guardado en la base principal.
            // Ahora solicitamos el descuento al microservicio de pruebas.
            try
            {
                var cliente = _httpClientFactory.CreateClient("Medicamentos");

                cliente.Timeout = TimeSpan.FromSeconds(15);

                var solicitud = new SolicitudEntregaApi
                {
                    IdTratamiento = NuevoTratamiento.IdTratamiento,
                    IdMedicamento = NuevoTratamiento.IdMedicamento,
                    Cantidad = NuevoTratamiento.Cantidad,
                    OrigenTratamiento = "AplicacionPrincipal"
                };

                using var respuesta = await cliente.PostAsJsonAsync(
                    "api/Medicamentos/entregas",
                    solicitud);

                if (respuesta.IsSuccessStatusCode)
                {
                    NuevoTratamiento.EstadoEntrega = "ENTREGADO";
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] =
                        "Tratamiento registrado y medicamento descontado correctamente.";
                }
                else
                {
                    var detalle = await respuesta.Content.ReadAsStringAsync();

                    // Un 200 ya cubre la repetición idéntica; cualquier otro código deja la entrega pendiente.
                    TempData["Mensaje"] =
                        $"La entrega sigue pendiente. Respuesta del inventario: {(int)respuesta.StatusCode}. {detalle}";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                TempData["Mensaje"] =
                    "Tratamiento guardado, pero no se pudo contactar con el inventario. " +
                    "La entrega requiere reintento.";
            }

            return RedirectToPage();

        }
    }
}
