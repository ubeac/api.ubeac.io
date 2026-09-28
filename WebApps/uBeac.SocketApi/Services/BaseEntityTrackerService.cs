using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Notifications;
using uBeac.Repositories;
using uBeac.SocketApi.Repositories;
using uBeac.WebSocket;

namespace uBeac.SocketApi.Services
{
    public class BaseEntityTrackerService<TEntity> : IChangeTrackerStartupService where TEntity : BaseEntity
    {
        IChangeTracker<Guid, TEntity> _changeTracker;
        private readonly IRepository<TEntity> _repository;
        private readonly ISocket _socket;
        private readonly ILogger<BaseEntityTrackerService<TEntity>> _logger;
        private Dictionary<Guid, Guid> _entitiesCache;
        private readonly object locker;
        private const string DELIMITER = "/";
        private const string CHANGELOG_PREFIX = "ChangeLog";

        private string _typeName = typeof(TEntity).Name;
        public BaseEntityTrackerService(IChangeTracker<Guid, TEntity> changeTracker, IRepository<TEntity> repository, ISocket socket, ILogger<BaseEntityTrackerService<TEntity>> logger)
        {
            locker = new object();
            _socket = socket;
            _repository = repository;
            _logger = logger;
            _changeTracker = changeTracker;
            _changeTracker.RegisterForInsert(entity => SendInsertNotification(entity));
            _changeTracker.RegisterForDelete(entityId => SendDeleteNotification(entityId));
            _changeTracker.RegisterForUpdate((entity, x) => SendUpdateNotification(entity, x));
        }

        public void Run()
        {
            _entitiesCache = _repository.GetAllAsync().Result;
            _changeTracker.Init();
        }

        protected virtual void SendInsertNotification(TEntity entity)
        {
            lock (locker)
            {
                _entitiesCache.Add(entity.Id, entity.TeamId);
            }
            SendToGroup(entity.Id, entity.TeamId, entity, ActionTypes.Added).Wait();
        }

        protected virtual void SendUpdateNotification(TEntity entity, Dictionary<string, object> updatedFields)
        {
            SendToGroup(entity.Id, entity.TeamId, entity, ActionTypes.Updated).Wait();
        }

        protected virtual void SendDeleteNotification(Guid entityId)
        {
            if (_entitiesCache.TryGetValue(entityId, out Guid teamId))
            {
                SendToGroup(entityId, teamId, null, ActionTypes.Deleted).Wait();
                lock (locker)
                {
                    _entitiesCache.Remove(entityId);
                }
            }
            else
            {
                _logger.LogError(string.Format(typeof(TEntity).Name + "Id {0} does not exists!", entityId.ToString()));
            }

        }

        protected async Task SendToGroup(Guid entityId, Guid teamId, TEntity entity, ActionTypes actionType)
        {
            try
            {
                ChangeLog changeLog = new ChangeLog()
                {
                    Id = entityId,
                    Action = actionType,
                    DateTime = DateTime.UtcNow,
                    TeamId = teamId,
                    Type = _typeName,
                    Value = entity
                };

                await _socket.SendToGroupAsync(GetGroupId(teamId), changeLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format("Error in sending notification in {0} for {1} with Id {2} ", actionType.ToString(), typeof(TEntity).Name, entityId.ToString()));
            }

        }
        
        protected string GetGroupId(Guid teamId)
        {
            return CHANGELOG_PREFIX + DELIMITER + teamId.ToString();
        }
    }
}
