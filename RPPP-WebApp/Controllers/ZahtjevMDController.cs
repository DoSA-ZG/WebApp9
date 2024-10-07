using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using NLog.LayoutRenderers;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace RPPP_WebApp.Controllers {
    public class ZahtjevMDController : Controller {
        private readonly RPPP09Context _context;
        private readonly ILogger<ZahtjevMDController> _logger;
        private readonly AppSettings appSettings;

        public ZahtjevMDController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevMDController> logger) {
            _context = ctx;
            _logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
            int pagesize = appSettings.PageSize;

            var query = _context.Zahtjev
                            .Include(p => p.IdVrsteZahtjevaNavigation)
                           .AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages) {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);


            var zahtjevi = await query
                         .Select(p => new ZahtjevMDViewModel
                         {
                             Zahtjev = p,

                         })
                         .Skip((page - 1) * pagesize)
                         .Take(pagesize)
                         .ToListAsync();

            foreach (var zahtjev in zahtjevi) {
                string zadaci = "";
                if (!zahtjev.Zahtjev.Zadatak.IsNullOrEmpty())
                {
                    foreach (var zadatak in zahtjev.Zahtjev.Zadatak)
                    {
                        zadaci += zadatak.Naziv + ", ";
                    }
                    zadaci = zadaci[..^2];
                }
                zahtjev.NaziviZadataka = zadaci;

            }

            var model = new ZahtjeviMDViewModel {
                Zahtjevi = zahtjevi,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        public async Task<IActionResult> ShowZadaci(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowZadaci)) {
            var zahtjev = await _context.Zahtjev
                                 .Include(p => p.Zadatak)
                                    .ThenInclude(zadatak => zadatak.ZaduzenaOsoba)
                                    .Where(p => p.IdZahtjev == id)
                                      .Select(p => new ZahtjevMDViewModel
                                      {
                                          Zahtjev = p,
                                      })
                                    .FirstOrDefaultAsync();
            if (zahtjev == null) {
                return NotFound($"Zahtjev s id {id} ne postoji");
            } else {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, zahtjev);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id) {
            var zahtjev = await _context.Zahtjev
                                  .Include(p => p.IdVrsteZahtjevaNavigation)
                                  .Where(p => p.IdZahtjev == id)
                                  .SingleOrDefaultAsync();
            if (zahtjev != null) {
                return PartialView(zahtjev);
            } else {
                return NotFound($"Neispravan id zahtjeva: {id}");
            }
        }
    }
}
