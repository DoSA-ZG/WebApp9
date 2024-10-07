using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
	public class PosaoValidator : AbstractValidator<Posao>
	{
		public PosaoValidator()
		{
			RuleFor(d => d.ProjektId)
			  .NotEmpty().WithMessage("Projekt je obavezno polje");

			RuleFor(d => d.Naziv)
				.NotEmpty().WithMessage("Naziv je obavezno polje");

			RuleFor(d => d.OcekivaniPocetak)
			  .NotEmpty().WithMessage("Očekivani početak je obavezno polje");

			RuleFor(d => d.OcekivaniZavrsetak)
			  .NotEmpty().WithMessage("Ocekivani zavrsetak je obavezno polje");

			RuleFor(d => d.Budzet)
			  .NotEmpty().WithMessage("Budzet je obavezno polje");

			RuleFor(d => d.VrstaPoslaId)
			  .NotEmpty().WithMessage("Vrsta posla je obavezno polje");
		}
	}
}
