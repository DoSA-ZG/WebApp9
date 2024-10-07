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
    public class TransakcijaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<TransakcijaController> logger;
        private readonly AppSettings appSettings;

        public TransakcijaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<TransakcijaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                int pagesize = appSettings.PageSize;

                var query = ctx.Transakcija
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

                var transakcije = await query
                    .Select(s => s)
                    .Include("Racun")
                    .Include("VrstaTransakcije")
                    .Include(p => p.UnutarnjiRacun)
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .ToListAsync();


                var model = new TransakcijeViewModel
                {
                    Transakcije = transakcije,
                    PagingInfo = pagingInfo
                };

                return View(model);
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dohvaćanja transakcija: {0}", exc.CompleteExceptionMessage());
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transakcija transakcija)
        {
            logger.LogTrace(JsonSerializer.Serialize(transakcija));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(transakcija);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Transakcija {transakcija.Idtransakcije} dodana.");

                    TempData[Constants.Message] = $"Transakcija {transakcija.Idtransakcije} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog transakcijaa: {0}",
                        exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(transakcija);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(transakcija);
            }
        }

        private async Task PrepareDropDownList()
        {
            var vrsteTransakcija = await ctx.VrstaTransakcije.OrderBy(k => k.Naziv)
                .Select(k => new { k.IdvrstaTransakcije, k.Naziv }).ToListAsync();
            ViewBag.VrsteTransakcija = new SelectList(vrsteTransakcija, nameof(VrstaTransakcije.IdvrstaTransakcije),
                nameof(VrstaTransakcije.Naziv));

            var racuni = await ctx.Racun.OrderBy(k => k.ImeRacuna).Select(k => new { k.IdRacuna, k.ImeRacuna })
                .ToListAsync();
            ViewBag.Racuni = new SelectList(racuni, nameof(Racun.IdRacuna), nameof(Racun.ImeRacuna));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var transakcija = await ctx.Transakcija.FindAsync(id);
            if (transakcija != null)
            {
                try
                {
                    ctx.Remove(transakcija);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Transakcija {id} uspješno obrisana");
                    responseMessage = new ActionResponseMessage(MessageType.Success,
                        $"Transakcija sa šifrom {id} uspješno obrisana.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Pogreška prilikom brisanja transakcije: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage =
                    new ActionResponseMessage(MessageType.Error, $"Transakcija sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transakcija = await ctx.Transakcija.AsNoTracking().Where(a => a.Idtransakcije == id)
                .SingleOrDefaultAsync();
            if (transakcija != null)
            {
                await PrepareDropDownList();
                return View(transakcija);
            }
            else
            {
                return NotFound($"Neispravan id transakcije: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Transakcija transakcija)
        {
            if (transakcija == null)
            {
                return NotFound("Nema poslanih podataka");
            }

            bool checkId = await ctx.Transakcija.AnyAsync(m => m.Idtransakcije == transakcija.Idtransakcije);
            if (!checkId)
            {
                return NotFound($"Neispravan id transakcije: {transakcija.Idtransakcije}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(transakcija);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Transakcija: {transakcija.Idtransakcije} uspješno ažurirana");
                    TempData[Constants.Message] = $"Transakcija {transakcija.Idtransakcije} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja transakcije: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(transakcija);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(transakcija);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var transakcija = await ctx.Transakcija
                .Include("VrstaTransakcije")
                .Include("Racun")
                .Where(a => a.Idtransakcije == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (transakcija != null)
            {
                return PartialView(transakcija);
            }
            else
            {
                return NotFound($"Neispravan id transakcije: {id}");
            }
        }
    }
}