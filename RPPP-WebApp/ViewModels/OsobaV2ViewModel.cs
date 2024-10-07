using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži podatke o osobi, ulozi, projektu i zadatku.
    /// </summary>
    public class OsobaV2ViewModel
    {
        /// <summary>
        /// Osoba koja se prikazuje.
        /// </summary>
        public Osoba Osoba { get; set; }

        /// <summary>
        /// Uloga osobe.
        /// </summary>
        public Uloga Uloga { get; set; }

        /// <summary>
        /// Projekt na kojem je osoba uključena.
        /// </summary>
        public Projekt Projekt { get; set; }

        /// <summary>
        /// Zadatak za koji je osoba zadužena.
        /// </summary>
        public Zadatak Zadatak { get; set; }
    }
}
