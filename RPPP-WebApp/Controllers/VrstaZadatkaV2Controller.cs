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
using System.Security.AccessControl;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje vrstama zadataka.
    /// </summary>
    public class VrstaZadatkaV2Controller : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<VrstaZadatkaV2Controller> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Konstruktor kontrolera VrstaZadatkaV2Controller.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        /// <param name="logger">Logger za bilježenje događaja.</param>
        public VrstaZadatkaV2Controller(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaZadatkaV2Controller> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Prikazuje stranicu sa popisom vrsta zadataka.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Red sortiranja.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrstaZadatka.AsNoTracking();

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
                return RedirectToAction(nameof(Index), new {page = 1, sort, ascending});
            }

            query = query.ApplySort(sort, ascending);

            var vrsteZadataka = await query
                                .Select(v => v)
                                .Skip((page - 1) * pagesize)
                                .Take(pagesize)
                                .ToListAsync();

            var model = new VrsteZadatakaV2ViewModel
            {
                VrsteZadataka = vrsteZadataka,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za stvaranje nove vrste zadatka.
        /// </summary>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        /// <summary>
        /// Dodavanje nove vrste zadatka u bazu podataka.
        /// </summary>
        /// <param name="vrstaZadatka">Nova vrsta zadatka.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VrstaZadatka vrstaZadatka)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaZadatka));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaZadatka);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Vrsta zadatka {vrstaZadatka.NazivVrsteZdtk} dodana.");
                    TempData[Constants.Message] = $"Vrsta zadatka {vrstaZadatka.NazivVrsteZdtk} dodana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri dodavanju nove vrste zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(vrstaZadatka);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaZadatka);
            }
        }

        /// <summary>
        /// Briše vrstu zadatka specificiraju ID-jem.
        /// </summary>
        /// <param name="id">ID vrste zadatka koja se briše.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaZadatka = await ctx.VrstaZadatka.FindAsync(id);

            if (vrstaZadatka != null )
            {
                var zadaci = await ctx.Zadatak
                            .Where(z => z.IdVrstaZdtk == id)
                            .Select(z => z)
                            .ToListAsync();

                if (zadaci.Count == 0 )
                {
                    try
                    {
                        string naziv = vrstaZadatka.NazivVrsteZdtk;
                        ctx.Remove(vrstaZadatka);
                        await ctx.SaveChangesAsync();

                        logger.LogInformation($"Vrsta zadatka {naziv} uspješno obrisana.");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta zadatka {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch ( Exception exc )
                    {
                        logger.LogError("Pogreška pri brisanju vrste zadatka: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška pri brisanju vrste zadatka: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta zadatka sa šifrom {id} koristi se kao strani ključ za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta zadatka sa šifrom {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        /// <summary>
        /// Prikazuje formu za uređivanje vrste zadatka.
        /// </summary>
        /// <param name="id">ID vrste zadatka koji se uređuje.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaZadatka = await ctx.VrstaZadatka
                                .AsNoTracking()
                                .Where(v => v.IdVrstaZdtk == id)
                                .SingleOrDefaultAsync();

            if (vrstaZadatka != null )
            {
                return View(vrstaZadatka);
            }
            else
            {
                return NotFound($"Nesipravna šifra vrste zadatka: {id}");
            }
        }

        /// <summary>
        /// Ažuriranje vrste zadatka u bazi podataka.
        /// </summary>
        /// <param name="vrstaZadatka">Vrsta zadatka za ažuriranje.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(VrstaZadatka vrstaZadatka)
        {
            if(vrstaZadatka == null)
            {
                return NotFound("Nema poslanih podataka.");
            }

            bool checkId = await ctx.VrstaZadatka.AnyAsync(v => v.IdVrstaZdtk == vrstaZadatka.IdVrstaZdtk);

            if (!checkId)
            {
                return NotFound($"Neispravna šifra vrste zadatka: {vrstaZadatka?.IdVrstaZdtk}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaZadatka);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Vrsta zadatka {vrstaZadatka.NazivVrsteZdtk} uspješno ažurirana.");
                    TempData[Constants.Message] = $"Vrsta zadatka {vrstaZadatka.NazivVrsteZdtk} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri ažuriranju vrste zadatka: " +  exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(vrstaZadatka);
                }
            }
            else
            {
                return View(vrstaZadatka);
            }
        }

        /// <summary>
        /// Dohvaća vrstu zadatka prema ID-u.
        /// </summary>
        /// <param name="id">ID vrste zadatka koji se dohvaća.</param>
        /// <returns>Asinkroni task koji predstavlja HTTP odgovor.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaZadatka = await ctx.VrstaZadatka
                                .Where(v => v.IdVrstaZdtk == id)
                                .Select(v => v)
                                .SingleOrDefaultAsync();

            if (vrstaZadatka != null)
            {
                return PartialView(vrstaZadatka);
            }
            else
            {
                return NotFound($"Neispravna šifra vrste zadatka: {id}");
            }
        }
    }
}