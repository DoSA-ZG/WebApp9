using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Validator za validaciju objekata tipa Uloga.
    /// </summary>
    public class UlogaV2Validator : AbstractValidator<Uloga>
    {
        /// <summary>
        /// Konstruktor koji konfigurira pravila validacije za objekte tipa Uloga.
        /// </summary>
        public UlogaV2Validator()
        {
            RuleFor(u => u.NazivUloge)
                .NotEmpty().WithMessage("Naziv je obavezno polje!");
        }
    }
}