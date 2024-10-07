using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZaduzenaOsobaViewModel
    {
		public IEnumerable<ZaduzenaOsoba> ZaduženeOsobe { get; set; }
		public PagingInfo PagingInfo { get; set; }
    }
}
