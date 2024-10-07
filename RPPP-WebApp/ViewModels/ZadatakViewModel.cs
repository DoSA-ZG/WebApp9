using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZadatakViewModel
    {
        public IEnumerable<Zadatak> Zadaci { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
