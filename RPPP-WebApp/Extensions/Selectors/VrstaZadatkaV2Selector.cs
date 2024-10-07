using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Statička klasa koja pruža metodu za primjenu sortiranja na upitu nad tablicom VrstaZadatka.
    /// </summary>
    public static class VrstaZadatkaSort
    {
        /// <summary>
        /// Primjenjuje sortiranje na upit nad tablicom VrstaZadatka prema odabranom kriteriju.
        /// </summary>
        /// <param name="query">Upit nad tablicom VrstaZadatka na koji se primjenjuje sortiranje.</param>
        /// <param name="sort">Vrsta sortiranja (1 za naziv vrste zadatka).</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Upit nad tablicom VrstaZadatka s primijenjenim sortiranjem prema odabranom kriteriju.</returns>
        public static IQueryable<VrstaZadatka> ApplySort(this IQueryable<VrstaZadatka> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<VrstaZadatka, object>> orderSelector = null;

            switch (sort)
            {
                case 1:
                    orderSelector = d => d.NazivVrsteZdtk;
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