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
    public class VrstaPoslaController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<VrstaPoslaController> logger;
        private readonly AppSettings appSettings;

        public VrstaPoslaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaPoslaController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.VrstaPosla
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

            var vrstePoslova = await query
                        .Select(s => s)
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new VrstePoslovaViewModel
            {
                VrstePoslova = vrstePoslova,
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
        public async Task<IActionResult> Create(VrstaPosla vrstaPosla)
        {
            logger.LogTrace(JsonSerializer.Serialize(vrstaPosla));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(vrstaPosla);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Vrsta posla {vrstaPosla.Naziv} dodana.");

                    TempData[Constants.Message] = $"Vrsta posla {vrstaPosla.Naziv} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove vrste posla: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaPosla);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaPosla);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaPosla = await ctx.VrstaPosla.FindAsync(id);
            if (vrstaPosla != null)
            {
                var objekti = await ctx.Posao
                            .Where(a => a.VrstaPoslaId == id)
                            .Select(a => a)
                            .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = vrstaPosla.Naziv;
                        ctx.Remove(vrstaPosla);
                        await ctx.SaveChangesAsync();
                        logger.LogInformation($"Vrsta posla: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta posla {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom brisanja vrste posla: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja vrste posla: {exc.CompleteExceptionMessage()}");
                    }
                }

                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta posla sa šifrom {id} je korištena kao foregin key za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta posla sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaPosla = await ctx.VrstaPosla.AsNoTracking().Where(a => a.IdVrstaPosla == id).SingleOrDefaultAsync();
            if (vrstaPosla != null)
            {
                return View(vrstaPosla);
            }
            else
            {
                return NotFound($"Neispravan id vrste posla: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaPosla vrstaPosla)
        {
            if (vrstaPosla == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.VrstaPosla.AnyAsync(m => m.IdVrstaPosla == vrstaPosla.IdVrstaPosla);
            if (!checkId)
            {
                return NotFound($"Neispravan id vrste posla: {vrstaPosla?.IdVrstaPosla}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(vrstaPosla);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Vrsta posla: {vrstaPosla.Naziv} uspješno ažurirana");
                    TempData[Constants.Message] = $"Vrsta posla: {vrstaPosla.Naziv} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja vrste posla: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaPosla);
                }
            }
            else
            {
                return View(vrstaPosla);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaPosla = await ctx.VrstaPosla
                                  .Where(a => a.IdVrstaPosla == id)
                                  .Select(a => a)
                                  .SingleOrDefaultAsync();
            if (vrstaPosla != null)
            {
                return PartialView(vrstaPosla);
            }
            else
            {
                return NotFound($"Neispravan id vrste posla: {id}");
            }
        }

    }
}