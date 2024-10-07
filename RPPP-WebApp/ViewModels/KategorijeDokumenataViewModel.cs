using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class KategorijeDokumenataViewModel
    {
        public IEnumerable<KategorijaDokumenta> KategorijeDokumenata { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}