using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PlataformaCreditos.Data;
using PlataformaCreditos.Models;
using PlataformaCreditos.ViewModels;
using System.Text.Json;

namespace PlataformaCreditos.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IDistributedCache _cache;

    public SolicitudesController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IDistributedCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<IActionResult> MisSolicitudes(
    string? estado,
    decimal? montoMin,
    decimal? montoMax,
    DateTime? fechaInicio,
    DateTime? fechaFin)
{
    if (montoMin < 0)
        ModelState.AddModelError("MontoMin", "El monto mínimo no puede ser negativo.");

    if (montoMax < 0)
        ModelState.AddModelError("MontoMax", "El monto máximo no puede ser negativo.");

    if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio > fechaFin)
        ModelState.AddModelError("FechaInicio", "La fecha de inicio no puede ser mayor que la fecha fin.");

    var userId = _userManager.GetUserId(User);
    var cacheKey = $"solicitudes_{userId}";

    List<SolicitudCredito>? solicitudes;

    var cacheData = await _cache.GetStringAsync(cacheKey);

    if (cacheData != null)
    {
        solicitudes = JsonSerializer.Deserialize<List<SolicitudCredito>>(cacheData) ?? new List<SolicitudCredito>();
    }
    else
    {
        solicitudes = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Cliente != null && s.Cliente.UsuarioId == userId)
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new SolicitudCredito
            {
                Id = s.Id,
                ClienteId = s.ClienteId,
                MontoSolicitado = s.MontoSolicitado,
                FechaSolicitud = s.FechaSolicitud,
                Estado = s.Estado,
                MotivoRechazo = s.MotivoRechazo
            })
            .ToListAsync();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(solicitudes),
            options);
    }

    if (ModelState.IsValid)
    {
        if (!string.IsNullOrWhiteSpace(estado))
            solicitudes = solicitudes.Where(s => s.Estado == estado).ToList();

        if (montoMin.HasValue)
            solicitudes = solicitudes.Where(s => s.MontoSolicitado >= montoMin.Value).ToList();

        if (montoMax.HasValue)
            solicitudes = solicitudes.Where(s => s.MontoSolicitado <= montoMax.Value).ToList();

        if (fechaInicio.HasValue)
            solicitudes = solicitudes.Where(s => s.FechaSolicitud.Date >= fechaInicio.Value.Date).ToList();

        if (fechaFin.HasValue)
            solicitudes = solicitudes.Where(s => s.FechaSolicitud.Date <= fechaFin.Value.Date).ToList();
    }

    var vm = new SolicitudesFiltroViewModel
    {
        Estado = estado,
        MontoMin = montoMin,
        MontoMax = montoMax,
        FechaInicio = fechaInicio,
        FechaFin = fechaFin,
        Solicitudes = solicitudes
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
            return NotFound();

        HttpContext.Session.SetString("UltimaSolicitudId", solicitud.Id.ToString());
        HttpContext.Session.SetString("UltimaSolicitudMonto", solicitud.MontoSolicitado.ToString());

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
                ModelState.AddModelError("", "El cliente no está activo.");

            var tienePendiente = await _context.SolicitudesCredito
                .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");

            if (tienePendiente)
                ModelState.AddModelError("", "No puedes registrar otra solicitud mientras tengas una pendiente.");

            if (vm.MontoSolicitado > cliente.IngresosMensuales * 10)
                ModelState.AddModelError("MontoSolicitado", "El monto solicitado no puede superar 10 veces tus ingresos mensuales.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        var solicitud = new SolicitudCredito
        {
            ClienteId = cliente!.Id,
            MontoSolicitado = vm.MontoSolicitado,
            FechaSolicitud = DateTime.Now,
            Estado = "Pendiente"
        };

        _context.SolicitudesCredito.Add(solicitud);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"solicitudes_{userId}");

        TempData["Mensaje"] = "Solicitud registrada correctamente.";

        return RedirectToAction(nameof(MisSolicitudes));
    }
}