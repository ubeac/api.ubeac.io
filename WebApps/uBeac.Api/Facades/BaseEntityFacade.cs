using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Api.Services;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IBaseEntityFacade<TEntity> where TEntity : IBaseEntity
    {
        Task<ResultSet<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ResultSet<Guid>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    }
    public class BaseEntityFacade<TEntity> : IBaseEntityFacade<TEntity> where TEntity : IBaseEntity
    {
        protected IBaseEntityService<TEntity> BaseEntityService { get; }
        protected ISecurityContext SecurityContext { get; }

        public BaseEntityFacade(IBaseEntityService<TEntity> baseEntityService, ISecurityContext securityContext)
        {
            baseEntityService.ThrowIfNull();
            BaseEntityService = baseEntityService;
            SecurityContext = securityContext;
        }

        public virtual async Task<ResultSet<TEntity>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await BaseEntityService.GetByIdAsync(id, cancellationToken);

            if (result == null)
                return new ResultSet<TEntity>(ResponseCodes.NotFound);

            if (!SecurityContext.HasAccess<Team>(AccessLevels.View, result.TeamId))
                return new ResultSet<TEntity>(ResponseCodes.UnAuthorized);

            return new ResultSet<TEntity>(result);
        }

        public virtual async Task<ResultSet<Guid>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.CreateBy = SecurityContext.Identity.UserId;
            entity.UpdateBy = SecurityContext.Identity.UserId;

            //todo: check this condition. The commented part prevents admin methods like add manufacturer

            if (/*entity.TeamId == null || entity.TeamId == Guid.Empty ||*/ !SecurityContext.HasAccess<Team>(AccessLevels.Admin, entity.TeamId))
                return new ResultSet<Guid>(Guid.Empty, ResponseCodes.UnAuthorized);

            var result = await BaseEntityService.AddAsync(entity, cancellationToken);

            if (result == Guid.Empty)
                return new ResultSet<Guid>(result, ResponseCodes.BadRequest);

            return new ResultSet<Guid>(result);
        }

        public virtual async Task<ResultSet<bool>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity.UpdateBy = SecurityContext.Identity.UserId;

            //todo: check this condition. The commented part prevents admin methods like add manufacturer

            if (/*entity.TeamId == null || entity.TeamId == Guid.Empty ||*/ !SecurityContext.HasAccess<Team>(AccessLevels.Admin, entity.TeamId))
                return new ResultSet<bool>(false, ResponseCodes.UnAuthorized);

            var result = await BaseEntityService.UpdateAsync(entity, cancellationToken);
            if (result)            
                return new ResultSet<bool>(result);            

            return new ResultSet<bool>(result, ResponseCodes.BadRequest);
        }

        public virtual async Task<ResultSet<bool>> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BaseEntityService.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return new ResultSet<bool>(false, ResponseCodes.NotFound);

            if (!SecurityContext.HasAccess<Team>(AccessLevels.Admin, entity.TeamId))
                return new ResultSet<bool>(false, ResponseCodes.UnAuthorized);

            var result = await BaseEntityService.DeleteAsync(id, cancellationToken);

            if (!result)
                return new ResultSet<bool>(result, ResponseCodes.NotAcceptable);

            return new ResultSet<bool>(result);
        }

    }
}
