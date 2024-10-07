using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NLog.LayoutRenderers;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using System.Security.Policy;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje zadacima.
    /// </summary>
    public class ZadatakV2Controller : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<ZadatakV2Controller> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Konstruktor kontrolera ZadatakV2Controller.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        /// <param name="logger">Logger za zapisivanje događaja.</param>
        public ZadatakV2Controller(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakV2Controller> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Prikazuje popis zadataka.
        /// </summary>
        /// <param name="page">Trenutna stranica.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije prikaza popisa zadataka.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Zadatak.AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if ((page < 1 || page > pagingInfo.TotalPages) && count != 0)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var zadaci = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Include(z => z.VrstaZadatka)
                        .Include(z => z.Zahtjev)
                        .Include(z => z.Osoba)
                        .ToListAsync();

            var model = new ZadaciV2ViewModel
            {
                Zadaci = zadaci,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za stvaranje novog zadatka.
        /// </summary>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije prikaza forme za stvaranje zadatka.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownList();
            return View();
        }

        /// <summary>
        /// Stvara novi zadatak u bazi podataka.
        /// </summary>
        /// <param name="zadatak">Novi zadatak koji se stvara.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije stvaranja zadatka.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Zadatak zadatak)
        {
            logger.LogTrace(JsonSerializer.Serialize(zadatak));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(zadatak);
                    await ctx.SaveChangesAsync();

                    zadatak.ZaduzenaOsoba.Add(new ZaduzenaOsoba
                    {
                        IdOsoba = zadatak.IdOsoba,
                        IdZadatak = zadatak.IdZadatak
                    });

                    ctx.Update(zadatak);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Zadatak {zadatak.Naziv} dodan.");
                    TempData[Constants.Message] = $"Zadatak {zadatak.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri dodavanju novog zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    await PrepareDropDownList();
                    return View(zadatak);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                await PrepareDropDownList();
                return View(zadatak);
            }
        }

        /// <summary>
        /// Priprema padajuće liste za vrste zadataka, zahtjeve i osobe.
        /// </summary>
        /// <returns>Task koji predstavlja operaciju pripreme padajućih popisa.</returns>
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

            var fullName = "";
            var osobe = await ctx.Osoba
                        .OrderBy(o => o.ImeOsobe)
                        .ThenBy(o => o.PrezimeOsobe)
                        .Select(o => new { o.IdOsoba, fullName = $"{o.ImeOsobe} {o.PrezimeOsobe}" })
                        .ToListAsync();
            ViewBag.Osobe = new SelectList(osobe, nameof(Osoba.IdOsoba), nameof(fullName));
        }

        /// <summary>
        /// Briše zadatak s određenim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator zadatka koji se briše.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije brisanja zadatka.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var zadatak = await ctx.Zadatak.FindAsync(id);

            if (zadatak != null)
            {
                try
                {
                    var zaduzeneOsobe = await ctx.ZaduzenaOsoba
                                    .Where(z => z.IdZadatak == id)
                                    .Select(z => z)
                                    .ToListAsync();

                    ctx.RemoveRange(zaduzeneOsobe);
                    await ctx.SaveChangesAsync();

                    string naziv = zadatak.Naziv;
                    ctx.Remove(zadatak);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Zadatak {naziv} uspješno obrisan.");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Zadatak {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri brisanju zadatka: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška pri brisanju zadatka: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Zadatak sa šifrom {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        /// <summary>
        /// Prikazuje formu za uređivanje zadatka.
        /// </summary>
        /// <param name="id">Identifikator zadatka koji se uređuje.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije prikaza forme za uređivanje zadatka.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var zadatak = await ctx.Zadatak
                            .AsNoTracking()
                            .Where(z => z.IdZadatak == id)
                            .SingleOrDefaultAsync();

            if (zadatak != null)
            {
                await PrepareDropDownList();
                return View(zadatak);
            }
            else
            {
                return NotFound($"Neispravna šifra zadatka: {id}");
            }
        }

        /// <summary>
        /// Ažurira zadani zadatak.
        /// </summary>
        /// <param name="zadatak">Zadatak koji se ažurira.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije ažuriranja zadatka.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Zadatak zadatak)
        {
            if (zadatak == null)
            {
                return NotFound("Nema poslanih podataka.");
            }

            bool checkId = await ctx.Zadatak.AnyAsync(z => z.IdZadatak == zadatak.IdZadatak);

            if (!checkId)
            {
                return NotFound($"Neispravna šifra zadatka: {zadatak?.IdZadatak}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    zadatak.ZaduzenaOsoba.First().IdOsoba = zadatak.IdOsoba;
                    zadatak.ZaduzenaOsoba.First().IdZadatak = zadatak.IdZadatak;

                    ctx.Update(zadatak);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Zadatak {zadatak.Naziv} uspješno ažuriran.");
                    TempData[Constants.Message] = $"Zadatak {zadatak.Naziv} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri ažuriranju zadatka: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    await PrepareDropDownList();
                    return View(zadatak);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(zadatak);
            }
        }

        /// <summary>
        /// Dohvaća zadani zadatak.
        /// </summary>
        /// <param name="id">Identifikator zadatka koji se dohvaća.</param>
        /// <returns>Task<IActionResult> koji predstavlja rezultat operacije dohvaćanja zadatka.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zadatak = await ctx.Zadatak
                            .Include("VrstaZadatka")
                            .Include("Zahtjev")
                            .Include("Osoba")
                            .Where(z => z.IdZadatak == id)
                            .Select(z => z)
                            .SingleOrDefaultAsync();

            if (zadatak != null)
            {
                return PartialView(zadatak);
            }
            else
            {
                return NotFound($"Nesipravna šifra zadatka: {id}");
            }
        }
    }
}
