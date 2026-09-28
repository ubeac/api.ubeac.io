using System;
using System.Collections.Generic;

namespace uBeac
{
    public interface IApplicationIdentity
    {
        Guid UserId { get; }
        string Username { get; }
        List<string> Roles { get; }
    }
}
