using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class KategorijaDokumentaSelector
    {
        public static IQueryable<KategorijaDokumenta> ApplySort(this IQueryable<KategorijaDokumenta> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<KategorijaDokumenta, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.NazivKategorijeDokumenta;
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
