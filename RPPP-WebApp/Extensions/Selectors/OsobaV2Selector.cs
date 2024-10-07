using RPPP_WebApp.Models;

namespace RPPP_WebApp.Extensions.Selectors
{
    /// <summary>
    /// Statička klasa koja pruža metodu za primjenu sortiranja nad upitima na tablici Osoba.
    /// </summary>
    public static class OsobaSort
    {
        /// <summary>
        /// Primjenjuje sortiranje na upit nad tablicom Osoba prema odabranom kriteriju.
        /// </summary>
        /// <param name="query">Upit nad tablicom Osoba na koji se primjenjuje sortiranje.</param>
        /// <param name="sort">Vrsta sortiranja (1 za ime, 2 za prezime, 3 za email, 4 za telefon, 5 za IBAN).</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Upit nad tablicom Osoba s primijenjenim sortiranjem prema odabranom kriteriju.</returns>
        public static IQueryable<Osoba> ApplySort(this IQueryable<Osoba> query, int sort, bool ascending)
        {
            System.Linq.Expressions.Expression<Func<Osoba, object>> orderSelector = null;
            switch (sort)
            {
                case 1:
                    orderSelector = d => d.ImeOsobe;
                    break;
                case 2:
                    orderSelector= d => d.PrezimeOsobe;
                    break;
                case 3:
                    orderSelector = d => d.Email;
                    break;
                case 4:
                    orderSelector = d => d.Telefon;
                    break;
                case 5:
                    orderSelector = d => d.Iban;
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