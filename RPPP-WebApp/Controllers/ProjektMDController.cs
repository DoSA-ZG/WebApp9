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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace RPPP_WebApp.Controllers {
    public class ProjektMDController : Controller {
        private readonly RPPP09Context _context;
        private readonly ILogger<PosaoMDController> _logger;
        private readonly AppSettings appSettings;

        public ProjektMDController(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<PosaoMDController> logger) {
            _context = ctx;
            _logger = logger;
            appSettings = options.Value;
        }

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true) {
            int pagesize = appSettings.PageSize;

            var query = _context.Projekt
                            .Include(p => p.VrstaProjekta)
                            .Include(p => p.Racun)
                            .Include(p => p.Posao)
                            .Include(p => p.Dokument)
                            .Include(p => p.UlogaNaProjektu)
                                .ThenInclude(u => u.Osoba)
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


            var projekti = await query
                         .Select(p => new ProjektMDViewModel
                         {
                             Projekt = p,

                         })
                         .Skip((page - 1) * pagesize)
                         .Take(pagesize)
                         .ToListAsync();

            foreach (var projekt in projekti) {
                string poslovi = "";
                if (!projekt.Projekt.Posao.IsNullOrEmpty())
                {
                    foreach (var posao in projekt.Projekt.Posao)
                    {
                        poslovi += posao.Naziv + ", ";
                    }
                    poslovi = poslovi[..^2];
                }
                projekt.NaziviPoslova = poslovi;

                string imena = "";
                if (!projekt.Projekt.UlogaNaProjektu.IsNullOrEmpty())
                {
                    foreach (var uloga in projekt.Projekt.UlogaNaProjektu)
                    {
                        var osoba = await _context.Osoba.FirstOrDefaultAsync(o => o.IdOsoba ==  uloga.IdOsoba);
                        if (osoba != null) 
                        {
                            imena += $"{osoba.ImeOsobe} {osoba.PrezimeOsobe}, ";
                        }
                    }
                    imena = imena[..^2];
                }
                projekt.ImenaRadnika = imena;

                string dokumenti = "";
                if (!projekt.Projekt.Dokument.IsNullOrEmpty())
                {
                    foreach (var dokument in projekt.Projekt.Dokument)
                    {
                        var naslov = await _context.Dokument.FirstOrDefaultAsync(d => d.DokumentId == dokument.DokumentId);
                        if (naslov != null)
                        {
                            dokumenti+= $"{dokument.Naslov}, ";
                        }
                    }
                    dokumenti = dokumenti[..^2];
                }
                projekt.Dokumenti = dokumenti;
            }

            var model = new ProjektiMDViewModel {
                Projekti = projekti,
                PagingInfo = pagingInfo
            };

            return View(model);
        }


        public async Task<IActionResult> ShowDokumenti(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowDokumenti))
        {
            var projekt = await _context.Projekt
                                 .Include(p => p.Dokument)
                                    .ThenInclude(d => d.KategorijaDokumenta)
                                    .Where(p => p.IdProjekt == id)
                                      .Select(p => new ProjektMDViewModel
                                      {
                                          Projekt = p,
                                      })
                                    .FirstOrDefaultAsync();
            if (projekt == null)
            {
                return NotFound($"Projekt s id {id} ne postoji");
            }
            else
            {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, projekt);
            }
        }

        public async Task<IActionResult> ShowPoslovi(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowPoslovi)) {
            var projekt = await _context.Projekt
                                 .Include(p => p.Posao)
                                    .ThenInclude(posao => posao.VrstaPosla)
                                    .Where(p => p.IdProjekt == id)
                                      .Select(p => new ProjektMDViewModel
                                      {
                                          Projekt = p,
                                      })
                                    .FirstOrDefaultAsync();
            if (projekt == null) {
                return NotFound($"Projekt s id {id} ne postoji");
            } else {

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, projekt);
            }
        }

        public async Task<IActionResult> ShowRadnici(int id, string filter, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowRadnici))
        {
            var projekt = await _context.Projekt
                                 .Include(p => p.UlogaNaProjektu)
                                    .ThenInclude(u => u.Osoba)
                                    .Where(p => p.IdProjekt == id)
                                      .Select(p => new ProjektMDViewModel
                                      {
                                          Projekt = p,
                                          RadniciList = p.UlogaNaProjektu.Select(u => u.Osoba).ToList(), 
                                      })
                                    .FirstOrDefaultAsync();
            if (projekt == null)
            {
                return NotFound($"Projekt s id {id} ne postoji");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                ViewBag.Filter = filter;

                return View(viewName, projekt);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id) {
            var projekt = await _context.Projekt
                                  .Include(p => p.VrstaProjekta)
                                  .Include(p => p.Racun)
                                  .Include(p => p.Posao)
                                  .Include(p => p.UlogaNaProjektu)
                                  .Where(p => p.IdProjekt == id)
                                  .SingleOrDefaultAsync();
            if (projekt != null) {
                return PartialView(projekt);
            } else {
                return NotFound($"Neispravan id projekta: {id}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0, int page = 1, int sort = 1, bool ascending = true)
        {
            var viewModel = new ProjektMDViewModel();
            if (id != 0)
            {
                viewModel.Projekt = await _context.Projekt
                    .Include(p => p.Dokument)
                        .ThenInclude(d => d.KategorijaDokumenta)
                    .Include(p => p.Racun)
                    .Include(p => p.Posao)
                        .ThenInclude(p => p.VrstaPosla)
                    .Include(p => p.VrstaProjekta)
                    .Include(p => p.UlogaNaProjektu)
                        .ThenInclude(u => u.Uloga)
                    .Include(p => p.UlogaNaProjektu)
                        .ThenInclude(u => u.Osoba)
                    .FirstOrDefaultAsync(p => p.IdProjekt == id);

                viewModel.RadniciList = viewModel.Projekt.UlogaNaProjektu.Select(u => u.Osoba).ToList();

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

            } 
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit(ProjektMDViewModel viewModel)
        {
            try
            {
                bool existing = false;
                if (viewModel.Projekt.IdProjekt == 0)
                {
                    _context.Add(viewModel.Projekt);
                }
                else
                {
                    existing = true;   
                    _context.Update(viewModel.Projekt);
                }
                await _context.SaveChangesAsync();


                foreach (var dokument in viewModel.Dokument)
                {
                    dokument.ProjektId = viewModel.Projekt.IdProjekt;
                    if (dokument.DokumentId != 0)
                    {
                        _context.Update(dokument);
                    }
                    else
                    {
                        dokument.ProjektId = viewModel.Projekt.IdProjekt;
                        _context.Add(dokument);
                    }

                }
                await _context.SaveChangesAsync();

                foreach (var posao in viewModel.Posao)
                {
                    posao.ProjektId = viewModel.Projekt.IdProjekt;
                    if (posao.Id != 0)
                    {
                        _context.Update(posao);
                    }
                    else
                    {
                        posao.ProjektId = viewModel.Projekt.IdProjekt;
                        _context.Add(posao);
                    }

                }
                await _context.SaveChangesAsync();

                foreach (var uloga in viewModel.UlogaNaProjektu)
                {
                    uloga.IdProjekt = viewModel.Projekt.IdProjekt;
                    if (uloga.Id != 0)
                    {
                        _context.Update(uloga);
                    }
                    else
                    {
                        uloga.IdProjekt = viewModel.Projekt.IdProjekt;
                        viewModel.Projekt.UlogaNaProjektu.Add(uloga);
                        _context.Add(uloga);
                    }
                }
                await _context.SaveChangesAsync();

                var oldProject = await _context.Projekt
                    .Include(p => p.Dokument)
                    .Include(p => p.Posao)
                    .FirstOrDefaultAsync(p => p.IdProjekt == viewModel.Projekt.IdProjekt);
                var oldDocs = oldProject.Dokument.ToList();
                var oldJobs = oldProject.Posao.ToList();
                var oldRoles = oldProject.UlogaNaProjektu.ToList();

                await DeleteObjects<Dokument>(oldDocs, viewModel.Dokument.ToList());
                await DeleteObjects<Posao>(oldJobs, viewModel.Posao.ToList());
                await DeleteRoles(viewModel.Projekt, viewModel.UlogaNaProjektu.ToList());


                string isNew = existing ? "uređen" : "dodan";
                _logger.LogInformation(new EventId(1000), $"MD projekt {viewModel.Projekt.ImeProjekta} {isNew}");
                TempData[Constants.Message] = $"MD projekt uspješno {isNew}. Id={viewModel.Projekt.IdProjekt}";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                _logger.LogError("Pogreška prilikom dodavanja/ uređivanja novog projekta: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                return View(viewModel);
            }

        }

        private async Task DeleteObjects<T>(IList<T> oldList, IList<T> newList)
        {
            foreach(var obj in oldList)
            {
                if(newList.IndexOf(obj) == -1)
                {
                    try
                    {
                        _context.Remove(obj);
                    }catch(Exception exc)
                    {
                        _logger.LogError(exc.ToString());
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task DeleteRoles(Projekt projekt, IList<UlogaNaProjektu> newList)
        {
            foreach (var uloga in projekt.UlogaNaProjektu)
            {
                if (newList.IndexOf(uloga) == -1)
                {
                    try
                    {
                        _context.Remove(uloga);
                        projekt.UlogaNaProjektu.Remove(uloga);
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError(exc.ToString());
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        [HttpDelete]
        public async Task Delete(int id)
        {
            var projekt = await _context.Projekt
                    .Include(p => p.Dokument)
                    .Include(p => p.Posao)
                    .FirstOrDefaultAsync(p => p.IdProjekt == id);
            var naziv = projekt.ImeProjekta;
            ActionResponseMessage responseMessage;
            if (projekt != null)
            {
                try
                {
                    foreach (var dokument in projekt.Dokument)
                    {
                        if (dokument != null)
                        {
                            _context.Remove(dokument);
                        }
                    }

                    foreach (var posao in projekt.Posao)
                    {
                        if (posao != null)
                        {
                            _context.Remove(posao);
                        }
                    }
                    _context.Remove(projekt);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Projekt: {naziv} uspješno obrisan");
                    TempData[Constants.Message] = $"Projekt: {naziv} uspješno obrisan";
                    TempData[Constants.ErrorOccurred] = false;
                    responseMessage = new ActionResponseMessage(MessageType.Success, $"Projekt {naziv} sa šifrom {id} uspješno obrisan.");
                } catch(Exception ex)
                {
                    _logger.LogError("Pogreška prilikom brisanja projekta: " + ex.CompleteExceptionMessage());
                    TempData[Constants.Message] = "Pogreška prilikom brisanja projekta: " + ex.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška prilikom brisanja projekta: {ex.CompleteExceptionMessage()}");
                }
                
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Posao sa šifrom {id} ne postoji");
            }
            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
        }
    }
}
