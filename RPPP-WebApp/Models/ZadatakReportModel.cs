using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

/// <summary>
/// Model koji predstavlja zadatke u izvještajima.
/// </summary>
public partial class ZadatakReportModel
{
    /// <summary>
    /// Jedinstveni identifikator zadatka.
    /// </summary>
    public int IdZadatak { get; set; }

    /// <summary>
    /// Vrijeme isporuke zadatka u formatu "dd.M.yyyy".
    /// </summary>
    public String VrijemeIsporuke { get; set; }

    /// <summary>
    /// Stupanj dovršenosti zadatka.
    /// </summary>
    public int StupanjDovrsenosti { get; set; }

    /// <summary>
    /// Prioritetnost zadatka.
    /// </summary>
    public string Prioritetnost { get; set; }

    /// <summary>
    /// Naziv zadatka.
    /// </summary>
    public string Naziv { get; set; }

    /// <summary>
    /// Osoba zadužena za izvršavanje zadatka.
    /// </summary>
    public virtual Osoba Osoba { get; set; }

    /// <summary>
    /// Vrsta zadatka.
    /// </summary>
    public virtual VrstaZadatka VrstaZadatka { get; set; }

    /// <summary>
    /// Zahtjev koji je povezan sa zadatkom.
    /// </summary>
    public virtual Zahtjev Zahtjev { get; set; }

    /// <summary>
    /// Stvara novu instancu ZadatakReportModel iz objekta tipa Zadatak.
    /// </summary>
    /// <param name="zadatak">Zadatak koji će se koristiti za inicijalizaciju modela.</param>
    /// <returns>Nova instanca ZadatakReportModel.</returns>
    public static ZadatakReportModel FromZadatak(Zadatak zadatak)
    {
        return new ZadatakReportModel
        {
            IdZadatak = zadatak.IdZadatak,
            VrijemeIsporuke = zadatak.VrijemeIsporuke.ToString("dd.M.yyyy"),
            StupanjDovrsenosti = zadatak.StupanjDovrsenosti,
            Prioritetnost = zadatak.Prioritetnost,
            Naziv = zadatak.Naziv,
            Osoba = zadatak.Osoba,
            VrstaZadatka = zadatak.VrstaZadatka,
            Zahtjev = zadatak.Zahtjev
        };
    }
}
