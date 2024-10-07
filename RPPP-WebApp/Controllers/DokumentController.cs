using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers
{
    public class DokumentController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<Dokument> _logger;
        private readonly AppSettings _appSettings;

        public DokumentController(RPPP09Context context, ILogger<Dokument> logger, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            var query = _context.Dokument.AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if((page < 1 || page > pagingInfo.TotalPages) && count == 0){
                return RedirectToAction(nameof(Index), new {page = 1, sort = 1, ascending});
            }

            query = query.ApplySort(sort, ascending);

            var dokumenti = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Include(d => d.KategorijaDokumenta)
                        .Include(p => p.Projekt)
                        .ToListAsync();

            Console.WriteLine(dokumenti);

            var model = new DokumentiViewModel
            {
                PagingInfo = pagingInfo,
                Dokumenti = dokumenti,
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
        public async Task<IActionResult> Create(Dokument dokument)
        {
            _logger.LogTrace(JsonSerializer.Serialize(dokument));
             if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(dokument);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Dokument {dokument.Naslov} dodan.");

                    TempData[Constants.Message] = $"Dokument {dokument.Naslov} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanje novog dokumenta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(dokument);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(dokument);
            }
        }

        private async Task PrepareDropDownList()
        {

            var kategorijeDokumenata = await _context.KategorijaDokumenta.OrderBy(k => k.NazivKategorijeDokumenta).Select(k => new { k.KategorijaDokumentaId, k.NazivKategorijeDokumenta }).ToListAsync();
            ViewBag.KategorijeDokumenata = new SelectList(kategorijeDokumenata, nameof(KategorijaDokumenta.KategorijaDokumentaId), nameof(KategorijaDokumenta.NazivKategorijeDokumenta));
        
            var projekti = await _context.Projekt.OrderBy(p => p.ImeProjekta).Select(p => new {p.IdProjekt, p.ImeProjekta}).ToListAsync();
            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.ImeProjekta));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var dokument = await _context.Dokument.FindAsync(id);
            if (dokument != null)
            {
                try
                {
                    var naslov = dokument.Naslov;
                    _context.Remove(dokument);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Dokument: {naslov} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Dokument {naslov} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja dokumenta: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Dokument sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dokument = await _context.Dokument.FirstOrDefaultAsync(d => d.DokumentId == id);
            if (dokument != null)
            {
                await PrepareDropDownList();
                return View(dokument);
            }
            else
            {
                return NotFound($"Neispravan id dokumenta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Dokument dokument)
        {
            if (dokument == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.Dokument.AnyAsync(d => d.DokumentId == dokument.DokumentId);
            if (!checkId)
            {
                return NotFound($"Neispravan id dokumenta: {dokument?.DokumentId}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dokument);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Dokument: {dokument.Naslov} uspješno ažuriran");
                    TempData[Constants.Message] = $"Dokument {dokument.Naslov} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja dokumenta: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(dokument);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(dokument);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var dokument = await _context.Dokument
                                  .Include(d => d.KategorijaDokumenta)
                                  .Include(p => p.Projekt)
                                  .FirstOrDefaultAsync(d => d.DokumentId == id);
            if (dokument != null)
            {
                return PartialView(dokument);
            }
            else
            {
                return NotFound($"Neispravan id dokumenta: {id}");
            }
        }
    }


}
