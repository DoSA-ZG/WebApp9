using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
	public class VrsteSuradnikaViewModel
	{
		public IEnumerable<VrstaSuradnika> VrsteSuradnika { get; set; }
		public PagingInfo PagingInfo { get; set; }
	}
}