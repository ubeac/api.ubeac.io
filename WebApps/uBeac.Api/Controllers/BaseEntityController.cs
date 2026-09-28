using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Models;

namespace uBeac.Api.Controllers
{    
    public abstract class BaseEntityController<TFacade, TModel, TInputModelAdd, TInputModelUpdate> : BaseController 
            where TModel : IBaseEntity 
            where TFacade : IBaseEntityFacade<TModel>
    {
        protected readonly TFacade Facade;

        public BaseEntityController(TFacade facade)
        {
            facade.ThrowIfNull();
            Facade = facade;
        }

        [HttpPost]
        [ProducesJson]
        [SwaggerOperation(Summary = EntityConstants.ADD_SUMMARY, Description = EntityConstants.ADD_DESCRIPTION)]
        public virtual async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("", Required = true)] TInputModelAdd entity)
        {
            var model = Mapping.Mapper.Map<TModel>(entity);
            return await Facade.AddAsync(model);
        }

        [HttpPost]
        [ProducesJson]
        [SwaggerOperation(Summary = EntityConstants.UPDATE_SUMMARY, Description = EntityConstants.UPDATE_DESCRIPTION)]
        public virtual async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("", Required = true)] TInputModelUpdate entity)
        {
            var model = Mapping.Mapper.Map<TModel>(entity);
            return await Facade.UpdateAsync(model);
        }

        [HttpDelete("{id}")]
        [ProducesJson]
        [SwaggerOperation(Summary = EntityConstants.DELETE_SUMMARY, Description = EntityConstants.DELETE_DESCRIPTION)]
        public virtual async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await Facade.RemoveAsync(id);
        }

    }
}
