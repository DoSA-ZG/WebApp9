using RPPP_WebApp.Models;
using System.Linq;
using System;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class PosaoSelector
    {
        public static IQueryable<Posao> ApplySort(this IQueryable<Posao> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Posao, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Naziv;
                    break;
                case 2:
                    orderSelector = d => d.Projekt.ImeProjekta;
                    break;
                case 3:
                    orderSelector = d => d.OcekivaniPocetak;
                    break;
                case 4:
                    orderSelector = d => d.OcekivaniZavrsetak;
                    break;
                case 5:
                    orderSelector = d => d.Budzet;
                    break;
                case 6:
                    orderSelector = d => d.VrstaPosla.Naziv;
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