﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Racun
{
    public int IdRacuna { get; set; }

    public string ImeRacuna { get; set; }

    public string Iban { get; set; }

    public int? IdValuta { get; set; }

    public virtual Valuta IdValutaNavigation { get; set; }

    public virtual ICollection<Projekt> Projekti { get; set; } = new List<Projekt>();

    public virtual ICollection<Transakcija> TransakcijaIdracunaNavigation { get; set; } = new List<Transakcija>();

    public virtual ICollection<Transakcija> TransakcijaUnutarnjiIdRacunaNavigation { get; set; } = new List<Transakcija>();
}