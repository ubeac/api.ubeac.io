/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System.Threading.Tasks;

namespace uBeac.Messaging
{
    public interface IConsumer: IMessagingClient
    {
        Task Handler(object sender, DeliverEventArgs deliverEventArgs);
    }
}
