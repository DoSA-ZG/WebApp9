﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Valuta
{
    public int Idvaluta { get; set; }

    public string IsoOznaka { get; set; }

    public virtual ICollection<Racun> Racun { get; set; } = new List<Racun>();
}