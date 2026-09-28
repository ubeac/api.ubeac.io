using System;
using System.Collections.Generic;

namespace uBeac
{
    public class DummyApplicationContext : IApplicationContext
    {
        public IApplicationIdentity User => new DummyApplicationIdentity();
    }
    public class DummyApplicationIdentity : IApplicationIdentity
    {
        public Guid UserId => new Guid();

        public string Username => string.Empty;

        public List<string> Roles => new List<string>();
    }
}
