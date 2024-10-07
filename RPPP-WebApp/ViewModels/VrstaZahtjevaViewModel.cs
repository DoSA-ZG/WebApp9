using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaZahtjevaViewModel
    {
        public IEnumerable<VrstaZahtjeva> VrsteZahtjeva { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}