using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
    public class VrstaPoslaValidator : AbstractValidator<VrstaPosla>
    {
        public VrstaPoslaValidator()
        {
            RuleFor(d => d.Naziv)
              .NotEmpty().WithMessage("Naziv je obavezno polje");
        }
    }
}
