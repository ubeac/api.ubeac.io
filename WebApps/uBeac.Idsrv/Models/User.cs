using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.Idsrv.Models
{
    public class User : IdentityUser<Guid>, IEntity<Guid>
    {
        public bool IsDeleted { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public List<UserLogin> Logins { get; set; }
        public List<UserClaim> Claims { get; set; }
        public List<UserToken> Tokens { get; set; }
        public List<string> Roles { get; set; }

        public User()
        {
            IsDeleted = false;
            Logins = new List<UserLogin>();
            Claims = new List<UserClaim>();
            Tokens = new List<UserToken>();
            Roles = new List<string>();
        }
    }

    public class UserClaim : IdentityUserClaim<string>
    {
    }

    public class UserLogin : IdentityUserLogin<string>
    {
    }

    public class UserRole : IdentityUserRole<string>
    {
    }

    public class UserToken : IdentityUserToken<string>
    {
    }
}
