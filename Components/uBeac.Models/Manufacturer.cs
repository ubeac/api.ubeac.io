using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Manufacturer : BaseEntity
    {
        public string Description { get; set; }
        public string Website { get; set; }
        public Guid Logo { get; set; }
        [BsonIgnore]
        public List<Product> Products { get; set; }

        public Manufacturer()
        {
            Products = new List<Product>();
        }
    }
}