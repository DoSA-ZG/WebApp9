using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;

namespace RPPP_WebApp.Controllers
{
    public class RacunController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<RacunController> logger;
        private readonly AppSettings appSettings;

        public RacunController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<RacunController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Racun
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
            if ((page < 1 || page > pagingInfo.TotalPages) && count != 0)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var racuni = await query
                .Select(s => s)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            foreach (var racun in racuni)
            {
                var valuta = await ctx.Valuta
                    .Where(a => a.Idvaluta == racun.IdValuta)
                    .Select(a => a)
                    .ToListAsync();
                racun.IdValutaNavigation = valuta[0];
            }

            var model = new RacuniViewModel
            {
                Racuni = racuni,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Racun racun)
        {
            logger.LogTrace(JsonSerializer.Serialize(racun));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(racun);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Račun {racun.ImeRacuna} dodan.");

                    TempData[Constants.Message] = $"Račun {racun.ImeRacuna} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog računa: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(racun);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(racun);
            }
        }

        private async Task PrepareDropDownList()
        {
            var valute = await ctx.Valuta.OrderBy(k => k.IsoOznaka).Select(k => new { k.Idvaluta, k.IsoOznaka })
                .ToListAsync();
            ViewBag.Valute = new SelectList(valute, nameof(Valuta.Idvaluta), nameof(Valuta.IsoOznaka));
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var racun = await ctx.Racun.FindAsync(id);
            if (racun != null)
            {
                try
                {
                    string naziv = racun.ImeRacuna;
                    ctx.Remove(racun);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Račun: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success,
                        $"Račun {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja računa: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Pogreška prilikom brisanja računa: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Račun sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var racun = await ctx.Racun.AsNoTracking().Where(a => a.IdRacuna == id).SingleOrDefaultAsync();
            if (racun != null)
            {
                await PrepareDropDownList();
                return View(racun);
            }
            else
            {
                return NotFound($"Neispravan id računa: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Racun racun)
        {
            if (racun == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId = await ctx.Racun.AnyAsync(m => m.IdRacuna == racun.IdRacuna);
            if (!checkId)
            {
                return NotFound($"Neispravan id računa: {racun?.IdRacuna}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(racun);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Račun: {racun.ImeRacuna} uspješno ažuriran");
                    TempData[Constants.Message] = $"Račun {racun.ImeRacuna} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja računa: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(racun);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(racun);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var racun = await ctx.Racun
                .Where(a => a.IdRacuna == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (racun != null)
            {
                var vrsta = await ctx.Valuta
                    .Where(a => a.Idvaluta == racun.IdValuta)
                    .Select(a => a)
                    .SingleOrDefaultAsync();

                racun.IdValutaNavigation = vrsta;
                return PartialView(racun);
            }
            else
            {
                return NotFound($"Neispravan id računa: {id}");
            }
        }
    }
}