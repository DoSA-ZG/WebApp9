using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ZaduzenaOsobaSelector
    {
        public static IQueryable<ZaduzenaOsoba> ApplySort(this IQueryable<ZaduzenaOsoba> query, int sort,
            bool ascending)
        {
            System.Linq.Expressions.Expression<Func<ZaduzenaOsoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.IdOsoba;
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