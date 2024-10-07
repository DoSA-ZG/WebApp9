using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaZahtjevaSelector
    {
        public static IQueryable<VrstaZahtjeva> ApplySort(this IQueryable<VrstaZahtjeva> query, int sort,
            bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaZahtjeva, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.NazivVrsteZahtjeva;
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