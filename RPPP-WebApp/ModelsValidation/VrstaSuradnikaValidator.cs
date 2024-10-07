using RPPP_WebApp.Models;
using FluentValidation;

namespace RPPP_WebApp.ModelsValidation
{
	public class VrstaSuradnikaValidator : AbstractValidator<VrstaSuradnika>
	{
		public VrstaSuradnikaValidator()
		{
			RuleFor(d => d.Naziv)
			  .NotEmpty().WithMessage("Naziv je obavezno polje");

			RuleFor(d => d.Opis)
				.NotEmpty().WithMessage("Opis je obavezno polje");
		}
	}
}
