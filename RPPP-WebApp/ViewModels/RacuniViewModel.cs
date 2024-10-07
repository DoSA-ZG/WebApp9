using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class RacuniViewModel
    {
        public IEnumerable<Racun> Racuni { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}