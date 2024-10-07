using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    public class ValutaValidator : AbstractValidator<Valuta>
    {
        public ValutaValidator()
        {
            RuleFor(d => d.IsoOznaka)
                .NotEmpty().WithMessage("Naziv je obavezno polje");
        }
    }
}