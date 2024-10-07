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
    public class PosaoController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<PosaoController> logger;
        private readonly AppSettings appSettings;

        public PosaoController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<PosaoController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Posao
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

            var poslovi = await query
                        .Select(s => s)
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            foreach (var posao in poslovi)
            {
                var vrstaPosla = await ctx.VrstaPosla
                                    .Where(a => a.IdVrstaPosla == posao.VrstaPoslaId)
                                    .Select(a => a)
                                    .ToListAsync();
                posao.VrstaPosla = vrstaPosla[0];

                var projekt = await ctx.Projekt
                                    .Where(a => a.IdProjekt == posao.ProjektId)
                                    .Select(a => a)
                                    .ToListAsync();
                posao.Projekt = projekt[0];
            }

            var model = new PosloviViewModel
            {
                Poslovi = poslovi,
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
        public async Task<IActionResult> Create(Posao posao)
        {
            logger.LogTrace(JsonSerializer.Serialize(posao));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(posao);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Posao {posao.Naziv} dodan.");

                    TempData[Constants.Message] = $"Posao {posao.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog posla: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(posao);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(posao);
            }
        }

        private async Task PrepareDropDownList()
        {

            var vrstePoslova = await ctx.VrstaPosla.OrderBy(k => k.Naziv).Select(k => new { k.IdVrstaPosla, k.Naziv }).ToListAsync();
            ViewBag.VrstePoslova = new SelectList(vrstePoslova, nameof(VrstaPosla.IdVrstaPosla), nameof(VrstaPosla.Naziv));

            var projekti = await ctx.Projekt.OrderBy(k => k.ImeProjekta).Select(k => new { k.IdProjekt, k.ImeProjekta}).ToListAsync();
            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.ImeProjekta));
        }



        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var posao = await ctx.Posao.FindAsync(id);
            if (posao != null)
            {
                try
                {
                    string naziv = posao.Naziv;
                    ctx.Remove(posao);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Posao: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Posao {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja posla: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja posla: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Posao sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var posao = await ctx.Posao.AsNoTracking().Where(a => a.Id == id).SingleOrDefaultAsync();
            if (posao != null)
            {
                await PrepareDropDownList();
                return View(posao);
            }
            else
            {
                return NotFound($"Neispravan id posla: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Posao posao)
        {
            if (posao == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await ctx.Posao.AnyAsync(m => m.Id == posao.Id);
            if (!checkId)
            {
                return NotFound($"Neispravan id posla: {posao?.Id}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(posao);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Posao: {posao.Naziv} uspješno ažuriran");
                    TempData[Constants.Message] = $"Posao {posao.Naziv} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom ažuriranja posla: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(posao);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(posao);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var posao = await ctx.Posao
                                  .Where(a => a.Id == id)
                                  .Select(a => a)
                                  .SingleOrDefaultAsync();
            if (posao != null)
            {
                var vrsta = await ctx.VrstaPosla
                                .Where(a => a.IdVrstaPosla == posao.VrstaPoslaId)
                                .Select(a => a)
                                .SingleOrDefaultAsync();

                posao.VrstaPosla = vrsta;
                return PartialView(posao);
            }
            else
            {
                return NotFound($"Neispravan id posla: {id}");
            }
        }

    }
}