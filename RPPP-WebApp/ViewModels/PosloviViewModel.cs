using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class PosloviViewModel
    {
        public IEnumerable<Posao> Poslovi { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}