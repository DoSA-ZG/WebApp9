using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model zadatka u MD formatu koji sadrži detalje o zadatku.
    /// </summary>
    public class ZadatakMDV2ViewModel
    {
        /// <summary>
        /// Zadatak.
        /// </summary>
        public Zadatak Zadatak { get; set; }

        /// <summary>
        /// Nazivi zaduženih osoba odvojeni zarezom.
        /// </summary>
        public String NaziviZadOsoba { get; set; }

        /// <summary>
        /// Identifikator zadatka.
        /// </summary>
        public int IdZadatak { get; set; }

        /// <summary>
        /// Vrijeme isporuke zadatka.
        /// </summary>
        public DateTime VrijemeIsporuke { get; set; }

        /// <summary>
        /// Stupanj dovršenosti zadatka.
        /// </summary>
        public int StupanjDovrsenosti { get; set; }

        /// <summary>
        /// Prioritetnost zadatka.
        /// </summary>
        public string Prioritetnost { get; set; }

        /// <summary>
        /// Identifikator osobe zadužene za zadatak.
        /// </summary>
        public int IdOsoba { get; set; }

        /// <summary>
        /// Identifikator vrste zadatka.
        /// </summary>
        public int IdVrstaZdtk { get; set; }

        /// <summary>
        /// Naziv zadatka.
        /// </summary>
        public string Naziv { get; set; }

        /// <summary>
        /// Identifikator zahtjeva.
        /// </summary>
        public int? IdZahtjev { get; set; }

        /// <summary>
        /// Osoba zadužena za zadatak.
        /// </summary>

        public virtual Osoba Osoba { get; set; }

        /// <summary>
        /// Vrsta zadatka.
        /// </summary>
        public virtual VrstaZadatka VrstaZadatka { get; set; }

        /// <summary>
        /// Zahtjev kojem je zadatak pridružen.
        /// </summary>
        public virtual Zahtjev Zahtjev { get; set; }

        /// <summary>
        /// Lista osoba zaduženih za zadatak.
        /// </summary>
        public virtual ICollection<ZaduzenaOsoba> ZaduzenaOsoba { get; set; } = new List<ZaduzenaOsoba>();

        /// <summary>
        /// Brojač.
        /// </summary>
        public int count = 0;
    }
}