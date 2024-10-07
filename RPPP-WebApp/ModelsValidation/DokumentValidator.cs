using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class DokumentValidator : AbstractValidator<Dokument>
    {
        public DokumentValidator() 
        {
            RuleFor(d => d.Naslov)
                .NotEmpty().WithMessage("Naslov je obavezan")
                .Length(0, 20).WithMessage("Naslov može imati najviše 20 znakova.");

            RuleFor(d => d.ProjektId)
                .NotEmpty().WithMessage("Projekt je obavezan");

            RuleFor(d => d.KategorijaDokumentaId)
                .NotEmpty().WithMessage("Kategorija je obavezna");
        }
    }
}
