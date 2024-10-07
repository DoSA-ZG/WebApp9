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
using Microsoft.CodeAnalysis.Differencing;

namespace RPPP_WebApp.Controllers {
    public class PosaoMDController : Controller {
        private readonly RPPP09Context ctx;
        private readonly ILogger<PosaoMDController> logger;
        private readonly AppSettings appSettings;
        private PosloviMDViewModel indexViewModel;

        public PosaoMDController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<PosaoMDController> logger) {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
            int pagesize = appSettings.PageSize;

            var query = ctx.Posao
                            .Include("Projekt")
                            .Include("VrstaPosla")
                            .Include("Suradnik")
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


            var poslovi = await query
                         .Select(a => new PosaoMDViewModel
                         {
                             Posao = a
                         })
                         .Skip((page - 1) * pagesize)
                         .Take(pagesize)
                         .ToListAsync();

            foreach (var posao in poslovi) {
                string naziv = "";
                if (!posao.Posao.Suradnik.IsNullOrEmpty())
                {
                    foreach (var suradnik in posao.Posao.Suradnik)
                    {
                        naziv += suradnik.Naziv + ", ";
                    }
                    naziv = naziv.Substring(0, naziv.Length - 2);
                }
                posao.NaziviSuradnika = naziv;
            }

            indexViewModel = new PosloviMDViewModel {
                Poslovi = poslovi,
                PagingInfo = pagingInfo
            };

            return View(indexViewModel);
        }


        public async Task<IActionResult> Show(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(Show)) {
            var posao = await ctx.Posao
                                 .Include("VrstaPosla")
                                 .Include("Suradnik")
                                 .Include("Projekt")
                                    .Where(a => a.Id == id)
                                      .Select(a => new PosaoMDViewModel
                                      {
                                          Posao = a
                                      })
                                    .FirstOrDefaultAsync();
            if (posao == null) {
                return NotFound($"Posao s id {id} ne postoji");
            } else {

                foreach(var suradnik in posao.Posao.Suradnik)
                {
                    var vrstaSuradnika = ctx.VrstaSuradnika.Where(d => d.IdVrstaSuradnika == suradnik.VrstaSuradnikaId).FirstOrDefault();
                    suradnik.VrstaSuradnika = vrstaSuradnika;
                }

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, posao);
            }
        }


        private async Task PrepareDropDownList()
		{

			var vrstePoslova = await ctx.VrstaPosla.OrderBy(k => k.Naziv).Select(k => new { k.IdVrstaPosla, k.Naziv }).ToListAsync();
			ViewBag.VrstePoslova = new SelectList(vrstePoslova, nameof(VrstaPosla.IdVrstaPosla), nameof(VrstaPosla.Naziv));

			var projekti = await ctx.Projekt.OrderBy(k => k.ImeProjekta).Select(k => new { k.IdProjekt, k.ImeProjekta }).ToListAsync();
			ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.ImeProjekta));
		}

        [HttpGet]
        public async Task<IActionResult> Get(int id) {
            var posao = await ctx.Posao
                                  .Include("Suradnik")
                                  .Include("VrstaPosla")
                                  .Include("Projekt")
                                  .Where(a => a.Id == id)
                                  .Select(a => a)
                                  .SingleOrDefaultAsync();
            if (posao != null) {
                return PartialView(posao);
            } else {
                return NotFound($"Neispravan id posla: {id}");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new PosaoMDViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PosaoMDViewModel masterDetailViewModel)
        {
            if (true)
            {
                foreach(var suradnik in masterDetailViewModel.Suradnik)
                    suradnik.Id = 0;

                var posao = new Posao();
                CopyMaster(posao, masterDetailViewModel);

                try
                {
                    posao.Suradnik = masterDetailViewModel.Suradnik;
                    ctx.Add(posao);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation(new EventId(1000), $"MD posao {posao.Naziv} dodan.");
                    TempData[Constants.Message] = $"MD posao uspješno dodan. Id={posao.Id}";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog posla: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(masterDetailViewModel);
                }
            }
            else
            {
                return View(masterDetailViewModel);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var posao = await ctx.Posao
                   .Include(p => p.Suradnik).Where(p => p.Id == id).SingleOrDefaultAsync();
            ActionResponseMessage responseMessage;
            if (posao != null)
            {
                try
                {
                    string naziv = posao.Naziv;

                    foreach (var suradnik in posao.Suradnik)
                    {
                        if (suradnik == null)
                            continue;
                        ctx.Remove(suradnik);
                    }

                    ctx.Remove(posao);
                    await ctx.SaveChangesAsync();
                    logger.LogInformation($"Posao: {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Posao: {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Posao {naziv} sa šifrom {id} uspješno obrisan.");
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom brisanja posla: " + exc.CompleteExceptionMessage());
                    TempData[Constants.Message] = "Pogreška prilikom brisanja posla: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
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
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var model = new PosaoMDViewModel();

            var posao = await ctx.Posao.Include(p => p.Projekt).Include(p => p.VrstaPosla)
                .Where(p => p.Id == id).SingleOrDefaultAsync();

            var suradnici = await ctx.Suradnik.Include(p => p.VrstaSuradnika)
                .Where(s => s.PosaoId == id).ToListAsync();
            posao.Suradnik = suradnici;

            if (posao != null)
            {
                model.Posao = posao;
                model.Suradnik = suradnici;
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(model);
            }
            else
            {
                return NotFound($"Neispravan id posla: {id}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PosaoMDViewModel masterDetailViewModel, int id, int? position, string filter, int page = 1, int sort = 1, bool ascending = true)
        {
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            ViewBag.Filter = filter;
            ViewBag.Position = position;
            masterDetailViewModel.Posao.Id = id;

            Console.WriteLine(masterDetailViewModel.Suradnik.Count);

            if (ModelState.IsValid)
            {
                var posao = await ctx.Posao
                                        .Where(p => p.Id == masterDetailViewModel.Posao.Id)
                                        .Include(p => p.Suradnik)
                                        .FirstOrDefaultAsync();
                if (posao == null)
                {
                    logger.LogWarning("Ne postoji posao s oznakom: {0} ", masterDetailViewModel.Posao.Id);
                    return NotFound("Ne postoji posao s id-om: " + masterDetailViewModel.Posao.Id);
                }

                foreach (var suradnik in posao.Suradnik)
                    ctx.Remove(suradnik);

                foreach (var suradnik in masterDetailViewModel.Suradnik)
                {
                    suradnik.PosaoId = id;
                    if (suradnik == null)
                        continue;
                    ctx.Add(suradnik);
                }

                try
                {
                    ctx.Update(posao);
                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"(MD) Posao s id: {posao.Id} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index), new
                    {
                        id = posao.Id,
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

        private static void CopyMaster(Posao posao, PosaoMDViewModel masterDetailViewModel)
        {
            posao.Naziv = masterDetailViewModel.Naziv;
            posao.ProjektId = masterDetailViewModel.ProjektId;
            posao.OcekivaniPocetak = masterDetailViewModel.OcekivaniPocetak;
            posao.OcekivaniZavrsetak = masterDetailViewModel.OcekivaniZavrsetak;
            posao.Budzet = masterDetailViewModel.Budzet;
            posao.VrstaPoslaId = masterDetailViewModel.VrstaPoslaId;
        }
    }
}
