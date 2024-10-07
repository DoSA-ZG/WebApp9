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
    public class ZadatakController : Controller
    {
        private readonly RPPP09Context _context;
        private readonly ILogger<Zadatak> _logger;
        private readonly AppSettings _appSettings;

        public ZadatakController(RPPP09Context context, ILogger<Zadatak> logger, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = _appSettings.PageSize;
            var query = _context.Zadatak.AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };

            if ((page < 1 || page > pagingInfo.TotalPages) && count == 0)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort = 1, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var zadaci = await query
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .Include(d => d.Zahtjev)
                        .ToListAsync();

            Console.WriteLine(zadaci);

            var model = new ZadatakViewModel
            {
                PagingInfo = pagingInfo,
                Zadaci = zadaci,
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
        public async Task<IActionResult> Create(Zadatak zadatak)
        {
            _logger.LogTrace(JsonSerializer.Serialize(zadatak));
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(zadatak);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(new EventId(1000), $"Zadatak {zadatak.Naziv} dodan.");

                    TempData[Constants.Message] = $"Zadatak {zadatak.Naziv} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom dodavanje novog zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(zadatak);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(zadatak);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var zadatak = await _context.Zadatak.FindAsync(id);
            if (zadatak != null)
            {
                try
                {
                    var naziv = zadatak.Naziv;
                    _context.Remove(zadatak);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Zadatak: {naziv} uspješno obrisan");
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Zadatak {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom brisanja zadataka: " + exc.CompleteExceptionMessage());
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja zadataka: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Zadatak sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var zadatak = await _context.Zadatak.FirstOrDefaultAsync(d => d.IdZadatak == id);
            if (zadatak != null)
            {
                return View(zadatak);
            }
            else
            {
                return NotFound($"Neispravan id zadataka: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Zadatak zadatak)
        {
            if (zadatak == null)
            {
                return NotFound("Nema poslanih podataka");
            }
            bool checkId = await _context.Zadatak.AnyAsync(d => d.IdZadatak == zadatak.IdZadatak);
            if (!checkId)
            {
                return NotFound($"Neispravan id zadataka: {zadatak?.IdZadatak}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zadatak);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Zadatak: {zadatak.Naziv} uspješno ažuriran");
                    TempData[Constants.Message] = $"Zadatak {zadatak.Naziv} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    _logger.LogError("Pogreška prilikom ažuriranja zadataka: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(zadatak);
                }
            }
            else
            {
                return View(zadatak);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var zadatak = await _context.Zadatak
                                  .Include(d => d.Zahtjev)
                                  .FirstOrDefaultAsync(d => d.IdZadatak == id);
            if (zadatak != null)
            {
                return PartialView(zadatak);
            }
            else
            {
                return NotFound($"Neispravan id zadataka: {id}");
            }
        }
    }


}
