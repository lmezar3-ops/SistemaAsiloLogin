
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaAsiloLogin.Data;
using SistemaAsiloLogin.Models;
using System.Net;
using System.Net.Mail;

namespace SistemaAsiloLogin.Pages
{
    [Authorize(Roles = "Administrador,MedicoGeneral,Medico")]
    public class SolicitudesMedicasModel : PageModel
    {
        private readonly AsiloDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SolicitudesMedicasModel> _logger;

        public SolicitudesMedicasModel(
            AsiloDbContext context,
            IConfiguration configuration,
            ILogger<SolicitudesMedicasModel> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public List<Paciente> Pacientes { get; set; } = new();
        public List<Medico> Medicos { get; set; } = new();
        public List<Especialidad> Especialidades { get; set; } = new();
        public List<Enfermero> Enfermeros { get; set; } = new();
        public List<SolicitudMedica> Solicitudes { get; set; } = new();

        [BindProperty]
        public SolicitudMedica Solicitud { get; set; } = new();

        private void CargarCombos()
        {
            Pacientes = _context.Pacientes
                .Where(p => p.Estado).ToList();

            Medicos = _context.Medicos
                .Where(m => m.Estado).ToList();

            Especialidades = _context.Especialidades.ToList();

            Enfermeros = _context.Enfermeros
                .Where(e => e.Estado).ToList();
        }

        private void CargarSolicitudes()
        {
            Solicitudes = _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .Include(s => s.MedicoGeneral)
                .Include(s => s.Especialidad)
                .Include(s => s.Enfermero)
                .OrderByDescending(s => s.IdSolicitud)
                .ToList();
        }

        public void OnGet()
        {
            CargarCombos();
            CargarSolicitudes();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CargarCombos();

            if (!ModelState.IsValid)
            {
                CargarSolicitudes();
                return Page();
            }

            var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p =>
             p.IdPaciente == Solicitud.IdPaciente &&
             p.Estado);

            var medico = await _context.Medicos
                .FirstOrDefaultAsync(m =>
                    m.IdMedico == Solicitud.IdMedicoGeneral &&
                    m.Estado);

            var especialidadExiste = await _context.Especialidades
                .AnyAsync(e =>
                    e.IdEspecialidad == Solicitud.IdEspecialidad);

            var enfermeroValido = Solicitud.IdEnfermero == null ||
             await _context.Enfermeros.AnyAsync(e =>
                   e.IdEnfermero == Solicitud.IdEnfermero &&
                   e.Estado);

            if (paciente == null || medico == null ||
                !especialidadExiste || !enfermeroValido)
            {
                ModelState.AddModelError(string.Empty,
                    "Seleccione un paciente, médico y especialidad válidos.");

                CargarSolicitudes();
                return Page();
            }

            // Evitar que el formulario envíe un estado arbitrario.
            Solicitud.Estado = "Pendiente";

            _context.SolicitudesMedicas.Add(Solicitud);
            await _context.SaveChangesAsync();

            // La solicitud ya está guardada.
            // Un fallo de correo no debe deshacer el registro.
            try
            {

                var correos = await _context.PacientesFamiliares
                    .Where(pf =>
                        pf.IdPaciente == Solicitud.IdPaciente &&
                        pf.Familiar != null &&
                        pf.Familiar.Estado &&
                        pf.Familiar.CorreoElectronico != null &&
                        pf.Familiar.CorreoElectronico.Trim() != "")
                    .OrderByDescending(pf => pf.EsResponsable)
                    .Select(pf => pf.Familiar!.CorreoElectronico!)
                    .Distinct()
                    .ToListAsync();


                var host = _configuration["Email:SmtpHost"];
                var from = _configuration["Email:From"];
                var user = _configuration["Email:SmtpUser"];
                var password = _configuration["Email:SmtpPassword"];

                var portText = _configuration["Email:SmtpPort"];
                var sslText = _configuration["Email:EnableSsl"];

                if (correos.Count == 0)
                {
                    TempData["AvisoCorreo"] =
                        "Solicitud registrada, pero el paciente no tiene familiares con correo registrado.";
                }
                else if (string.IsNullOrWhiteSpace(host) ||
                         string.IsNullOrWhiteSpace(from) ||
                         string.IsNullOrWhiteSpace(user) ||
                         string.IsNullOrWhiteSpace(password))
                {
                    TempData["AvisoCorreo"] =
                        "Solicitud registrada. Falta configurar el servicio de correo.";
                }
                else
                {
                    int port = int.TryParse(portText, out var p)
                        ? p : 587;

                    bool enableSsl = bool.TryParse(
                        sslText, out var ssl) ? ssl : true;

                    using var smtp = new SmtpClient(host, port)
                    {
                        EnableSsl = enableSsl,
                        Credentials = new NetworkCredential(user, password)
                    };


                    int enviados = 0;
                    int fallidos = 0;

                    foreach (var correo in correos)
                    {
                        try
                        {
                            using var mensaje = new MailMessage();
                            mensaje.From = new MailAddress(from);
                            mensaje.To.Add(correo!);

                            mensaje.Subject =
                                "Aviso de solicitud médica - Asilo Cabeza de Algodón";

                            mensaje.Body =
                                $"Estimado familiar:\n\n" +
                                $"Se ha registrado una solicitud médica para " +
                                $"{paciente.Nombres} {paciente.Apellidos}.\n\n" +
                                $"Fecha: {Solicitud.FechaSolicitud:dd/MM/yyyy HH:mm}\n" +
                                $"Motivo: {Solicitud.Motivo}\n" +
                                $"Médico remitente: {medico.Nombres} {medico.Apellidos}\n" +
                                $"Estado: Pendiente\n\n" +
                                "Este mensaje es una notificación del sistema del asilo.";

                            await smtp.SendMailAsync(mensaje);
                            enviados++;
                        }
                        catch (Exception ex)
                        {
                            fallidos++;

                            _logger.LogError(
                                ex,
                                "Falló el envío de correo para la solicitud {IdSolicitud} al paciente {IdPaciente}.",
                                Solicitud.IdSolicitud,
                                paciente.IdPaciente);
                        }
                    }

                    if (fallidos == 0)
                    {
                        TempData["AvisoCorreo"] =
                            $"Solicitud registrada. El servidor aceptó el envío de {enviados} correo(s).";
                    }
                    else if (enviados == 0)
                    {
                        TempData["AvisoCorreo"] =
                            "Solicitud registrada, pero fallaron todos los envíos. Revisa los registros de errores.";
                    }
                    else
                    {
                        TempData["AvisoCorreo"] =
                            $"Solicitud registrada. Envíos aceptados: {enviados}. Fallidos: {fallidos}. Revisa los registros.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al preparar los correos de la solicitud {IdSolicitud}.",
                    Solicitud.IdSolicitud);

                TempData["AvisoCorreo"] =
                    "Solicitud registrada, pero no se pudieron procesar los correos.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReenviarCorreoAsync(int idSolicitud)
        {
            var solicitud = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .Include(s => s.MedicoGeneral)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud == null ||
                solicitud.Paciente == null ||
                solicitud.MedicoGeneral == null)
            {
                TempData["AvisoCorreo"] =
                    "No se encontró la solicitud o faltan datos asociados.";

                return RedirectToPage();
            }

            var correos = await _context.PacientesFamiliares
                .Where(pf => pf.IdPaciente == solicitud.IdPaciente &&
                             pf.Familiar != null &&
                             pf.Familiar.Estado &&
                             pf.Familiar.CorreoElectronico != null &&
                             pf.Familiar.CorreoElectronico.Trim() != "")
                .OrderByDescending(pf => pf.EsResponsable)
                .Select(pf => pf.Familiar!.CorreoElectronico!)
                .Distinct()
                .ToListAsync();

            if (correos.Count == 0)
            {
                TempData["AvisoCorreo"] =
                    "No hay familiares activos con correo registrado para este paciente.";

                return RedirectToPage();
            }

            var host = _configuration["Email:SmtpHost"];
            var from = _configuration["Email:From"];
            var user = _configuration["Email:SmtpUser"];
            var password = _configuration["Email:SmtpPassword"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["AvisoCorreo"] =
                    "Falta configurar el servicio de correo.";

                return RedirectToPage();
            }

            try
            {
                var port = int.TryParse(
                    _configuration["Email:SmtpPort"], out var p) ? p : 587;


              var enableSsl = bool.TryParse(
               _configuration["Email:EnableSsl"], out var ssl) ? ssl : true;


                using var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(user, password)
                };

                int enviados = 0;
                int fallidos = 0;

                foreach (var correo in correos)
                {
                    try
                    {
                        using var mensaje = new MailMessage();
                        mensaje.From = new MailAddress(from);
                        mensaje.To.Add(correo);

                        mensaje.Subject =
                            "Aviso de solicitud médica - Asilo Cabeza de Algodón";

                        mensaje.Body =
                            $"Estimado familiar:\n\n" +
                            $"Se mantiene registrada una solicitud médica para " +
                            $"{solicitud.Paciente.Nombres} {solicitud.Paciente.Apellidos}.\n\n" +
                            $"Fecha: {solicitud.FechaSolicitud:dd/MM/yyyy HH:mm}\n" +
                            $"Motivo: {solicitud.Motivo}\n" +
                            $"Médico remitente: {solicitud.MedicoGeneral.Nombres} " +
                            $"{solicitud.MedicoGeneral.Apellidos}\n" +
                            $"Estado: {solicitud.Estado}\n\n" +
                            "Este mensaje es una notificación del sistema del asilo.";

                        await smtp.SendMailAsync(mensaje);
                        enviados++;
                    }
                    catch (Exception ex)
                    {
                        fallidos++;

                        _logger.LogError(
                            ex,
                            "Falló el reenvío de correo de la solicitud {IdSolicitud}.",
                            idSolicitud);
                    }
                }

                if (fallidos == 0)
                {
                    TempData["AvisoCorreo"] =
                        $"Gmail aceptó el envío de {enviados} correo(s). " +
                        "Esto no garantiza su entrega en la bandeja de entrada.";
                }
                else
                {
                    TempData["AvisoCorreo"] =
                        $"Envíos aceptados: {enviados}. Fallidos: {fallidos}. " +
                        "Revisa los registros de errores.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al preparar el reenvío de la solicitud {IdSolicitud}.",
                    idSolicitud);

                TempData["AvisoCorreo"] =
                    "No se pudo procesar el reenvío del correo. Revisa los registros.";
            }

            return RedirectToPage();
        }

    }
}
