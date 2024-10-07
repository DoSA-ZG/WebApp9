using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži kolekciju uloga i informacije o straničenju.
    /// </summary>
    public class UlogeV2ViewModel
    {
        /// <summary>
        /// Kolekcija uloga.
        /// </summary>
        public IEnumerable<Uloga> Uloge { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}