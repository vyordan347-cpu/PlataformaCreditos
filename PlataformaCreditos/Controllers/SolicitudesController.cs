using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditos.Data;
using PlataformaCreditos.ViewModels;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SolicitudesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MisSolicitudes(
        string? estado,
        decimal? montoMin,
        decimal? montoMax,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        if (montoMin < 0)
        {
            ModelState.AddModelError("MontoMin", "El monto mínimo no puede ser negativo.");
        }

        if (montoMax < 0)
        {
            ModelState.AddModelError("MontoMax", "El monto máximo no puede ser negativo.");
        }

        if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
        {
            ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser mayor que la fecha fin.");
        }

        var userId = _userManager.GetUserId(User);

        var query = _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Cliente != null && s.Cliente.UsuarioId == userId);

        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(s => s.Estado == estado);

            if (montoMin.HasValue)
                query = query.Where(s => s.MontoSolicitado >= montoMin.Value);

            if (montoMax.HasValue)
                query = query.Where(s => s.MontoSolicitado <= montoMax.Value);

            if (fechaInicio.HasValue)
                query = query.Where(s => s.FechaSolicitud.Date >= fechaInicio.Value.Date);

            if (fechaFin.HasValue)
                query = query.Where(s => s.FechaSolicitud.Date <= fechaFin.Value.Date);
        }

        var vm = new SolicitudesFiltroViewModel
        {
            Estado = estado,
            MontoMin = montoMin,
            MontoMax = montoMax,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var userId = _userManager.GetUserId(User);

        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s =>
                s.Id == id &&
                s.Cliente != null &&
                s.Cliente.UsuarioId == userId);

        if (solicitud == null)
        {
            return NotFound();
        }

        return View(solicitud);
    }
    public IActionResult Crear()
{
    return View(new CrearSolicitudViewModel());
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Crear(CrearSolicitudViewModel vm)
{
    var userId = _userManager.GetUserId(User);

    var cliente = await _context.Clientes
        .FirstOrDefaultAsync(c => c.UsuarioId == userId);

    if (cliente == null)
    {
        ModelState.AddModelError("", "No existe un cliente asociado a este usuario.");
    }
    else
    {
        if (!cliente.Activo)
        {
            ModelState.AddModelError("", "El cliente no está activo.");
        }

        var tienePendiente = await _context.SolicitudesCredito
            .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");

        if (tienePendiente)
        {
            ModelState.AddModelError("", "No puedes registrar otra solicitud mientras tengas una pendiente.");
        }

        if (vm.MontoSolicitado > cliente.IngresosMensuales * 10)
        {
            ModelState.AddModelError("MontoSolicitado", "El monto solicitado no puede superar 10 veces tus ingresos mensuales.");
        }
    }

    if (!ModelState.IsValid)
    {
        return View(vm);
    }

    var solicitud = new SolicitudCredito
    {
        ClienteId = cliente!.Id,
        MontoSolicitado = vm.MontoSolicitado,
        FechaSolicitud = DateTime.Now,
        Estado = "Pendiente"
    };

    _context.SolicitudesCredito.Add(solicitud);
    await _context.SaveChangesAsync();

    TempData["Mensaje"] = "Solicitud registrada correctamente.";

    return RedirectToAction(nameof(MisSolicitudes));
}
}