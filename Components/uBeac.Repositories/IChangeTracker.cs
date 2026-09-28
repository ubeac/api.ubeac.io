using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace uBeac.Repositories
{
    public interface IChangeTracker
    {
        Task Init();
    }

    public interface IChangeTracker<TKey, TEntity> : IChangeTracker
    {
        void RegisterForInsert(Action<TEntity> action);
        void RegisterForUpdate(Action<TEntity, Dictionary<string, object>> action);
        void RegisterForDelete(Action<TKey> action);
    }

    public class ChangeEventArgs<TKey, TEntity> : EventArgs
    {
        public TEntity Value { get; }
        public TKey Id { get; }
        public ChangeTrackerTypes Type { get; }
        public Dictionary<string, object> UpdatedFields { get; set; }

        public ChangeEventArgs(ChangeTrackerTypes changeTrackerTypes, TKey id, TEntity value, Dictionary<string, object> updatedFields)
        {
            Value = value;
            Id = id;
            Type = changeTrackerTypes;
            UpdatedFields = updatedFields;
        }
        public ChangeEventArgs(ChangeTrackerTypes changeTrackerTypes, TKey id, TEntity value):this(changeTrackerTypes, id, value, null)
        {            
        }
    }

    public enum ChangeTrackerTypes
    {
        None,
        Inserted,
        Deleted,
        Updated
    }

}
