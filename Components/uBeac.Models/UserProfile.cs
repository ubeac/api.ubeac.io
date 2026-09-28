using System;

namespace uBeac.Models
{
    public class UserProfile : BaseEntity
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime? BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WebSite { get; set; }
        public Guid Picture { get; set; }
        public Address Address { get; set; }
        public int TimeZoneOffset { get; set; }
        public string TimeZone { get; set; }
    }
}
