using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class VrsteTransakcijeViewModel
    {
        public IEnumerable<VrstaTransakcije> VrsteTransakcija { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}