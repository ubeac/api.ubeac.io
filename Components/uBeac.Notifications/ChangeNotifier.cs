using System.Threading.Tasks;
using uBeac.Messaging.RabbitMQ;

namespace uBeac.Notifications
{
    public interface IChangeNotifier
    {
        Task Send(ChangeLog changeLog);
    }

    public class ChangeNotifier : Producer, IChangeNotifier
    {
        public ChangeNotifier(MessagingClientOptions<ChangeNotifier> messagingClientOptions) : base(messagingClientOptions)
        {
        }

        public async Task Send(ChangeLog changeLog)
        {
            await Send<ChangeLog>(changeLog);
        }
    }
}
