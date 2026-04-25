using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlataformaCreditos.Data;

namespace PlataformaCreditos.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnalistaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var solicitudes = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Estado == "Pendiente")
            .ToListAsync();

        return View(solicitudes);
    }

    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado != "Pendiente")
        {
            TempData["Error"] = "La solicitud ya fue procesada.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.MontoSolicitado > solicitud.Cliente!.IngresosMensuales * 5)
        {
            TempData["Error"] = "No se puede aprobar: supera 5 veces los ingresos.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = "Aprobado";

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Solicitud aprobada.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await _context.SolicitudesCredito.FindAsync(id);

        if (solicitud == null)
            return NotFound();

        return View(solicitud);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string motivo)
    {
        var solicitud = await _context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(motivo))
        {
            ModelState.AddModelError("", "El motivo es obligatorio.");
            return View(solicitud);
        }

        if (solicitud.Estado != "Pendiente")
        {
            TempData["Error"] = "La solicitud ya fue procesada.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = "Rechazado";
        solicitud.MotivoRechazo = motivo;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Solicitud rechazada.";

        return RedirectToAction(nameof(Index));
    }
}