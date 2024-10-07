using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class TransakcijaSort
    {
        public static IQueryable<Transakcija> ApplySort(this IQueryable<Transakcija> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Transakcija, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.Iznos;
                    break;
                case 2:
                    orderSelector = d => d.Racun.ImeRacuna;
                    break;
                case 3:
                    orderSelector = d => d.VrstaTransakcije.Naziv;
                    break;
                case 4:
                    orderSelector = d => (d.SmjerTransakcije == "U") ? d.UnutarnjiRacun.ImeRacuna : false;
                    break;
                case 5:
                    orderSelector = d => (d.SmjerTransakcije == "U") ? d.UnutarnjiRacun.Iban : d.VanjskiIban;
                    break;
                case 6:
                    orderSelector = d => d.SmjerTransakcije;
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