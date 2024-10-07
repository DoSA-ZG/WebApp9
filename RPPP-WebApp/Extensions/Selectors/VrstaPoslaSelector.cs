using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaPoslaSort
    {
        public static IQueryable<VrstaPosla> ApplySort(this IQueryable<VrstaPosla> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaPosla, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
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