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
    public class ZahtjevController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<Zahtjev> _logger;
        private readonly AppSettings _appSettings;

        public ZahtjevController(RPPP09Context context, ILogger<Zahtjev> logger, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            var query = _context.Zahtjev.AsNoTracking();

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

            var zahtjevi = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Include(p => p.IdVrsteZahtjevaNavigation)
                        .ToListAsync();

            Console.WriteLine(zahtjevi);

            var model = new ZahtjeviViewModel
            {
                PagingInfo = pagingInfo,
                Zahtjevi = zahtjevi,
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
        public async Task<IActionResult> Create(Zahtjev zahtjev)
        {
            _logger.LogTrace(JsonSerializer.Serialize(zahtjev));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(zahtjev);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Zahtjev {zahtjev.Naziv} dodan.");

                    TempData[Constants.Message] = $"Zahtjev {zahtjev.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanje novog zahtjeva: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(zahtjev);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                await PrepareDropDownList();
                return View(zahtjev);
            }
        }

        private async Task PrepareDropDownList()
        {

            var vrsteZahtjeva = await _context.VrstaZahtjeva.OrderBy(v => v.NazivVrsteZahtjeva).Select(v => new { v.IdVrsteZahtjeva, v.NazivVrsteZahtjeva }).ToListAsync();
            ViewBag.VrsteZahtjeva = new SelectList(vrsteZahtjeva, nameof(VrstaZahtjeva.IdVrsteZahtjeva), nameof(VrstaZahtjeva.NazivVrsteZahtjeva));

        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var zahtjev = await _context.Zahtjev.FindAsync(id);
            if (zahtjev != null)
            {
                try
                {
                    var naziv = zahtjev.Naziv;
                    _context.Remove(zahtjev);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Zahtjev: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Zahtjev {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja zahtjeva: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Zahtjev sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var zahtjev = await _context.Zahtjev.FirstOrDefaultAsync(p => p.IdZahtjev == id);
            if (zahtjev != null)
            {
                await PrepareDropDownList();
                return View(zahtjev);
            }
            else
            {
                return NotFound($"Neispravan id zahtjeva: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Zahtjev zahtjev)
        {
            if (zahtjev == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.Zahtjev.AnyAsync(m => m.IdZahtjev == zahtjev.IdZahtjev);
            if (!checkId)
            {
                return NotFound($"Neispravan id zahtjeva: {zahtjev?.IdZahtjev}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zahtjev);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Zahtjev: {zahtjev.Naziv} uspješno ažuriran");
                    TempData[Constants.Message] = $"Zahtjev {zahtjev.Naziv} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja zahtjeva: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(zahtjev);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(zahtjev);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zahtjev = await _context.Zahtjev
                                  .Include(p => p.IdVrsteZahtjevaNavigation)
                                  .FirstOrDefaultAsync(p => p.IdZahtjev == id);
            if (zahtjev != null)
            {
                return PartialView(zahtjev);
            }
            else
            {
                return NotFound($"Neispravan id zahtjeva: {id}");
            }
        }
    }


}
