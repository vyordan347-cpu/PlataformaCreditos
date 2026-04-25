using System.ComponentModel.DataAnnotations;

namespace PlataformaCreditos.ViewModels;

public class CrearSolicitudViewModel
{
    [Required(ErrorMessage = "El monto solicitado es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal MontoSolicitado { get; set; }
}