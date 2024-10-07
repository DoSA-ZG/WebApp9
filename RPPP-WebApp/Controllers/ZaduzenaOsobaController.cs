using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;

namespace RPPP_WebApp.Controllers
{
    public class ZaduzenaOsobaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<ZaduzenaOsobaController> logger;
        private readonly AppSettings appSettings;

        public ZaduzenaOsobaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<ZaduzenaOsobaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.ZaduzenaOsoba
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

            var osobe = await query
                .Select(s => s)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new ZaduzenaOsobaViewModel()
            {
                ZaduženeOsobe = osobe,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZaduzenaOsoba osoba)
        {
            logger.LogTrace(JsonSerializer.Serialize(osoba));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(osoba);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Osoba {osoba.IdOsoba} dodana.");

                    TempData[Constants.Message] = $"Osoba {osoba.IdOsoba} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove osobe: {0}",
                        exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(osoba);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(osoba);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var osoba = await ctx.ZaduzenaOsoba.FindAsync(id);
            if (osoba != null)
            {
                try
                {
                    ctx.Remove(osoba);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Osoba: {id} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Osoba {id} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja osobe: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja osobe: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Osoba sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var osoba = await ctx.ZaduzenaOsoba.AsNoTracking().Where(a => a.IdOsoba == id)
                .SingleOrDefaultAsync();
            if (osoba != null)
            {
                return View(osoba);
            }
            else
            {
                return NotFound($"Neispravan id osobe: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ZaduzenaOsoba osoba)
        {
            if (osoba == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId =
                await ctx.ZaduzenaOsoba.AnyAsync(m => m.IdOsoba == osoba.IdOsoba);
            if (!checkId)
            {
                return NotFound($"Neispravan id osobe: {osoba.IdOsoba}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(osoba);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Osoba: {osoba.IdOsoba} uspješno ažurirana");
                    TempData[Constants.Message] = $"Osoba: {osoba.IdOsoba} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja osobe: " +
                                    exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(osoba);
                }
            }
            else
            {
                return View(osoba);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var osoba = await ctx.ZaduzenaOsoba
                .Where(a => a.IdOsoba == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (osoba != null)
            {
                return PartialView(osoba);
            }
            else
            {
                return NotFound($"Neispravan id valute: {id}");
            }
        }
    }
}