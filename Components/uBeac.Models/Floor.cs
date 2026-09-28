using System;

namespace uBeac.Models
{
    public class Floor : BaseEntity
    {
        public string Description { get; set; }
        public Guid PlanFileId { get; set; }
        public Guid BuildingId { get; set; }
    }
}
