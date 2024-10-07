using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    /// <summary>
    /// Validator za validaciju objekata tipa VrstaZadatka.
    /// </summary>
    public class VrstaZadatkaV2Validator : AbstractValidator<VrstaZadatka>
    {
        /// <summary>
        /// Konstruktor koji konfigurira pravila validacije za objekte tipa VrstaZadatka.
        /// </summary>
        public VrstaZadatkaV2Validator()
        {
            RuleFor(v => v.NazivVrsteZdtk)
              .NotEmpty().WithMessage("Naziv je obavezno polje!");
        }
    }
}