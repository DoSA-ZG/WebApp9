using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje ulogama.
    /// </summary>
    public class UlogaV2Controller : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<UlogaV2Controller> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Konstruktor kontrolera UlogaV2Controller.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        /// <param name="logger">Logger za bilježenje događaja.</param>
        public UlogaV2Controller(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<UlogaV2Controller> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Prikazuje popis uloga s paginacijom.
        /// </summary>
        /// <param name="page">Trenutna stranica.</param>
        /// <param name="sort">Način sortiranja.</param>
        /// <param name="ascending">Red sortiranja.</param>
        /// <returns>View s popisom uloga.</returns>
        public async Task<IActionResult> Index(int page = 1, int sort =  1, bool ascending =  true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Uloga.AsNoTracking();

            int count = await query.CountAsync();

            var pagingInfo = new PagingInfo()
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

            var uloge = await query
                    .Select(u => u)
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .ToListAsync();

            var model = new UlogeV2ViewModel
            {
                Uloge = uloge,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za stvaranje nove uloge.
        /// </summary>
        /// <returns>View s formom za stvaranje uloge.</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        /// <summary>
        /// Stvara novu ulogu.
        /// </summary>
        /// <param name="uloga">Podaci o novoj ulozi.</param>
        /// <returns>Preusmjeravanje na Index ako je uspješno stvorena, inače View s greškom.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Uloga uloga)
        {
            logger.LogTrace(JsonSerializer.Serialize(uloga));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(uloga);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Uloga {uloga.NazivUloge} dodana.");
                    TempData[Constants.Message] = $"Uloga {uloga.NazivUloge} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri dodaavanju nove uloge:  {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(uloga);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(uloga);
            }
        }

        /// <summary>
        /// Briše ulogu sa zadanim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator uloge koju treba obrisati.</param>
        /// <returns>Rezultat akcije.</returns>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var uloga = await ctx.Uloga.FindAsync(id);

            if (uloga != null)
            {
                var objekti = uloga.UlogaNaProjektu;
                if (objekti.Count == 0)
                {
                    try
                    {
                        string naziv = uloga.NazivUloge;
                        ctx.Remove(uloga);
                        await ctx.SaveChangesAsync();

                        logger.LogInformation($"Uloga {naziv} uspješno obrisana.");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Uloga {naziv} sa šifrom {id} uspješno obrisana.");
                    }
                    catch(Exception exc)
                    {
                        logger.LogError("Pogreška pri brisanju uloge: " +  exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška pri brisanju uloge: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Uloga sa šifrom {id} koristi se kao strani ključ za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Uloga sa šifrom {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });

            return new EmptyResult();
        }

        /// <summary>
        /// Prikazuje formu za uređivanje uloge s određenim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator uloge koju treba urediti.</param>
        /// <returns>View s formom za uređivanje uloge.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var uloga = await ctx.Uloga
                    .AsNoTracking()
                    .Where(u => u.IdUloga == id)
                    .SingleOrDefaultAsync();

            if (uloga != null)
            {
                return View(uloga);
            }
            else
            {
                return NotFound($"Neispravna šifra uloge: {id}");
            }
        }

        /// <summary>
        /// Uređuje postojeću ulogu.
        /// </summary>
        /// <param name="uloga">Podaci o uređenoj ulozi.</param>
        /// <returns>Presumjeravanje na Index ako je uspješno uređena, inače View s greškom.</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Uloga uloga)
        {
            if (uloga == null)
            {
                return NotFound("Nema poslanih podataka.");
            }

            bool checkId = await ctx.Uloga.AnyAsync(u => u.IdUloga == uloga.IdUloga);

            if (!checkId)
            {
                return NotFound($"Neispravna šifra uloge: {uloga?.IdUloga}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Update(uloga);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Uloga {uloga.NazivUloge} uspješno ažurirana.");
                    TempData[Constants.Message] = $"Uloga {uloga.NazivUloge} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri ažuriranju uloge: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(uloga);
                }
            }
            else
            {
                return View(uloga);
            }
        }

        /// <summary>
        /// Dohvaća detalje uloge s određenim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator uloge koju treba dohvatiti.</param>
        /// <returns>
        /// PartialView koji prikazuje detalje uloge.
        /// Ako uloga s traženim identifikatorom ne postoji, vraća se NotFound rezultat.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var uloga = await ctx.Uloga
                        .Where(u => u.IdUloga == id)
                        .Select(u => u)
                        .SingleOrDefaultAsync();

            if (uloga != null)
            {
                return PartialView(uloga);
            }
            else
            {
                return NotFound($"Neispravna šifra uloge: {id}");
            }
        }
    }
}