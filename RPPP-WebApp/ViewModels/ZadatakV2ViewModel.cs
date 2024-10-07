using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model za prikaz liste zadataka zajedno s informacijama o straničenju.
    /// </summary>
    public class ZadatakV2ViewModel
    {
        /// <summary>
        /// Lista zadataka.
        /// </summary>
        public IEnumerable<Zadatak> Zadaci { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}