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
    public class ValutaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<ValutaController> logger;
        private readonly AppSettings appSettings;

        public ValutaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<ValutaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Valuta
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

            var valute = await query
                .Select(s => s)
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            var model = new ValuteViewModel()
            {
                Valute = valute,
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
        public async Task<IActionResult> Create(Valuta valuta)
        {
            logger.LogTrace(JsonSerializer.Serialize(valuta));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(valuta);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Valuta {valuta.IsoOznaka} dodana.");

                    TempData[Constants.Message] = $"Valuta {valuta.IsoOznaka} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove valute: {0}",
                        exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(valuta);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(valuta);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var valuta = await ctx.Valuta.FindAsync(id);
            if (valuta != null)
            {
                var objekti = await ctx.Racun
                    .Where(a => a.IdValuta == id)
                    .Select(a => a)
                    .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = valuta.IsoOznaka;
                        ctx.Remove(valuta);
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Valuta: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success,
                            $"Valuta {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom brisanja valute: " +
                                        exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error,
                            $"Pogreška prilikom brisanja valute: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Valuta sa šifrom {id} je korištena kao foreign key za neki objekt.");
                }
            }
            else
            {
                responseMessage =
                    new ActionResponseMessage(MessageType.Error, $"Valuta sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var valuta = await ctx.Valuta.AsNoTracking().Where(a => a.Idvaluta == id)
                .SingleOrDefaultAsync();
            if (valuta != null)
            {
                return View(valuta);
            }
            else
            {
                return NotFound($"Neispravan id valute: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Valuta valuta)
        {
            if (valuta == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId =
                await ctx.Valuta.AnyAsync(m => m.Idvaluta == valuta.Idvaluta);
            if (!checkId)
            {
                return NotFound($"Neispravan id valute: {valuta.Idvaluta}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(valuta);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Valuta: {valuta.IsoOznaka} uspješno ažurirana");
                    TempData[Constants.Message] = $"Valuta: {valuta.IsoOznaka} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja valute: " +
                                    exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(valuta);
                }
            }
            else
            {
                return View(valuta);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var valuta = await ctx.Valuta
                .Where(a => a.Idvaluta == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (valuta != null)
            {
                return PartialView(valuta);
            }
            else
            {
                return NotFound($"Neispravan id valute: {id}");
            }
        }
    }
}