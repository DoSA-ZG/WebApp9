using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class RacunMDViewModel
    {
        public Racun Racun { get; set; }
        public int StanjeRacuna { get; set; }

        public int IdRacuna { get; set; }

        public string ImeRacuna { get; set; }

        public string Iban { get; set; }

        public int? IdValuta { get; set; }

        public virtual Valuta Valuta { get; set; }
        public virtual ICollection<Transakcija> Transakcija { get; set; } = new List<Transakcija>();

        public int Count = 0;
    }
}