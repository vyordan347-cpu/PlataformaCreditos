using PlataformaCreditos.Models;

namespace PlataformaCreditos.ViewModels;

public class SolicitudesFiltroViewModel
{
    public string? Estado { get; set; }

    public decimal? MontoMin { get; set; }

    public decimal? MontoMax { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public List<SolicitudCredito> Solicitudes { get; set; } = new();
}