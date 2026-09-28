using System;
using uBeac.Models;

namespace uBeac.Idsrv.InputModels
{
    // todo: add annotation
    public class UserProfileUpdateInputModel
    {
        public DateTime? BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WebSite { get; set; }
        public Guid Picture { get; set; }
        public Address Address { get; set; }
        public string TimeZone { get; set; }
        public int TimeZoneOffset { get; set; }
    }
}
