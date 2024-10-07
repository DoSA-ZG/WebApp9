using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ZadatakSort
    {
        public static IQueryable<Zadatak> ApplySort(this IQueryable<Zadatak> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Zadatak, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.Prioritetnost;
                    break;
                case 3:
                    orderSelector = d => d.VrijemeIsporuke;
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
