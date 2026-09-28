using Microsoft.AspNetCore.Identity;

namespace uBeac.Idsrv.Models
{
    public class Role : IdentityRole<string>
    {
        public Role(string roleName) : base(roleName)
        {
            Id = roleName.ToUpperInvariant();
        }
    }
}
