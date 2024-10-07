using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class PosloviMDViewModel
    {
        public IEnumerable<PosaoMDViewModel> Poslovi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}