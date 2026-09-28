using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(AccessConstants.DESCRIPTION)]
    public class AccessController : BaseController
    {
        private readonly IAccessFacade _accessFacade;
        public AccessController(IAccessFacade accessFacade)
        {
            _accessFacade = accessFacade;
        }

        [HttpPost]
        [SwaggerOperation(Summary = AccessConstants.ADD_SUMMARY, Description = AccessConstants.ADD_DESCRIPTION)]
        public virtual async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("", Required = true)] AccessInputModelAdd access)
        {
            var _access = Mapping.Mapper.Map<Access>(access);
            return await _accessFacade.AddAsync(_access);
        }

        [HttpPost]
        [SwaggerOperation(Summary = AccessConstants.UPDATE_SUMMARY, Description = AccessConstants.UPDATE_DESCRIPTION)]
        public virtual async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("", Required = true)] AccessInputModelUpdate access)
        {
            var _access = Mapping.Mapper.Map<Access>(access);
            return await _accessFacade.UpdateAsync(_access);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = AccessConstants.DELETE_SUMMARY, Description = AccessConstants.DELETE_DESCRIPTION)]
        public virtual async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await _accessFacade.RemoveAsync(id);
        }

    }
}
