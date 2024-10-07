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
    public class VrstaSuradnikaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<VrstaSuradnikaController> logger;
        private readonly AppSettings appSettings;

        public VrstaSuradnikaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaSuradnikaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrstaSuradnika
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

            var vrsteSuradnika = await query
                        .Select(s => s)
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new VrsteSuradnikaViewModel
            {
                VrsteSuradnika = vrsteSuradnika,
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
        public async Task<IActionResult> Create(VrstaSuradnika vrstaSuradnika)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaSuradnika));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaSuradnika);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Vrsta suradnika {vrstaSuradnika.Naziv} dodana.");

                    TempData[Constants.Message] = $"Vrsta suradnika {vrstaSuradnika.Naziv} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrste suradnika: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaSuradnika);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaSuradnika);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
            if (vrstaSuradnika != null)
            {
                var objekti = await ctx.Suradnik
                            .Where(a => a.VrstaSuradnikaId == id)
                            .Select(a => a)
                            .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = vrstaSuradnika.Naziv;
                        ctx.Remove(vrstaSuradnika);
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Vrsta suradnika: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta suradnika {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom brisanja vrste suradnika: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja vrste suradnika: {exc.CompleteExceptionMessage()}");
                    }
                }

                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta suradnika sa šifrom {id} je korištena kao foregin key za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta suradnika sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaSuradnika = await ctx.VrstaSuradnika.AsNoTracking().Where(a => a.IdVrstaSuradnika == id).SingleOrDefaultAsync();
            if (vrstaSuradnika != null)
            {
                return View(vrstaSuradnika);
            }
            else
            {
                return NotFound($"Neispravan id vrste suradnika: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaSuradnika vrstaSuradnika)
        {
            if (vrstaSuradnika == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.VrstaSuradnika.AnyAsync(m => m.IdVrstaSuradnika == vrstaSuradnika.IdVrstaSuradnika);
            if (!checkId)
            {
                return NotFound($"Neispravan id vrste suradnika: {vrstaSuradnika?.IdVrstaSuradnika}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaSuradnika);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Vrsta suradnika: {vrstaSuradnika.Naziv} uspješno ažurirana");
                    TempData[Constants.Message] = $"Vrsta suradnika: {vrstaSuradnika.Naziv} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja vrste suradnika: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaSuradnika);
                }
            }
            else
            {
                return View(vrstaSuradnika);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaSuradnika = await ctx.VrstaSuradnika
                                  .Where(a => a.IdVrstaSuradnika == id)
                                  .Select(a => a)
                                  .SingleOrDefaultAsync();
            if (vrstaSuradnika != null)
            {
                return PartialView(vrstaSuradnika);
            }
            else
            {
                return NotFound($"Neispravan id vrste suradnika: {id}");
            }
        }

    }
}