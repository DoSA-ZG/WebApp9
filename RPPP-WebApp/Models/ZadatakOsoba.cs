#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

/// <summary>
/// Predstavlja vezu između zadatka i osobe zadužene za taj zadatak.
/// </summary>
public class ZadatakOsoba
{
    /// <summary>
    /// Zadatak.
    /// </summary>
    public Zadatak Zadatak { get; set; }

    /// <summary>
    /// Osoba zadužena za zadatak.
    /// </summary>
    public Osoba Osoba { get; set; }
}
