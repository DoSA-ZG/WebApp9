using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrsteProjekataViewModel
    {
        public IEnumerable<VrstaProjekta> VrsteProjekata { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
