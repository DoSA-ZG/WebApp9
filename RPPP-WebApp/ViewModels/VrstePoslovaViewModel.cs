using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class VrstePoslovaViewModel
    {
        public IEnumerable<VrstaPosla> VrstePoslova { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}