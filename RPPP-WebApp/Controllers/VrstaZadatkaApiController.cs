using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using RPPP_WebApp.Util.ExceptionFilters;
using RPPP_WebApp.Models;
using RPPP_WebApp.Models.DTO;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Mapper;

namespace RPPP_WebApp.Controllers
{
    /// <summary>
    /// Kontroler za upravljanje vrstama zadataka putem API-ja.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(ProblemDetailsForSqlException))]
    public class VrstaZadatkaApiController : ControllerBase, ICustomController<int, VrstaZadatkaUpdateDTO, VrstaZadatkaPostDTO, VrstaZadatkaGetDTO>
    {
        private readonly RPPP09Context ctx;

        private static Dictionary<string, Expression<Func<VrstaZadatka, object>>> orderSelectors = new()
        {
            [nameof(VrstaZadatka.IdVrstaZdtk).ToLower()] = vs => vs.IdVrstaZdtk,
            [nameof(VrstaZadatka.NazivVrsteZdtk).ToLower()] = vs => vs.NazivVrsteZdtk,
        };

        /// <summary>
        /// Konstruktor kontrolera VrstaZadatkaApiController.
        /// </summary>
        /// <param name="ctx">Kontekst baze podataka.</param>
        public VrstaZadatkaApiController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }

        /// <summary>
        /// Vraća broj vrsta zadataka prema opcionalnom filtru.
        /// </summary>
        /// <param name="filter">Opcionalni filter po nazivu vrste zadatka.</param>
        /// <returns>Broj vrsta zadataka koji odgovaraju filtru.</returns>
        [HttpGet("count", Name = "BrojVrstaZadataka")]
        public async Task<int> Count([FromQuery] string filter)
        {
            var query = ctx.VrstaZadatka.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(s => s.NazivVrsteZdtk.Contains(filter));
            }
            int count = await query.CountAsync();
            return count;
        }

        /// <summary>
        /// Dohvaća sve vrste zadataka prema opcionalnom filtru i parametrima za straničenje i sortiranje.
        /// </summary>
        /// <param name="loadParams">Parametri za straničenje, filtriranje i sortiranje.</param>
        /// <returns>Lista DTO-ova koji predstavljaju vrste zadataka.</returns>
        [HttpGet(Name = "DohvatiVrsteZadataka")]
        public async Task<List<VrstaZadatkaGetDTO>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.VrstaZadatka.AsQueryable();

            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(vs => vs.NazivVrsteZdtk.Contains(loadParams.Filter));
            }

            if (loadParams.SortColumn != null)
            {
                if (orderSelectors.TryGetValue(loadParams.SortColumn.ToLower(), out var expr))
                {
                    query = loadParams.Descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
                }
            }

            var list = await query
                .Select(vs => vs)
                .Skip(loadParams.StartIndex)
                .Take(loadParams.Rows)
                .ToListAsync();

            return list.ConvertAll(vrstaZadatka => VrstaZadatkaMapper.ToGetDTO(vrstaZadatka));

        }

        /// <summary>
        /// Dohvaća vrstu zadatka prema ID-u.
        /// </summary>
        /// <param name="id">ID vrste zadatka.</param>
        /// <returns>ActionResult koji sadrži DTO vrste zadatka ako je pronađen, inače NotFound rezultat.</returns>
        [HttpGet("{id}", Name = "DohvatiVrstuzadatka")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VrstaZadatkaGetDTO>> Get(int id)
        {
            var vrstaZadatka = await ctx.VrstaZadatka
                                  .Where(vz => vz.IdVrstaZdtk == id)
                                  .Select(vz => vz)
                                  .FirstOrDefaultAsync();
            if (vrstaZadatka == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Ne postoji zadatak za id = {id}");
            }
            else
            {
                return VrstaZadatkaMapper.ToGetDTO(vrstaZadatka);
            }
        }

        /// <summary>
        /// Briše vrstu zadatka prema ID-u.
        /// </summary>
        /// <param name="id">ID vrste zadatka.</param>
        /// <returns>ActionResult koji predstavlja rezultat brisanja.</returns>
        [HttpDelete("{id}", Name = "ObrisiVrstuZadatka")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var vrstaZadatka = await ctx.VrstaZadatka.FindAsync(id);
            if (vrstaZadatka == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(vrstaZadatka);
                await ctx.SaveChangesAsync();

                return NoContent();
            };
        }

        /// <summary>
        /// Ažurira vrstu zadatka prema ID-u.
        /// </summary>
        /// <param name="id">ID vrste zadatka.</param>
        /// <param name="dto">DTO za ažuriranje vrste zadatka.</param>
        /// <returns>ActionResult koji predstavlja rezultat ažuriranja.</returns>
        [HttpPut("{id}", Name = "AzurirajVrstuZadatka")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, VrstaZadatkaUpdateDTO dto)
        {
            var model = VrstaZadatkaMapper.ToVrstaZadatka(dto);

            if (model.IdVrstaZdtk != id)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Razliciti id-evi {id} i {model.IdVrstaZdtk}");
            }
            else
            {
                var vrstaZadatka = await ctx.VrstaZadatka.FindAsync(id);
                if (vrstaZadatka == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Nevazeci id: {id}");
                }

                vrstaZadatka.NazivVrsteZdtk = model.NazivVrsteZdtk;

                await ctx.SaveChangesAsync();

                return NoContent();
            }
        }

        /// <summary>
        /// Stvara novu vrstu zadatka prema zadanim parametrima.
        /// </summary>
        /// <param name="dto">DTO za stvaranje nove vrste zadatka.</param>
        /// <returns>ActionResult koji predstavlja rezultat stvaranja.</returns>
        [HttpPost(Name = "DodajVrstuZadatka")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(VrstaZadatkaPostDTO dto)
        {
            var vrstaZadatka = VrstaZadatkaMapper.ToVrstaZadatka(dto);

            ctx.Add(vrstaZadatka);
            await ctx.SaveChangesAsync();

            var addedItem = await Get(vrstaZadatka.IdVrstaZdtk);

            return CreatedAtAction(nameof(Get), new { id = vrstaZadatka.IdVrstaZdtk }, addedItem.Value);
        }
    }
}
