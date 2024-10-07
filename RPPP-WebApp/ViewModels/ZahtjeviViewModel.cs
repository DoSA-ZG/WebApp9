using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjeviViewModel
    {
        public IEnumerable<Zahtjev> Zahtjevi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}