using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    public abstract class AdminBaseEntityController<TEntity, TInputModel> : AdminBaseController where TEntity : IBaseEntity
    {
        protected readonly IBaseEntityFacade<TEntity>  BaseEntityFacade;

        public AdminBaseEntityController(IBaseEntityFacade<TEntity> baseEntityFacade)
        {
            baseEntityFacade.ThrowIfNull();
            BaseEntityFacade = baseEntityFacade;
        }

        [HttpDelete("{id}")]
        public async virtual Task<ResultSet<bool>> Remove(Guid id)
        {
            var result = new ResultSet<bool>();
            try
            {
                return await BaseEntityFacade.RemoveAsync(id);
            }
            catch (Exception ex)
            {
                result.AddError(new Error(ex));
            }
            return result;
        }

        [HttpPost]
        public async virtual Task<ResultSet<Guid>> Add([FromBody] TInputModel model)
        {
            var entity = Mapping.Mapper.Map<TEntity>(model);
            return await BaseEntityFacade.AddAsync(entity);
            //return new ResultSet<Guid>(result.Data);
        }

        [HttpPost]
        public async virtual Task<ResultSet<bool>> Update([FromBody] TInputModel model)
        {
            var entity = Mapping.Mapper.Map<TEntity>(model);
            return await BaseEntityFacade.UpdateAsync(entity);
        }

    }
}
