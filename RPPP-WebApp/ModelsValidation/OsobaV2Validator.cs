using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Validator za validaciju objekata tipa Osoba.
    /// </summary>
    public class OsobaV2Validator : AbstractValidator<Osoba>
    {
        /// <summary>
        /// Konstruktor koji konfigurira pravila validacije za objekte tipa Osoba.
        /// </summary>
        public OsobaV2Validator()
        {
            RuleFor(o => o.ImeOsobe)
                .NotEmpty().WithMessage("Ime je obavezno polje!");

            RuleFor(o => o.PrezimeOsobe)
                .NotEmpty().WithMessage("Prezime je obavezno polje!");

            RuleFor(o => o.Email)
                .NotEmpty().WithMessage("E-mail je obavezno polje!");

            RuleFor(o => o.Telefon)
                .NotEmpty().WithMessage("Telefon je obavezno polje!");

            RuleFor(o => o.Iban)
                .NotEmpty().WithMessage("IBAN je obavezno polje!");
        }
    }
}