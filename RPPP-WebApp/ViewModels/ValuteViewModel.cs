using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ValuteViewModel
    {
        public IEnumerable<Valuta> Valute { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}