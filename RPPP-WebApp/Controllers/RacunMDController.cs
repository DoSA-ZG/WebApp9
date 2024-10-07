using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.Differencing;
using RPPP_WebApp.Extensions;

namespace RPPP_WebApp.Controllers
{
    public class RacunMDController : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<RacunMDController> logger;
        private readonly AppSettings appSettings;

        public RacunMDController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options,
            ILogger<RacunMDController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Racun
                .Include(a => a.IdValutaNavigation)
                .Include(a => a.TransakcijaIdracunaNavigation)
                .ThenInclude(transakcija => transakcija.VrstaTransakcije)
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
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);


            var racuni = await query
                .Select(a => new RacunMDViewModel
                {
                    Racun = a
                })
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync();

            foreach (var racun in racuni)
            {
                int stanjeRacuna = 0;
                if (!racun.Racun.TransakcijaIdracunaNavigation.IsNullOrEmpty())
                {
                    foreach (var transakcija in racun.Racun.TransakcijaIdracunaNavigation)
                    {
                        int iznos = transakcija.VrstaTransakcije.Naziv.Equals("Uplata",
                            StringComparison.OrdinalIgnoreCase)
                            ? transakcija.Iznos
                            : -transakcija.Iznos;
                        stanjeRacuna += iznos;
                    }
                }

                racun.StanjeRacuna = stanjeRacuna;
            }

            var model = new RacuniMDViewModel
            {
                Racuni = racuni,
                PagingInfo = pagingInfo
            };

            return View(model);
        }


        public async Task<IActionResult> Show(int id, string filter, int page = 1, int sort = 1, bool ascending = true,
            string viewName = nameof(Show))
        {
            var racun = await ctx.Racun
                .Include(a => a.IdValutaNavigation)
                .Include(a => a.TransakcijaIdracunaNavigation)
                .ThenInclude(transakcija => transakcija.VrstaTransakcije)
                .Where(a => a.IdRacuna == id)
                .Select(a => new RacunMDViewModel
                {
                    Racun = a
                })
                .FirstOrDefaultAsync();
            if (racun == null)
            {
                return NotFound($"Račun s id {id} ne postoji");
            }
            else
            {
                foreach (var transakcija in racun.Racun.TransakcijaIdracunaNavigation)
                {
                    var vrstaTransakcije = ctx.VrstaTransakcije
                        .FirstOrDefault(d => d.IdvrstaTransakcije == transakcija.IdvrstaTransakcije);
                    transakcija.VrstaTransakcije = vrstaTransakcije;

                    var unutarnjiRacun = ctx.Racun
                        .FirstOrDefault(d => d.IdRacuna == transakcija.UnutarnjiIdRacuna);
                    transakcija.UnutarnjiRacun = unutarnjiRacun;
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, racun);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var racun = await ctx.Racun
                .Include("Transakcija")
                .Include("Valuta")
                .Include("Projekt")
                .Where(a => a.IdRacuna == id)
                .Select(a => a)
                .SingleOrDefaultAsync();
            if (racun != null)
            {
                return PartialView(racun);
            }
            else
            {
                return NotFound($"Neispravan id računa: {id}");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new RacunMDViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RacunMDViewModel masterDetailViewModel)
        {
            foreach (var transakcija in masterDetailViewModel.Transakcija)
                transakcija.Idtransakcije = 0;

            var racun = new Racun();
            CopyMaster(racun, masterDetailViewModel);

            try
            {
                racun.TransakcijaIdracunaNavigation = masterDetailViewModel.Transakcija;
                ctx.Add(racun);
                await ctx.SaveChangesAsync();
                logger.LogInformation(new EventId(1000), $"MD račun {racun.ImeRacuna} dodan.");
                TempData[Constants.Message] = $"MD račun uspješno dodan. Id={racun.IdRacuna}";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom dodavanje novog računa: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                return View(masterDetailViewModel);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var racun = await ctx.Racun
                .Include(p => p.TransakcijaIdracunaNavigation).Where(p => p.IdRacuna == id).SingleOrDefaultAsync();
            ActionResponseMessage responseMessage;
            if (racun != null)
            {
                try
                {
                    string naziv = racun.ImeRacuna;

                    foreach (var transakcija in racun.TransakcijaIdracunaNavigation)
                    {
                        if (transakcija == null)
                            continue;
                        ctx.Remove(transakcija);
                    }

                    ctx.Remove(racun);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Račun: {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Račun: {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                    responseMessage = new ActionResponseMessage(MessageType.Success,
                        $"Račun {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());
                    TempData[Constants.Message] =
                        "Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    responseMessage = new ActionResponseMessage(MessageType.Error,
                        $"Pogreška prilikom brisanja transakcije: {exc.CompleteExceptionMessage()}");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Račun sa šifrom {id} ne postoji");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var model = new RacunMDViewModel();

            var racun = await ctx.Racun.Include(p => p.IdValutaNavigation)
                .Where(p => p.IdRacuna == id).SingleOrDefaultAsync();

            var transakcija = await ctx.Transakcija.Include(p => p.VrstaTransakcije)
                .Include(p => p.UnutarnjiRacun)
                .Where(s => s.Idracuna == id).ToListAsync();
            racun.TransakcijaIdracunaNavigation = transakcija;

            if (racun != null)
            {
                model.Racun = racun;
                model.Transakcija = transakcija;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(model);
            }
            else
            {
                return NotFound($"Neispravan id transakcije: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RacunMDViewModel masterDetailViewModel, int id, int? position,
            string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            ViewBag.Position = position;
            masterDetailViewModel.Racun.IdRacuna = id;

            Console.WriteLine(masterDetailViewModel.Transakcija.Count);

            if (ModelState.IsValid)
            {
                var racun = await ctx.Racun
                    .Where(p => p.IdRacuna == masterDetailViewModel.Racun.IdRacuna)
                    .Include(p => p.TransakcijaIdracunaNavigation)
                    .FirstOrDefaultAsync();
                if (racun == null)
                {
                    logger.LogWarning("Ne postoji račun s oznakom: {0} ", masterDetailViewModel.Racun.IdRacuna);
                    return NotFound("Ne postoji račun s id-om: " + masterDetailViewModel.Racun.IdRacuna);
                }

                foreach (var transakcija in racun.TransakcijaIdracunaNavigation)
                    ctx.Remove(transakcija);

                foreach (var transakcija in masterDetailViewModel.Transakcija)
                {
                    transakcija.Idracuna = id;
                    if (transakcija == null)
                        continue;
                    ctx.Add(transakcija);
                }

                try
                {
                    ctx.Update(racun);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"(MD) Račun s id: {racun.IdRacuna} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new
                    {
                        id = racun.IdRacuna,
                        position,
                        filter,
                        page,
                        sort,
                        ascending
                    });
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(masterDetailViewModel);
                }
            }
            else
            {
                return View(masterDetailViewModel);
            }
        }

        private static void CopyMaster(Racun racun, RacunMDViewModel masterDetailViewModel)
        {
            racun.ImeRacuna = masterDetailViewModel.ImeRacuna;
            racun.IdValuta = masterDetailViewModel.IdValuta;
            racun.Iban = masterDetailViewModel.Iban;
        }
    }
}