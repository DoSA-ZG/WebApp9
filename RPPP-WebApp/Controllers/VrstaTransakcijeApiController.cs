using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using RPPP_WebApp.Util.ExceptionFilters;
using RPPP_WebApp.Models;
using RPPP_WebApp.Models.DTO;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Mapper;

namespace RPPP_WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(ProblemDetailsForSqlException))]
    public class VrstaTransakcijeApiController : ControllerBase,
        ICustomController<int, VrstaTransakcijeUpdateDTO, VrstaTransakcijePostDTO, VrstaTransakcijeGetDTO>
    {
        private readonly RPPP09Context ctx;

        private static Dictionary<string, Expression<Func<VrstaTransakcije, object>>> orderSelectors = new()
        {
            [nameof(VrstaTransakcije.IdvrstaTransakcije).ToLower()] = vs => vs.IdvrstaTransakcije,
            [nameof(VrstaTransakcije.Naziv).ToLower()] = vs => vs.Naziv,
        };


        public VrstaTransakcijeApiController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }


        /// <summary>
        /// Vraća broj svih vrsta transakcija filtriran prema nazivu vrste transakcije 
        /// </summary>
        /// <param name="filter">Opcionalni filter za naziv vrste transakcija</param>
        /// <returns></returns>
        [HttpGet("count", Name = "BrojVrstaTransakcija")]
        public async Task<int> Count([FromQuery] string filter)
        {
            var query = ctx.VrstaTransakcije.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(s => s.Naziv.Contains(filter));
            }

            int count = await query.CountAsync();
            return count;
        }

        [HttpGet(Name = "DohvatiVrsteTransakcija")]
        public async Task<List<VrstaTransakcijeGetDTO>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.VrstaTransakcije.AsQueryable();

            if (!string.IsNullOrWhiteSpace(loadParams.Filter))
            {
                query = query.Where(vs => vs.Naziv.Contains(loadParams.Filter));
            }

            if (loadParams.SortColumn != null)
            {
                if (orderSelectors.TryGetValue(loadParams.SortColumn.ToLower(), out var expr))
                {
                    query = loadParams.Descending ? query.OrderByDescending(expr) : query.OrderBy(expr);
                }
            }

            var list = await query.Select(vs => vs)
                .Skip(loadParams.StartIndex)
                .Take(loadParams.Rows)
                .ToListAsync();
            return list.ConvertAll(vrstaTransakcije => VrstaTransakcijeMapper.ToGetDTO(vrstaTransakcije));
        }

        [HttpGet("{id}", Name = "DohvatiVrstuTransakcije")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VrstaTransakcijeGetDTO>> Get(int id)
        {
            var vrstaTransakcije = await ctx.VrstaTransakcije
                .Where(vs => vs.IdvrstaTransakcije == id)
                .Select(vs => vs)
                .FirstOrDefaultAsync();
            if (vrstaTransakcije == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
            }
            else
            {
                return VrstaTransakcijeMapper.ToGetDTO(vrstaTransakcije);
            }
        }


        /// <summary>
        /// Brisanje vrste transakcije određenog s id
        /// </summary>
        /// <param name="id">Vrijednost primarnog ključa (Id vrsta transakcije)</param>
        /// <returns></returns>
        /// <response code="204">Ako je vrsta transakcije uspješno obrisana</response>
        /// <response code="404">Ako vrsta transakcije s poslanim id-om ne postoji</response>      
        [HttpDelete("{id}", Name = "ObrisiVrstuTransakcije")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var vrstaTransakcije = await ctx.VrstaTransakcije.FindAsync(id);
            if (vrstaTransakcije == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(vrstaTransakcije);
                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Ažurira vrstu transakcije
        /// </summary>
        /// <param name="id">parametar čija vrijednost jednoznačno identificira vrstu transakcije</param>
        /// <param name="dto">Podaci o vrsti transakcije. IdvrstaTransakcije mora se podudarati s parametrom id</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "AzurirajVrstuTransakcije")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, VrstaTransakcijeUpdateDTO dto)
        {
            var model = VrstaTransakcijeMapper.ToVrstaTransakcije(dto);

            //ModelState.IsValid i model != null provjera se automatski zbog [ApiController]
            if (model.IdvrstaTransakcije != id)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest,
                    detail: $"Different ids {id} vs {model.IdvrstaTransakcije}");
            }
            else
            {
                var vrstaTransakcije = await ctx.VrstaTransakcije.FindAsync(id);
                if (vrstaTransakcije == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }

                vrstaTransakcije.Naziv = model.Naziv;

                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Stvara novu vrstu transakcije opisanu poslanim modelom
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "DodajVrstuTransakcije")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(VrstaTransakcijePostDTO dto)
        {
            var vrstaTransakcije = VrstaTransakcijeMapper.ToVrstaTransakcije(dto);
            ctx.Add(vrstaTransakcije);
            await ctx.SaveChangesAsync();

            var addedItem = await Get(vrstaTransakcije.IdvrstaTransakcije);

            return CreatedAtAction(nameof(Get), new { id = vrstaTransakcije.IdvrstaTransakcije }, addedItem.Value);
        }
    }
}