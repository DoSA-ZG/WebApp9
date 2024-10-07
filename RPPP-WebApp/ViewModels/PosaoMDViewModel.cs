using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PosaoMDViewModel
    {
        public Posao Posao { get; set; }
        public String NaziviSuradnika { get; set; }

        public int Id { get; set; }

        public int ProjektId { get; set; }

        public string Naziv { get; set; }

        public DateTime OcekivaniPocetak { get; set; }

        public DateTime OcekivaniZavrsetak { get; set; }

        public int Budzet { get; set; }

        public int VrstaPoslaId { get; set; }

        public virtual Projekt Projekt { get; set; }

        public virtual ICollection<Suradnik> Suradnik { get; set; } = new List<Suradnik>();

        public virtual VrstaPosla VrstaPosla { get; set; }

        public int count = 0;
    }
}
