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
    public class KategorijaDokumentaController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<KategorijaDokumentaController> _logger;
        private readonly AppSettings _appSettings;

        public KategorijaDokumentaController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<KategorijaDokumentaController> logger)
        {
            _context = ctx;
            _logger = logger;
            _appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;

            var query = _context.KategorijaDokumenta
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

            var kategorijeDokumenata = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new KategorijeDokumenataViewModel
            {
                KategorijeDokumenata = kategorijeDokumenata,
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
        public async Task<IActionResult> Create(KategorijaDokumenta kategorijaDokumenta)
        {
            _logger.LogTrace(JsonSerializer.Serialize(kategorijaDokumenta));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(kategorijaDokumenta);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Kategorija dokumenta {kategorijaDokumenta.NazivKategorijeDokumenta} dodana.");

                    TempData[Constants.Message] = $"Kategorija dokumenta {kategorijaDokumenta.NazivKategorijeDokumenta} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanja nove kategorije dokumenta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(kategorijaDokumenta);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(kategorijaDokumenta);
            }
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var kategorijaDokumenta = await _context.KategorijaDokumenta.FindAsync(id);
            if (kategorijaDokumenta != null)
            {
                var objekti = await _context.Dokument
                            .Where(d => d.KategorijaDokumentaId == id)
                            .ToListAsync();

                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = kategorijaDokumenta.NazivKategorijeDokumenta;
                        _context.Remove(kategorijaDokumenta);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Kategorija dokumenta: {naziv} uspješno obrisana");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Kategorija dokumenta {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError("Pogreška prilikom brisanja kategorije dokumenta: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja kategorije dokumenta: {exc.CompleteExceptionMessage()}");
                    }
                }

                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Kategorija dokumenta sa šifrom {id} je korištena kao foregin key za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Kategorija dokumenta sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kategorijaDokumenta = await _context.KategorijaDokumenta.AsNoTracking().FirstOrDefaultAsync(k => k.KategorijaDokumentaId == id);
            if (kategorijaDokumenta != null)
            {
                return View(kategorijaDokumenta);
            }
            else
            {
                return NotFound($"Neispravan id kategorije dokumenta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(KategorijaDokumenta kategorijaDokumenta)
        {
            if (kategorijaDokumenta == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.KategorijaDokumenta.AnyAsync(k => k.KategorijaDokumentaId == kategorijaDokumenta.KategorijaDokumentaId);
            if (!checkId)
            {
                return NotFound($"Neispravan id kategorije dokumenta: {kategorijaDokumenta?.KategorijaDokumentaId}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategorijaDokumenta);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Kategorija dokumenta: {kategorijaDokumenta.NazivKategorijeDokumenta} uspješno ažurirana");
                    TempData[Constants.Message] = $"Kategorija dokumenta: {kategorijaDokumenta.NazivKategorijeDokumenta} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja kategorije dokumenta: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(kategorijaDokumenta);
                }
            }
            else
            {
                return View(kategorijaDokumenta);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var kategorijaDokumenta = await _context.KategorijaDokumenta
                                  .FirstOrDefaultAsync(k => k.KategorijaDokumentaId == id);
            if (kategorijaDokumenta != null)
            {
                return PartialView(kategorijaDokumenta);
            }
            else
            {
                return NotFound($"Neispravan id kategorije dokumenta: {id}");
            }
        }

    }
}