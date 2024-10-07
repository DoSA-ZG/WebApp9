using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using MVC.ViewModels;
using Microsoft.Data.SqlClient.DataClassification;

namespace RPPP_WebApp.Controllers
{
    public class AutoCompleteController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<ZadatakMDV2Controller> zdtkLogger;
        private readonly ILogger<PosaoMDController> logger;
        private readonly AppSettings appData;

        public AutoCompleteController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakMDV2Controller> logger)
        {
            appData = options.Value;
            this.ctx = ctx;
            zdtkLogger = logger;
        }

        public async Task<IEnumerable<IdLabel>> Projekt(string term)
        {
            var query = ctx.Projekt
                     .Where(d => d.ImeProjekta.Contains(term))
                     .OrderBy(d => d.ImeProjekta)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdProjekt,
                         Label = d.ImeProjekta
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> VrstaPosla(string term)
        {
            var query = ctx.VrstaPosla
                     .Where(d => d.Naziv.Contains(term))
                     .OrderBy(d => d.Naziv)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdVrstaPosla,
                         Label = d.Naziv
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> VrstaSuradnika(string term)
        {
            var query = ctx.VrstaSuradnika
                     .Where(d => d.Naziv.Contains(term))
                     .OrderBy(d => d.Naziv)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdVrstaSuradnika,
                         Label = d.Naziv
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> VrstaProjekta(string term)
        {
            var query = ctx.VrstaProjekta
                     .Where(d => d.Naziv.ToLower().Contains(term.ToLower()))
                     .OrderBy(d => d.Naziv)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdVrstaProjekta,
                         Label = d.Naziv
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> Racun(string term)
        {
            var query = ctx.Racun
                     .Where(d => d.Iban.Replace(" ", "").ToLower().Contains(term.Replace(" ", "").ToLower()))
                     .OrderBy(d => d.Iban)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdRacuna,
                         Label = d.Iban
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }
        public async Task<IEnumerable<IdLabel>> KategorijaDokumenta(string term)
        {
            var query = ctx.KategorijaDokumenta
                     .Where(d => d.NazivKategorijeDokumenta.ToLower().Contains(term.ToLower()))
                     .OrderBy(d => d.NazivKategorijeDokumenta)
                     .Select(d => new IdLabel
                     {
                         Id = d.KategorijaDokumentaId,
                         Label = d.NazivKategorijeDokumenta
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }
        public async Task<IEnumerable<IdLabel>> Posao(string term)
        {
            var query = ctx.Posao
                     .Where(d => d.Naziv.ToLower().Contains(term.ToLower()))
                     .OrderBy(d => d.Naziv)
                     .Select(d => new IdLabel
                     {
                         Id = d.Id,
                         Label = d.Naziv
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Metoda koja vraća kolekciju objekata tipa IdLabel za osobu prema zadanom pojmu.
        /// </summary>
        /// <param name="term">Pojam za pretragu.</param>
        /// <returns>Kolekciju objekata tipa IdLabel.</returns>
        public async Task<IEnumerable<IdLabel>> Osoba(string term)
        {
            var query = ctx.Osoba
                     .Where(d => (d.ImeOsobe + " " + d.PrezimeOsobe).ToLower().Contains(term.ToLower()))
                     .OrderBy(d => d.PrezimeOsobe)
                     .ThenBy(d => d.ImeOsobe)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdOsoba,
                         Label = d.ImeOsobe + " " + d.PrezimeOsobe
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Metoda koja vraća kolekciju objekata tipa IdLabel za ulogu prema zadanom pojmu.
        /// </summary>
        /// <param name="term">Pojam za pretragu.</param>
        /// <returns>Kolekciju objekata tipa IdLabel.</returns>
        public async Task<IEnumerable<IdLabel>> Uloga(string term)
        {
            var query = ctx.Uloga
                     .Where(d => d.NazivUloge.ToLower().Contains(term.ToLower()))
                     .OrderBy(d => d.NazivUloge)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdUloga,
                         Label = d.NazivUloge
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Metoda koja vraća kolekciju objekata tipa IdLabel za vrstu zadatka prema zadanom pojmu.
        /// </summary>
        /// <param name="term">Pojam za pretragu.</param>
        /// <returns>Kolekciju objekata tipa IdLabel.</returns>
        public async Task<IEnumerable<IdLabel>> VrstaZadatka(string term)
        {
            var query = ctx.VrstaZadatka
                     .Where(d => d.NazivVrsteZdtk.Contains(term))
                     .OrderBy(d => d.NazivVrsteZdtk)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdVrstaZdtk,
                         Label = d.NazivVrsteZdtk
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        /// <summary>
        /// Metoda koja vraća kolekciju objekata tipa IdLabel za zahtjev prema zadanom pojmu.
        /// </summary>
        /// <param name="term">Pojam za pretragu.</param>
        /// <returns>Kolekciju objekata tipa IdLabel.</returns>
        public async Task<IEnumerable<IdLabel>> Zahtjev(string term)
        {
            var query = ctx.Zahtjev
                     .Where(d => d.Naziv.Contains(term))
                     .OrderBy(d => d.Naziv)
                     .Select(d => new IdLabel
                     {
                         Id = d.IdZahtjev,
                         Label = d.Naziv
                     });

            var list = await query.OrderBy(l => l.Label)
                                  .ThenBy(l => l.Id)
                                  .Take(appData.AutoCompleteCount)
                                  .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> VrstaTransakcije(string term)
        {
            var query = ctx.VrstaTransakcije
                .Where(d => d.Naziv.Contains(term))
                .OrderBy(d => d.Naziv)
                .Select(d => new IdLabel
                {
                    Id = d.IdvrstaTransakcije,
                    Label = d.Naziv
                });

            var list = await query.OrderBy(l => l.Label)
                .ThenBy(l => l.Id)
                .Take(appData.AutoCompleteCount)
                .ToListAsync();
            return list;
        }

        public async Task<IEnumerable<IdLabel>> Valuta(string term)
        {
            var query = ctx.Valuta
                .Where(d => d.IsoOznaka.Contains(term))
                .OrderBy(d => d.IsoOznaka)
                .Select(d => new IdLabel
                {
                    Id = d.Idvaluta,
                    Label = d.IsoOznaka
                });

            var list = await query.OrderBy(l => l.Label)
                .ThenBy(l => l.Id)
                .Take(appData.AutoCompleteCount)
                .ToListAsync();
            return list;
        }
    }
}
