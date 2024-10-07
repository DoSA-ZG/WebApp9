using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class ProjektSelector
    {
        public static IQueryable<Projekt> ApplySort(this IQueryable<Projekt> query,  int sort, bool ascending) 
        {
            System.Linq.Expressions.Expression<Func<Projekt, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = p => p.ImeProjekta;
                    break;
                case 2:
                    orderSelector= p => p.PocetakProjekta;
                    break;
                case 3:
                    orderSelector = p => p.KrajProjekta;
                    break;
                case 4:
                    orderSelector = p => p.Oznaka;
                    break;
                case 5:
                    orderSelector = p => p.VrstaProjekta.Naziv;
                    break;
                case 6:
                    orderSelector = p => p.Racun.Iban;
                    break;
                case 7:
                    orderSelector = p => p.Opis;
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
