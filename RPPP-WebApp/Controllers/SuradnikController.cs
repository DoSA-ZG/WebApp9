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
    public class SuradnikController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<SuradnikController> logger;
        private readonly AppSettings appSettings;

        public SuradnikController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<SuradnikController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            try {
            int pagesize = appSettings.PageSize;

            var query = ctx.Suradnik
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

            var suradnici = await query
                        .Select(s => s)
                        .Include("Posao")
                        .Include("VrstaSuradnika")
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new SuradniciViewModel
            {
                Suradnici = suradnici,
                PagingInfo = pagingInfo
            };

            return View(model);
            }
            catch(Exception exc){
                logger.LogError("Pogreška prilikom dodavanje novog suradnika: {0}", exc.CompleteExceptionMessage());
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
        public async Task<IActionResult> Create(Suradnik suradnik)
        {
            logger.LogTrace(JsonSerializer.Serialize(suradnik));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(suradnik);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Suradnik {suradnik.Naziv} dodan.");

                    TempData[Constants.Message] = $"Suradnik {suradnik.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog suradnika: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(suradnik);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(suradnik);
            }
        }

        private async Task PrepareDropDownList()
        {
            var vrsteSuradnika = await ctx.VrstaSuradnika.OrderBy(k => k.Naziv).Select(k => new { k.IdVrstaSuradnika, k.Naziv }).ToListAsync();
            ViewBag.VrsteSuradnika = new SelectList(vrsteSuradnika, nameof(VrstaSuradnika.IdVrstaSuradnika), nameof(VrstaSuradnika.Naziv));

            var poslovi = await ctx.Posao.OrderBy(k => k.Naziv).Select(k => new { k.Id, k.Naziv }).ToListAsync();
            ViewBag.Poslovi = new SelectList(poslovi, nameof(Posao.Id), nameof(Posao.Naziv));
        }



        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var suradnik = await ctx.Suradnik.FindAsync(id);
            if (suradnik != null)
            {
                try
                {
                    string naziv = suradnik.Naziv;
                    ctx.Remove(suradnik);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Suradnik: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Suradnik {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja suradnika: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja suradnika: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Suradnik sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var suradnik = await ctx.Suradnik.AsNoTracking().Where(a => a.Id == id).SingleOrDefaultAsync();
            if (suradnik != null)
            {
                await PrepareDropDownList();
                return View(suradnik);
            }
            else
            {
                return NotFound($"Neispravan id suradnika: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Suradnik suradnik)
        {
            if (suradnik == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.Suradnik.AnyAsync(m => m.Id == suradnik.Id);
            if (!checkId)
            {
                return NotFound($"Neispravan id suradnika: {suradnik?.Id}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(suradnik);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Suradnik: {suradnik.Naziv} uspješno ažuriran");
                    TempData[Constants.Message] = $"Suradnik {suradnik.Naziv} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja suradnika: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(suradnik);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(suradnik);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var suradnik = await ctx.Suradnik
                                 .Include("VrstaSuradnika")
                                 .Include("Projekt")
                                  .Where(a => a.Id == id)
                                  .Select(a => a)
                                  .SingleOrDefaultAsync();
            if (suradnik != null)
            {
                return PartialView(suradnik);
            }
            else
            {
                return NotFound($"Neispravan id suradnika: {id}");
            }
        }

    }
}
