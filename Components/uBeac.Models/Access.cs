using System;

namespace uBeac.Models
{
    public class Access
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateBy { get; set; }
        public Guid TeamId { get; set; }
        public Guid UserId { get; set; }
        public AccessLevels Level { get; set; }

        public Access()
        {
        }
        public Access(Guid userId, Guid teamId, AccessLevels level)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.UtcNow;
            UserId = userId;
            Level = level;
            TeamId = teamId;
            CreateBy = userId;
        }
    }
}
