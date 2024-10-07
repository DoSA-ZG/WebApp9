using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ValutaSelector
    {
        public static IQueryable<Valuta> ApplySort(this IQueryable<Valuta> query, int sort,
            bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Valuta, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.IsoOznaka;
                    break;
            }

            if (orderSelector != null)
            {
                query = ascending ? query.OrderBy(orderSelector) : query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}