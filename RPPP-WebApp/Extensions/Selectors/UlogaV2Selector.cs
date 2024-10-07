using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Statička klasa koja pruža metodu za primjenu sortiranja na upitu nad tablicom Uloga.
    /// </summary>
    public static class UlogaSort
    {
        /// <summary>
        /// Primjenjuje sortiranje na upit nad tablicom Uloga prema odabranom kriteriju.
        /// </summary>
        /// <param name="query">Upit nad tablicom Uloga na koji se primjenjuje sortiranje.</param>
        /// <param name="sort">Vrsta sortiranja (1 za naziv uloge).</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Upit nad tablicom Uloga s primijenjenim sortiranjem prema odabranom kriteriju.</returns>
        public static IQueryable<Uloga> ApplySort(this IQueryable<Uloga> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Uloga, object>> orderSelector = null;

            switch(sort)
            {
                case 1:
                    orderSelector = d => d.NazivUloge;
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