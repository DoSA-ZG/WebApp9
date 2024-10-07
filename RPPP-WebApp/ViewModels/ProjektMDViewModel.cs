using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektMDViewModel
    {
        public Projekt Projekt { get; set; }
        public string NaziviPoslova { get; set; }
        public string ImenaRadnika {  get; set; }
        public string Dokumenti {  get; set; } 
        public IEnumerable<Osoba> RadniciList { get; set; }
        public virtual ICollection<Dokument> Dokument { get; set; } = new List<Dokument>();
        public virtual ICollection<Posao> Posao { get; set; } = new List<Posao>();
        public virtual ICollection<UlogaNaProjektu> UlogaNaProjektu { get; set; } = new List<UlogaNaProjektu>();

        public int count = 0;
    }
}
