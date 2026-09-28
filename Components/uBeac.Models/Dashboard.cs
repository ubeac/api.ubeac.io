using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Dashboard : BaseEntity
    {
        public string Description { get; set; }
        public double ViewOrder { get; set; }
        [BsonIgnore]
        public List<Widget> Widgets { get; set; }

        public Dashboard()
        {
            Widgets = new List<Widget>();
        }
    }
}
