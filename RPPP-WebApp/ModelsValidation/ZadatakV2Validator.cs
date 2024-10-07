using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Validator za validaciju objekata tipa Zadatak.
    /// </summary>
    public class ZadatakV2Validator : AbstractValidator<Zadatak>
    {
        /// <summary>
        /// Konstruktor koji konfigurira pravila validacije za objekte tipa Zadatak.
        /// </summary>
        public ZadatakV2Validator()
        {
            RuleFor(z => z.VrijemeIsporuke)
              .NotEmpty().WithMessage("Vrijeme isporuke je obavezno polje!");

            RuleFor(z => z.StupanjDovrsenosti)
                .GreaterThanOrEqualTo(0)
                .InclusiveBetween(0, 100).WithMessage("Stupanj dovršenosti mora biti između 0 i 100!");

            RuleFor(z => z.Prioritetnost)
              .NotEmpty().WithMessage("Prioritetnost je obavezno polje!");

            RuleFor(z => z.IdOsoba)
              .NotEmpty().WithMessage("Osoba je obavezno polje!");

            RuleFor(z => z.IdVrstaZdtk)
              .NotEmpty().WithMessage("Vrsta zadatka je obavezno polje!");
        }
    }
}