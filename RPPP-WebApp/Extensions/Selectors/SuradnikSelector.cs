using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class SuradnikSort
    {
        public static IQueryable<Suradnik> ApplySort(this IQueryable<Suradnik> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Suradnik, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.Posao.Naziv;
                    break;
                case 3:
                    orderSelector = d => d.Oib;
                    break;
                case 4:
                    orderSelector = d => d.Adresa;
                    break;
                case 5:
                    orderSelector = d => d.PostanskiBroj;
                    break;
                case 6:
                    orderSelector = d => d.Grad;
                    break;
                case 7:
                    orderSelector = d => d.VrstaSuradnika.Naziv;
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