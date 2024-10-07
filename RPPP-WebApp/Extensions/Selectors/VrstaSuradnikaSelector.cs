using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class VrstaSuradnikaSort
    {
        public static IQueryable<VrstaSuradnika> ApplySort(this IQueryable<VrstaSuradnika> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaSuradnika, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.Opis;
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