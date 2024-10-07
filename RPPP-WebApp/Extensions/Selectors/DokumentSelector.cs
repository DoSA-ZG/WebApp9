using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class DokumentSelector
    {
        public static IQueryable<Dokument> ApplySort(this IQueryable<Dokument> query,  int sort, bool ascending) 
        {
            System.Linq.Expressions.Expression<Func<Dokument, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.Naslov;
                    break;
                case 2:
                    orderSelector = p => p.Projekt.ImeProjekta;
                    break;
                case 3:
                    orderSelector= p => p.KategorijaDokumenta.NazivKategorijeDokumenta;
                    break;
                case 4:
                    orderSelector = p => p.Sadrzaj;
                    break;
            }
            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
