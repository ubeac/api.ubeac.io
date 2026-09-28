using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Product : BaseEntity
    {
        public Guid ManufacturerId { get; set; }
        public string Description { get; set; }
        public int ViewOrder { get; set; }
        public Guid ManualFile { get; set; }
        public Guid TechnicalSpecFile { get; set; }
        public Guid Image { get; set; }
        public string Url { get; set; }
        [BsonIgnore]
        public List<Firmware> Firmwares { get; set; }

        public Product()
        {
            Firmwares = new List<Firmware>();
        }
    }
}