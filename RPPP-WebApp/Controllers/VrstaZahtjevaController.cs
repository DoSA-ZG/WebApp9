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
    public class VrstaZahtjevaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<VrstaZahtjevaController> logger;
        private readonly AppSettings appSettings;

        public VrstaZahtjevaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<VrstaZahtjevaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrstaZahtjeva
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

            var vrsteZahtjeva = await query
                .Select(s => s)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new VrstaZahtjevaViewModel()
            {
                VrsteZahtjeva = vrsteZahtjeva,
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
        public async Task<IActionResult> Create(VrstaZahtjeva vrstaZahtjeva)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaZahtjeva));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaZahtjeva);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Vrsta zahtjeva {vrstaZahtjeva.NazivVrsteZahtjeva} dodana.");

                    TempData[Constants.Message] = $"Vrsta zahtjeva {vrstaZahtjeva.NazivVrsteZahtjeva} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrste zahtjeva: {0}",
                        exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaZahtjeva);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaZahtjeva);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaZahtjeva = await ctx.VrstaZahtjeva.FindAsync(id);
            if (vrstaZahtjeva != null)
            {
                var objekti = await ctx.Zahtjev
                    .Where(a => a.IdVrsteZahtjeva == id)
                    .Select(a => a)
                    .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = vrstaZahtjeva.NazivVrsteZahtjeva;
                        ctx.Remove(vrstaZahtjeva);
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Vrsta zahtjeva: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success,
                            $"Vrsta zahtjeva {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom brisanja vrste zahtjeva: " +
                                        exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error,
                            $"Pogreška prilikom brisanja vrste zahtjeva: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Vrsta zahtjeva sa šifrom {id} je korištena kao foreign key za neki objekt.");
                }
            }
            else
            {
                responseMessage =
                    new ActionResponseMessage(MessageType.Error, $"Vrsta zahtjeva sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaZahtjeva = await ctx.VrstaZahtjeva.AsNoTracking().Where(a => a.IdVrsteZahtjeva == id)
                .SingleOrDefaultAsync();
            if (vrstaZahtjeva != null)
            {
                return View(vrstaZahtjeva);
            }
            else
            {
                return NotFound($"Neispravan id vrste zahtjeva: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaZahtjeva vrstaZahtjeva)
        {
            if (vrstaZahtjeva == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId =
                await ctx.VrstaZahtjeva.AnyAsync(m => m.IdVrsteZahtjeva == vrstaZahtjeva.IdVrsteZahtjeva);
            if (!checkId)
            {
                return NotFound($"Neispravan id vrste zahtjeva: {vrstaZahtjeva.IdVrsteZahtjeva}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaZahtjeva);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Vrsta zahtjeva: {vrstaZahtjeva.NazivVrsteZahtjeva} uspješno ažuriran.");
                    TempData[Constants.Message] = $"Vrsta zahtjeva: {vrstaZahtjeva.NazivVrsteZahtjeva} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja vrste zahtjeva: " +
                                    exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaZahtjeva);
                }
            }
            else
            {
                return View(vrstaZahtjeva);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaZahtjeva = await ctx.VrstaZahtjeva
                .Where(a => a.IdVrsteZahtjeva == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (vrstaZahtjeva != null)
            {
                return PartialView(vrstaZahtjeva);
            }
            else
            {
                return NotFound($"Neispravan id vrste zahtjeva: {id}");
            }
        }
    }
}