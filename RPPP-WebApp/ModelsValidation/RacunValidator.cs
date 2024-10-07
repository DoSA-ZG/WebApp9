using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    public class RacunValidator : AbstractValidator<Racun>
    {
        public RacunValidator()
        {
            RuleFor(d => d.ImeRacuna)
                .NotEmpty().WithMessage("Naziv računa je obavezno polje");

            RuleFor(d => d.Iban)
                .NotEmpty().WithMessage("IBAN je obavezno polje");

            RuleFor(d => d.IdValuta)
                .NotEmpty().WithMessage("Valuta je obavezno polje");
        }
    }
}