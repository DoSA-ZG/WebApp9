using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    public class ProjektValidator : AbstractValidator<Projekt>
    {
        public ProjektValidator()
        {
            RuleFor(p => p.ImeProjekta)
              .NotEmpty().WithMessage("Naziv je obavezno polje");

            RuleFor(p => p.PocetakProjekta)
              .NotEmpty().WithMessage("Datum početka projekta je obavezno polje");

            RuleFor(p => p.KrajProjekta)
              .NotEmpty().WithMessage("Datum završetka projekta je obavezno polje")
              .Must((projekt, krajProjekta) => krajProjekta > projekt.PocetakProjekta)
                .WithMessage("Datum završetka projekta mora bit nakon datuma početka projekta");

            RuleFor(p => p.Oznaka)
              .NotEmpty().WithMessage("Oznaka je obavezno polje");

            RuleFor(p => p.IdVrstaProjekta)
              .NotEmpty().WithMessage("Vrsta projekta je obavezno polje");
            
            RuleFor(p => p.IdRacuna)
              .NotEmpty().WithMessage("Račun je obavezno polje");

        }
    }
}
