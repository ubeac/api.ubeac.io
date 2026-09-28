using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface IBaseEntityService<TEntity> : IBaseService<Guid, TEntity> where TEntity : IBaseEntity
    {
        Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    }
    public class BaseEntityService<TEntity> : BaseService<Guid, TEntity>, IBaseEntityService<TEntity> where TEntity : IBaseEntity
    {
        protected readonly IBaseEntityRepository<TEntity> BaseEntityRepository;

        public BaseEntityService(IBaseEntityRepository<TEntity> repository) : base(repository)
        {
            BaseEntityRepository = repository;
        }

        public override async Task<bool> UpdateAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            // extracting cerated date and create by from old data
            var oldModel = await Repository.GetByIdAsync(model.Id, cancellationToken);
            if (oldModel == null || model.TeamId != oldModel.TeamId)
                return false;

            model.CreateBy = oldModel.CreateBy;
            model.CreateDate = oldModel.CreateDate;
            model.UpdateDate = DateTime.UtcNow;

            return await base.UpdateAsync(model, cancellationToken);

        }
        
        public virtual async Task<List<TEntity>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await BaseEntityRepository.GetByTeamIdAsync(teamId, cancellationToken);
        }

        public virtual async Task<bool> DeleteByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await BaseEntityRepository.DeleteByTeamIdAsync(teamId, cancellationToken);
        }

        public override async Task<Guid> AddAsync(TEntity model, CancellationToken cancellationToken = default)
        {
            var createDate = DateTime.UtcNow;
            model.CreateDate = createDate;
            model.UpdateDate = createDate;
            model.Id = Guid.NewGuid();

            return await base.AddAsync(model, cancellationToken);
        }
    }
}
