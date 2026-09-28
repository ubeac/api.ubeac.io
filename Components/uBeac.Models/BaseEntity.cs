using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Serialization;

namespace uBeac.Models
{
    public interface IBaseEntity : IEntity<Guid>
    {
        DateTime CreateDate { get; set; }
        DateTime UpdateDate { get; set; }
        Guid CreateBy { get; set; }
        Guid UpdateBy { get; set; }
        string Name { get; set; }
        Dictionary<string, object> Attributes { get; set; }
        Guid TeamId { get; set; }

    }

    public abstract class BaseEntity : IBaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(CustomDictionarySerializer))]
        public Dictionary<string, object> Attributes { get; set; }
        public Guid TeamId { get; set; }

        public BaseEntity()
        {
            Attributes = new Dictionary<string, object>();
        }
    }
}
