using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Controllers
{
    public interface ICustomController<TKey, TModelUpdate, TModelCreate, TModelGet>
    {
        public Task<int> Count([FromQuery] string filter);
        public Task<List<TModelGet>> GetAll([FromQuery] LoadParams loadParams);
        public Task<ActionResult<TModelGet>> Get(TKey id);
        public Task<IActionResult> Create(TModelCreate model);
        public Task<IActionResult> Update(TKey id, TModelUpdate model);
        public Task<IActionResult> Delete(TKey id);
    }
}