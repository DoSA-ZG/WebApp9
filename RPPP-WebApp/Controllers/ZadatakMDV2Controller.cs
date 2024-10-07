using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using NLog.LayoutRenderers;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.Differencing;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje MD zadacima.
    /// </summary>
    public class ZadatakMDV2Controller : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<ZadatakMDV2Controller> logger;
        private readonly AppSettings appSettings;
        private ZadaciMDV2ViewModel indexViewModel;

        /// <summary>
        /// Konstruktor za ZadatakMDV2Controller.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        /// <param name="logger">Logger za bilježenje događaja.</param>
        public ZadatakMDV2Controller(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakMDV2Controller> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Prikazuje popis zadataka sa paginacijom i sortiranjem.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>View s popisom zadataka.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Zadatak
                        .Include("VrstaZadatka")
                        .Include("Zahtjev")
                        .Include("ZaduzenaOsoba")
                        .AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new {page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var zadaci = await query.Select(z => new ZadatakMDV2ViewModel
            {
                Zadatak = z
            })
            .Skip((page - 1) * pagesize)
            .Take(pagesize)
            .ToListAsync();

            foreach (var zadatak in zadaci)
            {
                string nazivi = "";

                if (!zadatak.Zadatak.ZaduzenaOsoba.IsNullOrEmpty())
                {
                    foreach (var zaduzenik in zadatak.Zadatak.ZaduzenaOsoba)
                    {
                        var osoba = await ctx.Osoba
                                    .Where(o => o.IdOsoba == zaduzenik.IdOsoba)
                                    .Select(o => o)
                                    .SingleOrDefaultAsync();
                        nazivi += osoba.ImeOsobe + " " + osoba.PrezimeOsobe + ", ";
                    }

                    nazivi = nazivi.Substring(0, nazivi.Length - 2);
                }

                zadatak.NaziviZadOsoba = nazivi;
            }

            indexViewModel = new ZadaciMDV2ViewModel
            {
                Zadaci = zadaci,
                PagingInfo = pagingInfo
            };

            return View(indexViewModel);
        }

        /// <summary>
        /// Prikazuje detalje određenog zadatka.
        /// </summary>
        /// <param name="id">ID zadatka.</param>
        /// <param name="filter">Filter za pretraživanje.</param>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <param name="viewName">View za prikaz.</param>
        /// <returns>View s detaljima zadatka.</returns>
        public async Task<IActionResult> Show(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show))
        {
            var zadatak = await ctx.Zadatak
                .Include("VrstaZadatka")
                .Include("Zahtjev")
                .Include("ZaduzenaOsoba")
                .Where(z => z.IdZadatak == id)
                .Select(z => new ZadatakMDV2ViewModel
                {
                    Zadatak = z
                })
                .FirstOrDefaultAsync();

            if (zadatak == null)
            {
                return NotFound($"Zadatak sa šifrom {id} ne postoji.");
            }
            else
            {
                foreach(var zaduzenik in zadatak.Zadatak.ZaduzenaOsoba)
                {
                    var osoba = ctx.Osoba
                        .Include("UlogaNaProjektu")
                        .Where(o => o.IdOsoba ==  zaduzenik.IdOsoba)
                        .FirstOrDefault();

                    zaduzenik.Osoba = osoba;

                    var uloge = ctx.UlogaNaProjektu
                        .Include("Uloga")
                        .Where(unp => unp.IdOsoba == osoba.IdOsoba)
                        .ToList();

                    zaduzenik.Osoba.UlogaNaProjektu = uloge;
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, zadatak);
            }
        }

        /// <summary>
        /// Priprema padajuću listu vrsta zadataka i zahtjeva.
        /// </summary>
        private async Task PrepareDropDownList()
        {
            var vrsteZadataka = await ctx.VrstaZadatka
                                .OrderBy(v => v.NazivVrsteZdtk)
                                .Select(v => new { v.IdVrstaZdtk, v.NazivVrsteZdtk })
                                .ToListAsync();
            ViewBag.VrsteZadataka = new SelectList(vrsteZadataka, nameof(VrstaZadatka.IdVrstaZdtk), nameof(VrstaZadatka.NazivVrsteZdtk));

            var zahtjevi = await ctx.Zahtjev
                            .OrderBy(z => z.Naziv)
                            .Select(z => new { z.IdZahtjev, z.Naziv })
                            .ToListAsync();
            ViewBag.Zahtjevi = new SelectList(zahtjevi, nameof(Zahtjev.IdZahtjev), nameof(Zahtjev.Naziv));
        }

        /// <summary>
        /// Dohvaća detalje zadatka.
        /// </summary>
        /// <param name="id">ID zadatka koji se dohvaća.</param>
        /// <returns>ActionResult koji predstavlja detalje zadatka.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zadatak = await ctx.Zadatak
                        .Include("VrstaZadatka")
                        .Include("Zahtjev")
                        .Include("ZaduzenaOsoba")
                        .Where(z => z.IdZadatak == id)
                        .Select(z => z)
                        .SingleOrDefaultAsync();

            if (zadatak != null)
            {
                return PartialView(zadatak);
            }
            else
            {
                return NotFound($"Neispravna šifra zadatka: {id}");
            }
        }

        /// <summary>
        /// Prikazuje formu za stvaranje novog zadatka.
        /// </summary>
        /// <returns>View rezultat za stvaranje novog zadatka.</returns>
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new ZadatakMDV2ViewModel();
            return View(viewModel);
        }

        /// <summary>
        /// Stvara novi zadatak.
        /// </summary>
        /// <param name="MDViewModel">ViewModel koji sadrži podatke o novom zadatku.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije stvaranja novog zadatka.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(ZadatakMDV2ViewModel MDViewModel)
        {
            if (true)
            {
                foreach (var zaduzenik in MDViewModel.ZaduzenaOsoba)
                {
                    zaduzenik.IdZaduzenja = 0;
                }
                var zadatak = new Zadatak();
                CopyMaster(zadatak, MDViewModel);

                try
                {
                    zadatak.ZaduzenaOsoba = MDViewModel.ZaduzenaOsoba;

                    ctx.Add(zadatak);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"MD zadatak {zadatak.Naziv} dodan.");
                    TempData[Constants.Message] = $"MD zadatak uspješno dodan. Id={zadatak.IdZadatak}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(MDViewModel);
                }
            }
            else
            {
                return View(MDViewModel);
            }
        }

        /// <summary>
        /// Briše zadatak s određenim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator zadatka koji se briše.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije brisanja zadatka.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var zadatak = await ctx.Zadatak
                .Include(z => z.ZaduzenaOsoba)
                .Where(z => z.IdZadatak == id)
                .SingleOrDefaultAsync();

            ActionResponseMessage responseMessage;

            if (zadatak != null)
            {
                try
                {
                    string naziv = zadatak.Naziv;

                    foreach (var zaduzenik in zadatak.ZaduzenaOsoba)
                    {
                        if (zaduzenik != null)
                        {
                            ctx.Remove(zaduzenik);
                        }
                    }

                    ctx.Remove(zadatak);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Zadatak: {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Zadatak: {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Zadatak {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage());
                    TempData[Constants.Message] = "Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja zadatka: {exc.CompleteExceptionMessage()}");

                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Zadatak sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        /// <summary>
        /// Prikazuje formu za uređivanje zadatka.
        /// </summary>
        /// <param name="id">Identifikator zadatka koji se uređuje.</param>
        /// <param name="page">Trenutna stranica.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije prikaza forme za uređivanje zadatka.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var MDViewModel = new ZadatakMDV2ViewModel();

            var zadatak = await ctx.Zadatak
                .Include(z => z.VrstaZadatka)
                .Include(z => z.Zahtjev)
                .Where(z => z.IdZadatak == id)
                .SingleOrDefaultAsync();

            var zaduzenici = await ctx.ZaduzenaOsoba
                .Include(o => o.Osoba)
                .Where(z => z.IdZadatak == id)
                .ToListAsync();

            zadatak.ZaduzenaOsoba = zaduzenici;

            if (zadatak != null)
            {
                MDViewModel.Zadatak = zadatak;
                MDViewModel.ZaduzenaOsoba = zaduzenici;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(MDViewModel);
            }
            else
            {
                return NotFound($"Neispravan id zadatka: {id}");
            }
        }

        /// <summary>
        /// Ažurira zadani zadatak.
        /// </summary>
        /// <param name="MDViewModel">ViewModel koji sadrži podatke za ažuriranje zadatka.</param>
        /// <param name="id">Identifikator zadatka koji se ažurira.</param>
        /// <param name="position">Pozicija.</param>
        /// <param name="filter">Filter.</param>
        /// <param name="page">Trenutna stranica.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije ažuriranja zadatka.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(ZadatakMDV2ViewModel MDViewModel, int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            ViewBag.Position = position;
            MDViewModel.Zadatak.IdZadatak = id;

            if (ModelState.IsValid)
            {
                var zadatak = await ctx.Zadatak
                    .Where(z => z.IdZadatak == MDViewModel.Zadatak.IdZadatak)
                    .Include(z => z.ZaduzenaOsoba)
                    .FirstOrDefaultAsync();

                if (zadatak == null)
                {
                    logger.LogWarning("Ne postoji zadatak s oznakom: {0} ", MDViewModel.Zadatak.IdZadatak);
                    return NotFound("Ne postoji zadatak s id-om: " + MDViewModel.Zadatak.IdZadatak);
                }

                foreach (var zaduzenik in zadatak.ZaduzenaOsoba)
                {
                    ctx.Remove(zaduzenik);
                }

                foreach (var zaduzenik in MDViewModel.ZaduzenaOsoba)
                {
                    zaduzenik.IdZadatak = id;
                    if (zaduzenik == null)
                        continue;
                    ctx.Add(zaduzenik);
                }

                try
                {
                    ctx.Update(zadatak);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"(MD) Zadatak s id: {zadatak.IdZadatak} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new
                    {
                        id = zadatak.IdZadatak,
                        position,
                        filter,
                        page,
                        sort,
                        ascending
                    });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(MDViewModel);
                }
            }
            else
            {
                return View(MDViewModel);
            }
        }

        /// <summary>
        /// Kopira svojstva master objekta u zadani zadatak.
        /// </summary>
        /// <param name="zadatak">Zadatak u koji se kopiraju podaci.</param>
        /// <param name="MDViewModel">ViewModel koji sadrži podatke o master objektu.</param>
        private static void CopyMaster(Zadatak zadatak, ZadatakMDV2ViewModel MDViewModel)
        {
            zadatak.VrijemeIsporuke = MDViewModel.VrijemeIsporuke;
            zadatak.StupanjDovrsenosti = MDViewModel.StupanjDovrsenosti;
            zadatak.Prioritetnost = MDViewModel.Prioritetnost;
            zadatak.IdOsoba = MDViewModel.IdOsoba;
            zadatak.IdVrstaZdtk = MDViewModel.IdVrstaZdtk;
            zadatak.Naziv = MDViewModel.Naziv;
            zadatak.IdZahtjev = MDViewModel.IdZahtjev;
        }
    }
}