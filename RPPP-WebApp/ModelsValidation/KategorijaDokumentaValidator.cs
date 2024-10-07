using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class KategorijaDokumentaValidator : AbstractValidator<KategorijaDokumenta>
    {
        public KategorijaDokumentaValidator()
        {
            RuleFor(k => k.NazivKategorijeDokumenta)
                .NotEmpty().WithMessage("Naziv je obavezan");
        }
    }
}
