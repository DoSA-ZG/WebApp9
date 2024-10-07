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
    public class ProjektController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<Projekt> _logger;
        private readonly AppSettings _appSettings;

        public ProjektController(RPPP09Context context, ILogger<Projekt> logger, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            var query = _context.Projekt.AsNoTracking();

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

            var projekti = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Include(p => p.Racun)
                        .Include(p => p.VrstaProjekta)
                        .ToListAsync();

            Console.WriteLine(projekti);

            var model = new ProjektiViewModel
            {
                PagingInfo = pagingInfo,
                Projekti = projekti,
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
        public async Task<IActionResult> Create(Projekt projekt)
        {
            _logger.LogTrace(JsonSerializer.Serialize(projekt));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(projekt);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Projekt {projekt.ImeProjekta} dodan.");

                    TempData[Constants.Message] = $"Projekt {projekt.ImeProjekta} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanje novog projekta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(projekt);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(projekt);
            }
        }

        private async Task PrepareDropDownList()
        {

            var vrsteProjekata = await _context.VrstaProjekta.OrderBy(v => v.Naziv).Select(v => new { v.IdVrstaProjekta, v.Naziv }).ToListAsync();
            ViewBag.VrsteProjekata = new SelectList(vrsteProjekata, nameof(VrstaProjekta.IdVrstaProjekta), nameof(VrstaProjekta.Naziv));

            var racuni = await _context.Racun.OrderBy(r => r.IdRacuna).Select(r => new { r.IdRacuna, r.Iban }).ToListAsync();
            ViewBag.Racuni = new SelectList(racuni, nameof(Racun.IdRacuna), nameof(Racun.Iban));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var projekt = await _context.Projekt.FindAsync(id);
            if (projekt != null)
            {
                try
                {
                    var naziv = projekt.ImeProjekta;
                    _context.Remove(projekt);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Projekt: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Projekt {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom brisanja projekta: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja projekta: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Projekt sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var projekt = await _context.Projekt.FirstOrDefaultAsync(p => p.IdProjekt == id);
            if (projekt != null)
            {
                await PrepareDropDownList();
                return View(projekt);
            }
            else
            {
                return NotFound($"Neispravan id projekta: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Projekt projekt)
        {
            if (projekt == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.Projekt.AnyAsync(m => m.IdProjekt == projekt.IdProjekt);
            if (!checkId)
            {
                return NotFound($"Neispravan id projekta: {projekt?.IdProjekt}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projekt);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Projekt: {projekt.ImeProjekta} uspješno ažuriran");
                    TempData[Constants.Message] = $"Projekt {projekt.ImeProjekta} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja projekta: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(projekt);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(projekt);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var projekt = await _context.Projekt
                                  .Include(p => p.VrstaProjekta)
                                  .FirstOrDefaultAsync(p => p.IdProjekt == id);
            if (projekt != null)
            {
                return PartialView(projekt);
            }
            else
            {
                return NotFound($"Neispravan id projekta: {id}");
            }
        }
    }


}
