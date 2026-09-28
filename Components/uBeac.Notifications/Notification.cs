using System;

namespace uBeac.Notifications
{
    public static class Notification
    {
        static IChangeNotifier _changeNotifier;
        public static IChangeNotifier Instance
        {
            get => _changeNotifier;
            set => _changeNotifier = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
