using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class SuradniciViewModel
    {
        public IEnumerable<Suradnik> Suradnici { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}