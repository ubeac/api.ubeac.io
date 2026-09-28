using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace uBeac.Models
{
    public class Team : BaseEntity
    {
        public string Namespace { get; set; }
        public string Description { get; set; }
        public Address Address { get; set; }
        [BsonIgnore]
        public List<Building> Buildings { get; set; }
        [BsonIgnore]
        public List<Gateway> Gateways { get; set; }
        [BsonIgnore]
        public List<Device> Devices { get; set; }
        [BsonIgnore]
        public List<DeviceSummary> DeviceSummaries { get; set; }
        [BsonIgnore]
        public List<Dashboard> Dashboards { get; set; }
        [BsonIgnore]
        public List<Access> Accesses { get; set; }
        [BsonIgnore]
        public List<UserProfile> Users { get; set; }
        public List<Token> Tokens { get; set; }
        public Team()
        {
            Tokens = new List<Token>();
        }
    }
    public class Token
    {
        public string AccessToken { get; set; }
        public AccessLevels Role { get; set; }
    }
}