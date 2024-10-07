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
    public class VrstaProjektaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<VrstaProjektaController> _logger;
        private readonly AppSettings _appSettings;

        public VrstaProjektaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaProjektaController> logger)
        {
            _context = ctx;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;

            var query = _context.VrstaProjekta
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

            var vrsteProjekata = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new VrsteProjekataViewModel
            {
                VrsteProjekata = vrsteProjekata,
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
        public async Task<IActionResult> Create(VrstaProjekta vrstaProjekta)
        {
            _logger.LogTrace(JsonSerializer.Serialize(vrstaProjekta));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(vrstaProjekta);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Vrsta projekta {vrstaProjekta.Naziv} dodana.");

                    TempData[Constants.Message] = $"Vrsta projekta {vrstaProjekta.Naziv} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanja nove vrste projekta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaProjekta);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(vrstaProjekta);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var vrstaProjekta = await _context.VrstaProjekta.FindAsync(id);
            if (vrstaProjekta != null)
            {
                var objekti = await _context.Projekt
                            .Where(p => p.IdVrstaProjekta == id)
                            .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = vrstaProjekta.Naziv;
                        _context.Remove(vrstaProjekta);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Vrsta projekta: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Vrsta projekta {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError("Pogreška prilikom brisanja vrste projekta: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja vrste projekta: {exc.CompleteExceptionMessage()}");
                    }
                }

                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta projekta sa šifrom {id} je korištena kao foregin key za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Vrsta projekta sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vrstaProjekta = await _context.VrstaProjekta.AsNoTracking().FirstOrDefaultAsync(a => a.IdVrstaProjekta == id);
            if (vrstaProjekta != null)
            {
                return View(vrstaProjekta);
            }
            else
            {
                return NotFound($"Neispravan id vrste projekta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VrstaProjekta vrstaProjekta)
        {
            if (vrstaProjekta == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.VrstaProjekta.AnyAsync(v => v.IdVrstaProjekta == vrstaProjekta.IdVrstaProjekta);
            if (!checkId)
            {
                return NotFound($"Neispravan id vrste projekta: {vrstaProjekta?.IdVrstaProjekta}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vrstaProjekta);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Vrsta projekta: {vrstaProjekta.Naziv} uspješno ažurirana");
                    TempData[Constants.Message] = $"Vrsta projekta: {vrstaProjekta.Naziv} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja vrste projekta: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(vrstaProjekta);
                }
            }
            else
            {
                return View(vrstaProjekta);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var vrstaProjekta = await _context.VrstaProjekta
                                  .FirstOrDefaultAsync(v => v.IdVrstaProjekta == id);
            if (vrstaProjekta != null)
            {
                return PartialView(vrstaProjekta);
            }
            else
            {
                return NotFound($"Neispravan id vrste projekta: {id}");
            }
        }

    }
}