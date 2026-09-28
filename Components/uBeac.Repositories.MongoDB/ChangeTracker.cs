using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace uBeac.Repositories.MongoDB
{
    public class ChangeTracker<TKey, TEntity> : IChangeTracker<TKey, TEntity>
    {
        private readonly IMongoDatabase _database;
        private BsonDocument resume_token = null;
        private Action<TEntity, Dictionary<string, object>> update_action;
        private Action<TKey> delete_action;
        private Action<TEntity> insert_action;
        private readonly ILogger<IChangeTracker> _logger;

        public ChangeTracker(MongoDatabaseFactory database, ILogger<IChangeTracker> logger)
        {
            _database = database.GetMongoDB();
            _logger = logger;
        }

        public void RegisterForInsert(Action<TEntity> action)
        {
            insert_action = action;
        }

        public void RegisterForDelete(Action<TKey> action)
        {
            delete_action = action;
        }

        public void RegisterForUpdate(Action<TEntity, Dictionary<string, object>> action)
        {
            update_action = action;
        }

        public async Task ChangeHandler(object sender, ChangeEventArgs<TKey, TEntity> changeEventArgs)
        {
            var entity = changeEventArgs.Value;
            await Task.FromResult(0);

            switch (changeEventArgs.Type)
            {
                case ChangeTrackerTypes.Inserted:
                    insert_action.Invoke(entity);
                    return;

                case ChangeTrackerTypes.Deleted:
                    delete_action.Invoke(changeEventArgs.Id);
                    return;

                case ChangeTrackerTypes.Updated:
                    update_action.Invoke(entity, changeEventArgs.UpdatedFields);
                    return;
                case ChangeTrackerTypes.None:
                    _logger.LogError(entity.GetType().Name + " ChangeTrackerTypes.None is not supported in MongoDB ChangeStream!");
                    return;
                default:
                    _logger.LogError(entity.GetType().Name + " ChangeTrackerTypes is not defined in MongoDB ChangeStream!");
                    return;
            }
        }

        public async Task Init()
        {
            var collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);

            var option = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup, ResumeAfter = resume_token };

            var operationTypes = new List<ChangeStreamOperationType>();

            if (insert_action != null)
                operationTypes.Add(ChangeStreamOperationType.Insert);
            if (update_action != null)
            {
                operationTypes.Add(ChangeStreamOperationType.Update);
                operationTypes.Add(ChangeStreamOperationType.Replace);
            }
            if (delete_action != null)
                operationTypes.Add(ChangeStreamOperationType.Delete);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TEntity>>().Match(x => operationTypes.Contains(x.OperationType));

            var cursor = await collection.WatchAsync(pipeline, option);

            foreach (var change in cursor.ToEnumerable())
            {
                resume_token = change.ResumeToken;
                var type = ChangeTrackerTypes.None;
                var id = BsonSerializer.Deserialize<TKey>(change.DocumentKey.GetValue("_id").ToJson());

                switch (change.OperationType)
                {
                    case ChangeStreamOperationType.Insert:
                        type = ChangeTrackerTypes.Inserted;
                        await ChangeHandler(this, new ChangeEventArgs<TKey, TEntity>(type, id, change.FullDocument));
                        break;

                    case ChangeStreamOperationType.Update:
                        type = ChangeTrackerTypes.Updated;
                        await ChangeHandler(this, new ChangeEventArgs<TKey, TEntity>(type, id, change.FullDocument, change.UpdateDescription.UpdatedFields.AsBsonDocument.ToDictionary()));
                        break;

                    case ChangeStreamOperationType.Replace:
                        type = ChangeTrackerTypes.Updated;
                        await ChangeHandler(this, new ChangeEventArgs<TKey, TEntity>(type, id, change.FullDocument));
                        break;

                    case ChangeStreamOperationType.Delete:
                        type = ChangeTrackerTypes.Deleted;
                        await ChangeHandler(this, new ChangeEventArgs<TKey, TEntity>(type, id, change.FullDocument));
                        break;

                    case ChangeStreamOperationType.Invalidate:
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
