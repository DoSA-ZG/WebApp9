using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje osobama.
    /// </summary>

    public class OsobaV2Controller : Controller
    {
        private readonly RPPP09Context ctx;
        private readonly ILogger<OsobaV2Controller> logger;
        private readonly AppSettings appSettings;

        /// <summary>
        /// Konstruktor klase OsobaV2Controller.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        /// <param name="options">Opcije aplikacije.</param>
        /// <param name="logger">Logger za bilježenje događaja.</param>

        public OsobaV2Controller(RPPP09Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<OsobaV2Controller> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }

        /// <summary>
        /// Metoda koja prikazuje početnu stranicu s popisom osoba i paginacijom.
        /// </summary>
        /// <param name="page">Broj stranice.</param>
        /// <param name="sort">Vrsta sortiranja.</param>
        /// <param name="ascending">Poredak sortiranja.</param>
        /// <returns>Pogled s popisom osoba.</returns>

        public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Osoba.AsNoTracking();

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

            var osobe = await query
                        .Include("Zadatak")
                        .Select(o => o)
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();

            var model = new OsobeV2ViewModel
            {
                Osobe = osobe,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        /// <summary>
        /// Prikazuje formu za stvaranje nove osobe.
        /// </summary>
        /// <returns>Pogled s obrascem za unos podataka o novoj osobi.</returns>

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownList();
            return View();
        }

        /// <summary>
        /// Stvara novu osobu na temelju podataka unesenih u formi.
        /// </summary>
        /// <param name="os">Podaci o novoj osobi.</param>
        /// <returns>
        /// Ako je uspješno stvorena nova osoba, preusmjerava na početnu stranicu s popisom osoba.
        /// Ako nije uspjelo stvaranje nove osobe, vraća pogled s obrascem za unos podataka o novoj osobi.
        /// </returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OsobaV2ViewModel os)
        {
            logger.LogTrace(JsonSerializer.Serialize(os));

            if (ModelState.IsValid) 
            {
                try
                {
                    var o = new Osoba
                    {
                        ImeOsobe = os.Osoba.ImeOsobe,
                        PrezimeOsobe = os.Osoba.PrezimeOsobe,
                        Email = os.Osoba.Email,
                        Telefon = os.Osoba.Telefon,
                        Iban = os.Osoba.Iban
                    };

                    ctx.Add(o);
                    await ctx.SaveChangesAsync();

                    if(!(o.ZaduzenaOsoba.Where(z => z.IdOsoba == o.IdOsoba && z.IdZadatak == z.IdZadatak).Any()))
                    {
                        var zad = new ZaduzenaOsoba
                        {
                            IdOsoba = o.IdOsoba,
                            IdZadatak = os.Zadatak.IdZadatak
                        };

                        ctx.Add(zad);
                        await ctx.SaveChangesAsync();

                        o.ZaduzenaOsoba.Add(zad);

                        var zdtk = ctx.Zadatak.Where(z => z.IdZadatak == os.Zadatak.IdZadatak).Select(z => z).First();
                        o.Zadatak.Add(zdtk);

                        ctx.Update(o);
                        await ctx.SaveChangesAsync();
                    }

                    var unp = new UlogaNaProjektu
                    {
                        IdProjekt = os.Projekt.IdProjekt,
                        IdUloga = os.Uloga.IdUloga,
                        IdOsoba = o.IdOsoba
                    };

                    ctx.Add(unp);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation(new EventId(1000), $"Osoba {os.Osoba.ImeOsobe} {os.Osoba.PrezimeOsobe} dodana.");

                    TempData[Constants.Message] = $"Osoba {os.Osoba.ImeOsobe} {os.Osoba.PrezimeOsobe} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri dodavanju nove osobe: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(os);
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(os);
            }
        }

        /// <summary>
        /// Priprema padajućih popisa uloga, projekata i zadataka za prikaz u pogledu.
        /// </summary>
        /// <returns>Task koji označava asinkrono izvršavanje metode.</returns>

        private async Task PrepareDropDownList()
        {
            var uloge = await ctx.Uloga.OrderBy(k => k.NazivUloge).Select(k => new { k.IdUloga, k.NazivUloge }).ToListAsync();
            ViewBag.Uloge = new SelectList(uloge, nameof(Uloga.IdUloga), nameof(Uloga.NazivUloge));

            var projekti = await ctx.Projekt.OrderBy(k => k.ImeProjekta).Select(k => new { k.IdProjekt, k.ImeProjekta }).ToListAsync();
            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.ImeProjekta));

            var zadatci = await ctx.Zadatak.OrderBy(k => k.Naziv).Select(k => new { k.IdZadatak, k.Naziv }).ToListAsync();
            ViewBag.Zadatci = new SelectList(zadatci, nameof(Zadatak.IdZadatak), nameof(Zadatak.Naziv));
        }

        /// <summary>
        /// Briše osobu iz baze podataka na temelju njenog identifikatora.
        /// </summary>
        /// <param name="id">Identifikator osobe koju treba obrisati.</param>
        /// <returns>
        /// Prazan rezultat koji označava uspješno izvršavanje operacije brisanja.
        /// </returns>

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            ActionResponseMessage responseMessage;
            var osoba = await ctx.Osoba.FindAsync(id);

            if (osoba != null)
            {
                var ulogeNaProjektu = osoba.UlogaNaProjektu;

                var zaduzeneOsobe = await ctx.ZaduzenaOsoba
                                    .Where(z => z.IdOsoba == id)
                                    .Select(z => z)
                                    .ToListAsync();

                var zadaci = await ctx.Zadatak
                                    .Where(z => z.IdOsoba == id)
                                    .Select(z => z)
                                    .ToListAsync();

                if(zadaci.Count == 0)
                {
                    try
                    {
                        ctx.RemoveRange(ulogeNaProjektu);
                        ctx.RemoveRange(zaduzeneOsobe);
                        await ctx.SaveChangesAsync();

                        string ime = osoba.ImeOsobe;
                        string prezime = osoba.PrezimeOsobe;
                        ctx.Remove(osoba);
                        await ctx.SaveChangesAsync();

                        logger.LogInformation($"Osoba {ime} {prezime} uspješno obrisana.");
                        responseMessage = new ActionResponseMessage(MessageType.Success, $"Osoba {ime} {prezime} sa šifrom {id} uspješno obrisana.");
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška pri brisanju osobe: " + exc.CompleteExceptionMessage());
                        responseMessage = new ActionResponseMessage(MessageType.Error, $"Pogreška pri brisanju osobe: {exc.CompleteExceptionMessage()}");
                    }
                }
                else
                {
                    responseMessage = new ActionResponseMessage(MessageType.Error, $"Osoba sa šifrom {id} koristi se kao strani ključ za neki objekt.");
                }
            }
            else
            {
                responseMessage = new ActionResponseMessage(MessageType.Error, $"Osoba sa šifrom {id} ne postoji.");
            }

            Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
            return new EmptyResult();
        }

        /// <summary>
        /// Prikazuje obrazac za uređivanje podataka o osobi.
        /// </summary>
        /// <param name="id">Identifikator osobe koju treba urediti.</param>
        /// <returns>
        /// Pogled koji omogućuje uređivanje podataka o osobi.
        /// Ako osoba s traženim identifikatorom ne postoji, vraća se NotFound rezultat.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var os = await ctx.Osoba.AsNoTracking().Where(o => o.IdOsoba == id).SingleOrDefaultAsync();
            var unp = await ctx.UlogaNaProjektu
                .AsNoTracking()
                .Include(u => u.Uloga)
                .Include(u => u.Projekt)
                .Where(u => u.IdOsoba == id)
                .ToListAsync();

            var osoba = new OsobaV2ViewModel
            {
                Osoba = os,
                Uloga = unp.First().Uloga,
                Projekt = unp.First().Projekt,
                Zadatak = new Zadatak()
            };

            if (osoba != null)
            {
                await PrepareDropDownList();
                return View(osoba);
            }
            else
            {
                return NotFound($"Neispravna šifra osobe: {id}");
            }
        }

        /// <summary>
        /// Ažurira podatke o osobi na temelju podataka iz obrasca.
        /// </summary>
        /// <param name="os">Model koji sadrži podatke o osobi za ažuriranje.</param>
        /// <returns>
        /// Ako su podaci valjani i ažuriranje uspješno, preusmjerava na akcijsku metodu Index.
        /// Ako nema poslanih podataka ili su podaci nevaljani, vraća NotFound ili ponovno prikazuje obrazac za uređivanje s porukama o greškama.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Edit(OsobaV2ViewModel os)
        {
            if (os == null)
            {
                return NotFound("Nema poslanih podataka.");
            }

            bool checkId = await ctx.Osoba.AnyAsync(o => o.IdOsoba == os.Osoba.IdOsoba);

            if (!checkId)
            {
                return NotFound($"Nesipravna šifra osobe: {os?.Osoba.IdOsoba}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var o = new Osoba
                    {
                        IdOsoba = os.Osoba.IdOsoba,
                        ImeOsobe = os.Osoba.ImeOsobe,
                        PrezimeOsobe = os.Osoba.PrezimeOsobe,
                        Email = os.Osoba.Email,
                        Telefon = os.Osoba.Telefon,
                        Iban = os.Osoba.Iban
                    };

                    if (!(o.ZaduzenaOsoba.Where(z => z.IdOsoba == o.IdOsoba && z.IdZadatak == z.IdZadatak).Any()))
                    {
                        var zad = new ZaduzenaOsoba
                        {
                            IdOsoba = o.IdOsoba,
                            IdZadatak = os.Zadatak.IdZadatak
                        };

                        ctx.Add(zad);
                        await ctx.SaveChangesAsync();

                        o.ZaduzenaOsoba.Add(zad);

                        var zdtk = ctx.Zadatak.Where(z => z.IdZadatak == os.Zadatak.IdZadatak).Select(z => z).First();
                        o.Zadatak.Add(zdtk);

                        ctx.Update(o);
                        await ctx.SaveChangesAsync();
                    }

                    var existingRow = await ctx.UlogaNaProjektu
                        .FirstOrDefaultAsync(u => u.IdOsoba == o.IdOsoba);

                    existingRow.IdProjekt = os.Projekt.IdProjekt;
                    existingRow.IdUloga = os.Uloga.IdUloga;

                    ctx.Update(existingRow);
                    await ctx.SaveChangesAsync();

                    logger.LogInformation($"Osoba {os.Osoba.ImeOsobe} {os.Osoba.PrezimeOsobe} uspješno ažurirana.");
                    TempData[Constants.Message] = $"Osoba {os.Osoba.ImeOsobe} {os.Osoba.PrezimeOsobe} uspješno ažurirana.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška pri ažuriranju osobe: " + exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    await PrepareDropDownList();
                    return View(os);
                }
            }
            else
            {
                await PrepareDropDownList();
                return View(os);
            }
        }

        /// <summary>
        /// Dohvaća detalje osobe s određenim identifikatorom.
        /// </summary>
        /// <param name="id">Identifikator osobe koju treba dohvatiti.</param>
        /// <returns>
        /// PartialView koji prikazuje detalje osobe.
        /// Ako osoba s traženim identifikatorom ne postoji, vraća se NotFound rezultat.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var osoba = await ctx.Osoba
                    .Where(o => o.IdOsoba == id)
                    .Select(o => o)
                    .SingleOrDefaultAsync();

            if (osoba != null)
            {
                return PartialView(osoba);
            }
            else
            {
                return NotFound($"Nesipravna šifra osobe: {id}");
            }
        }
    }
}
