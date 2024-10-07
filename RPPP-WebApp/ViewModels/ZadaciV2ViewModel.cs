using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži listu zadataka i informacije o straničenju.
    /// </summary>
    public class ZadaciV2ViewModel
    {
        /// <summary>
        /// Lista zadataka.
        /// </summary>
        public IList<Zadatak> Zadaci { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}