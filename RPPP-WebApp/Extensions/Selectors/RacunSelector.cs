using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class RacunSelector
    {
        public static IQueryable<Racun> ApplySort(this IQueryable<Racun> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Racun, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.ImeRacuna;
                    break;
                case 2:
                    orderSelector = d => d.Iban;
                    break;
                case 3:
                    orderSelector = d => d.IdValutaNavigation.IsoOznaka;
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