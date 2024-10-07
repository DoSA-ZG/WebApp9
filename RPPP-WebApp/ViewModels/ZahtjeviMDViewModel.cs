using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjeviMDViewModel
    {
        public IEnumerable<ZahtjevMDViewModel> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}