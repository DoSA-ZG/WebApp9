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
    [ApiController]
    [Route("[controller]")]
    [TypeFilter(typeof(ProblemDetailsForSqlException))]
    public class VrstaSuradnikaApiController : ControllerBase, ICustomController<int, VrstaSuradnikaUpdateDTO, VrstaSuradnikaPostDTO, VrstaSuradnikaGetDTO>
    {
        private readonly RPPP09Context ctx;
        private static Dictionary<string, Expression<Func<VrstaSuradnika, object>>> orderSelectors = new()
        {
            [nameof(VrstaSuradnika.IdVrstaSuradnika).ToLower()] = vs => vs.IdVrstaSuradnika,
            [nameof(VrstaSuradnika.Naziv).ToLower()] = vs => vs.Naziv,
            [nameof(VrstaSuradnika.Opis).ToLower()] = vs => vs.Opis,
        };


        public VrstaSuradnikaApiController(RPPP09Context ctx)
        {
            this.ctx = ctx;
        }


        /// <summary>
        /// Vraća broj svih vrsta suradnika filtriran prema nazivu vrste suradnika 
        /// </summary>
        /// <param name="filter">Opcionalni filter za naziv vrste suradnika</param>
        /// <returns></returns>
        [HttpGet("count", Name = "BrojVrstaSuradnika")]
        public async Task<int> Count([FromQuery] string filter)
        {
            var query = ctx.VrstaSuradnika.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(s => s.Naziv.Contains(filter));
            }
            int count = await query.CountAsync();
            return count;
        }

        [HttpGet(Name = "DohvatiVrsteSuradnika")]
        public async Task<List<VrstaSuradnikaGetDTO>> GetAll([FromQuery] LoadParams loadParams)
        {
            var query = ctx.VrstaSuradnika.AsQueryable();

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
            return list.ConvertAll(vrstaSuradnika => VrstaSuradnikaMapper.ToGetDTO(vrstaSuradnika));
            
        }

        [HttpGet("{id}", Name = "DohvatiVrstuSuradnika")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VrstaSuradnikaGetDTO>> Get(int id)
        {
            var vrstaSuradnika = await ctx.VrstaSuradnika
                                  .Where(vs => vs.IdVrstaSuradnika == id)
                                  .Select(vs => vs)
                                  .FirstOrDefaultAsync();
            if (vrstaSuradnika == null)
            {
                return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
            }
            else
            {
                return VrstaSuradnikaMapper.ToGetDTO(vrstaSuradnika) ;
            }
        }


        /// <summary>
        /// Brisanje vrste suradnika određenog s id
        /// </summary>
        /// <param name="id">Vrijednost primarnog ključa (Id vrsta suradnika)</param>
        /// <returns></returns>
        /// <response code="204">Ako je vrsta suradnika uspješno obrisana</response>
        /// <response code="404">Ako vrsta suradnika s poslanim id-om ne postoji</response>      
        [HttpDelete("{id}", Name = "ObrisiVrstuSuradnika")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
            if (vrstaSuradnika == null)
            {
                return NotFound();
            }
            else
            {
                ctx.Remove(vrstaSuradnika);
                await ctx.SaveChangesAsync();
                return NoContent();
            };
        }

        /// <summary>
        /// Ažurira vrstu suradnika
        /// </summary>
        /// <param name="id">parametar čija vrijednost jednoznačno identificira vrstu suradnika</param>
        /// <param name="model">Podaci o vrsti suradnika. IdVrstaSuradnika mora se podudarati s parametrom id</param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "AzurirajVrstuSuradnika")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, VrstaSuradnikaUpdateDTO dto)
        {
            var model = VrstaSuradnikaMapper.ToVrstaSuradnika(dto);

            if (model.IdVrstaSuradnika != id) //ModelState.IsValid i model != null provjera se automatski zbog [ApiController]
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.IdVrstaSuradnika}");
            }
            else
            {
                var vrstaSuradnika = await ctx.VrstaSuradnika.FindAsync(id);
                if (vrstaSuradnika == null)
                {
                    return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
                }

                vrstaSuradnika.Naziv = model.Naziv;
                vrstaSuradnika.Opis = model.Opis;

                await ctx.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Stvara novu vrstu suradnika opisom poslanim modelom
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost(Name = "DodajVrstuSuradnika")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(VrstaSuradnikaPostDTO dto)
        {
            var vrstaSuradnika = VrstaSuradnikaMapper.ToVrstaSuradnika(dto);
            ctx.Add(vrstaSuradnika);
            await ctx.SaveChangesAsync();

            var addedItem = await Get(vrstaSuradnika.IdVrstaSuradnika);

            return CreatedAtAction(nameof(Get), new { id = vrstaSuradnika.IdVrstaSuradnika }, addedItem.Value);
        }
    }

}
