using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
	public class SuradnikValidator : AbstractValidator<Suradnik>
	{
		public SuradnikValidator()
		{
			RuleFor(d => d.Naziv)
			  .NotEmpty().WithMessage("Naziv je obavezno polje");

			RuleFor(d => d.Oib)
				.NotEmpty().WithMessage("OIB je obavezno polje");
			RuleFor(d => d.Oib)
				.Length(11).WithMessage("Duljina OIB-a mora biti 11.");

			RuleFor(d => d.Adresa)
			  .NotEmpty().WithMessage("Adresa je obavezno polje");

			RuleFor(d => d.PostanskiBroj)
			  .NotEmpty().WithMessage("Postanski broj je obavezno polje");

			RuleFor(d => d.Grad)
			  .NotEmpty().WithMessage("Grad je obavezno polje");

			RuleFor(d => d.VrstaSuradnikaId)
			  .NotEmpty().WithMessage("Vrsta suradnika je obavezno polje");

			RuleFor(d => d.PosaoId)
				.NotEmpty().WithMessage("Morate odabrati posao na kojemu radi suradnik");
		}
	}
}
