﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Posao
{
    public int Id { get; set; }

    public int ProjektId { get; set; }

    public string Naziv { get; set; }

    public DateTime OcekivaniPocetak { get; set; }

    public DateTime OcekivaniZavrsetak { get; set; }

    public int Budzet { get; set; }

    public int VrstaPoslaId { get; set; }

    public virtual Projekt Projekt { get; set; }

    public virtual ICollection<Suradnik> Suradnik { get; set; } = new List<Suradnik>();

    public virtual VrstaPosla VrstaPosla { get; set; }

}