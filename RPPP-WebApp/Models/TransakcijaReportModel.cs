namespace RPPP_WebApp.Models;

public class TransakcijaReportModel
{
    public int Idtransakcije { get; set; }
    public string imeRacuna { get; set; }
    public string primateljPosiljatelj { get; set; }
    public int iznos { get; set; }
    public string vrsta { get; set; }
    public string unutarnjaVanjska { get; set; }

    public static TransakcijaReportModel FromTransakcija(Transakcija transakcija)
    {
        return new TransakcijaReportModel
        {
            Idtransakcije = transakcija.Idtransakcije,
            imeRacuna = transakcija.Racun.ImeRacuna,
            primateljPosiljatelj =
                (transakcija.SmjerTransakcije == "U" && transakcija.UnutarnjiRacun != null)
                    ? transakcija.UnutarnjiRacun.Iban + " (" + transakcija.UnutarnjiRacun.ImeRacuna + ")"
                    : transakcija.VanjskiIban,
            iznos = transakcija.Iznos,
            vrsta = transakcija.VrstaTransakcije.Naziv,
            unutarnjaVanjska = (transakcija.SmjerTransakcije == "U")
                ? "Unutarnja"
                : ((transakcija.SmjerTransakcije != null) ? "Vanjska" : "")
        };
    }
}