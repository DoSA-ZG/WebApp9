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
    public class VrstaTransakcijeController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<VrstaTransakcijeController> logger;
        private readonly AppSettings appSettings;

        public VrstaTransakcijeController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<VrstaTransakcijeController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrstaTransakcije
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

            var vrsteTransakcije = await query
                .Select(s => s)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new VrsteTransakcijeViewModel()
            {
                VrsteTransakcija = vrsteTransakcije,
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
        public async Task<IActionResult> Create(VrstaTransakcije vrstaTransakcije)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaTransakcije));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaTransakcije);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Vrsta transakcije {vrstaTransakcije.Naziv} dodana.");

                    TempData[Constants.Message] = $"Vrsta transakcije {vrstaTransakcije.Naziv} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrste transakcije: {0}",
                        exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaTransakcije);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaTransakcije);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaTransakcije = await ctx.VrstaTransakcije.FindAsync(id);
            if (vrstaTransakcije != null)
            {
                var objekti = await ctx.Transakcija
                    .Where(a => a.IdvrstaTransakcije == id)
                    .Select(a => a)
                    .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = vrstaTransakcije.Naziv;
                        ctx.Remove(vrstaTransakcije);
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Vrsta transakcije: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success,
                            $"Vrsta transakcije {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom brisanja vrste transakcije: " +
                                        exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error,
                            $"Pogreška prilikom brisanja vrste transakcije: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Vrsta transakcije sa šifrom {id} je korištena kao foreign key za neki objekt.");
                }
            }
            else
            {
                responseMessage =
                    new ActionResponseMessage(MessageType.Error, $"Vrsta transakcije sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaTransakcije = await ctx.VrstaTransakcije.AsNoTracking().Where(a => a.IdvrstaTransakcije == id)
                .SingleOrDefaultAsync();
            if (vrstaTransakcije != null)
            {
                return View(vrstaTransakcije);
            }
            else
            {
                return NotFound($"Neispravan id vrste transakcije: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaTransakcije vrstaTransakcije)
        {
            if (vrstaTransakcije == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId =
                await ctx.VrstaTransakcije.AnyAsync(m => m.IdvrstaTransakcije == vrstaTransakcije.IdvrstaTransakcije);
            if (!checkId)
            {
                return NotFound($"Neispravan id vrste transakcije: {vrstaTransakcije.IdvrstaTransakcije}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaTransakcije);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Vrsta transakcije: {vrstaTransakcije.Naziv} uspješno ažurirana");
                    TempData[Constants.Message] = $"Vrsta transakcije: {vrstaTransakcije.Naziv} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja vrste transakcije: " +
                                    exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaTransakcije);
                }
            }
            else
            {
                return View(vrstaTransakcije);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaTransakcije = await ctx.VrstaTransakcije
                .Where(a => a.IdvrstaTransakcije == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (vrstaTransakcije != null)
            {
                return PartialView(vrstaTransakcije);
            }
            else
            {
                return NotFound($"Neispravan id vrste transakcije: {id}");
            }
        }
    }
}