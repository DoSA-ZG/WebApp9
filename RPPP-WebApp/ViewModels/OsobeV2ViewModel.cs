using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži kolekciju osoba i informacije o straničenju.
    /// </summary>
    public class OsobeV2ViewModel
    {
        /// <summary>
        /// Kolekcija osoba.
        /// </summary>
        public IEnumerable<Osoba> Osobe { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}