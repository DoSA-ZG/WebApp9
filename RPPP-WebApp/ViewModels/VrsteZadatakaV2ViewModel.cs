using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži kolekciju vrsta zadataka i informacije o straničenju.
    /// </summary>
    public class VrsteZadatakaV2ViewModel
    {
        /// <summary>
        /// Kolekcija vrsta zadataka.
        /// </summary>
        public IEnumerable<VrstaZadatka> VrsteZadataka { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}