using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektiMDViewModel
    {
        public IEnumerable<ProjektMDViewModel> Projekti { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
