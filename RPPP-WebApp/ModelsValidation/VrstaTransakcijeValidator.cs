using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    public class VrstaTransakcijeValidator : AbstractValidator<VrstaTransakcije>
    {
        public VrstaTransakcijeValidator()
        {
            RuleFor(d => d.Naziv)
                .NotEmpty().WithMessage("Naziv je obavezno polje");
        }
    }
}