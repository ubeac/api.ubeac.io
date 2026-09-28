using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Building : BaseEntity
    {
        public string Description { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
        public Address Address { get; set; }
        [BsonIgnore]
        public List<Floor> Floors { get; set; }

        public Building()
        {
            Floors = new List<Floor>();
        }

    }
}
