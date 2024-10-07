using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektiViewModel
    {
        public IList<Projekt> Projekti { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
