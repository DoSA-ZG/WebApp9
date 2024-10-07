using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class TransakcijaValidator : AbstractValidator<Transakcija>
    {
        public TransakcijaValidator()
        {
            RuleFor(d => d.Idracuna)
                .NotEmpty().WithMessage("Račun je obavezno polje");

            RuleFor(d => d.Iznos)
                .NotEmpty().WithMessage("Iznos je obavezno polje");
            RuleFor(d => d.Iznos)
                .GreaterThan(0).WithMessage("Iznos transakcije mora biti pozitivan broj");

            RuleFor(d => d.IdvrstaTransakcije)
                .NotEmpty().WithMessage("Vrsta transakcije je obavezno polje");

            RuleFor(d => d.SmjerTransakcije != "U" && d.SmjerTransakcije != "V")
                .Equal(false).WithMessage("Smjer transakcije mora biti unutarnji ili vanjski");

            RuleFor(d => d.SmjerTransakcije == "U" && d.UnutarnjiIdRacuna == 0)
                .Equal(false).WithMessage("Unutarnje transakcije moraju imati odabrani račun");

            RuleFor(d => d.SmjerTransakcije == "V" && d.VanjskiIban.IsNullOrEmpty())
                .Equal(false).WithMessage("Vanjske transakcije moraju imati IBAN");
        }
    }
}