﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

/// <summary>
/// Djelomična klasa koja predstavlja osobu.
/// </summary>
public partial class Osoba
{
    /// <summary>
    /// Jedinstveni identifikator osobe.
    /// </summary>
    public int IdOsoba { get; set; }

    /// <summary>
    /// Ime osobe.
    /// </summary>
    public string ImeOsobe { get; set; }

    /// <summary>
    /// Prezime osobe.
    /// </summary>
    public string PrezimeOsobe { get; set; }

    /// <summary>
    /// E-mail adresa osobe.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Broj telefona osobe.
    /// </summary>
    public string Telefon { get; set; }

    /// <summary>
    /// IBAN (International Bank Account Number) osobe.
    /// </summary>
    public string Iban { get; set; }

    /// <summary>
    /// Kolekcija uloga na projektima koje su dodijeljene osobi.
    /// </summary>
    public virtual ICollection<UlogaNaProjektu> UlogaNaProjektu { get; set; } = new List<UlogaNaProjektu>();

    /// <summary>
    /// Kolekcija zadataka koji su dodijeljeni osobi.
    /// </summary>
    public virtual ICollection<Zadatak> Zadatak { get; set; } = new List<Zadatak>();

    /// <summary>
    /// Kolekcija osoba zaduženih za zadatak.
    /// </summary>
    public virtual ICollection<ZaduzenaOsoba> ZaduzenaOsoba { get; set; } = new List<ZaduzenaOsoba>();
}