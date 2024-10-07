#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public class RacunTransakcijaReportModel
{
    public Racun Racun { get; set; }

    public TransakcijaReportModel Transakcija { get; set; }
}