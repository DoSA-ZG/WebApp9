using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    /// <summary>
    /// Prikazni model koji sadrži kolekciju zadatka u MD formatu i informacije o straničenju.
    /// </summary>
    public class ZadaciMDV2ViewModel
    {
        /// <summary>
        /// Kolekcija zadatka u MD formatu.
        /// </summary>
        public IEnumerable<ZadatakMDV2ViewModel> Zadaci { get; set; }

        /// <summary>
        /// Informacije o straničenju.
        /// </summary>
        public PagingInfo PagingInfo { get; set; }
    }
}