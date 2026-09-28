/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

namespace uBeac.Messaging
{
    public interface ISubscriber: IMessagingClient
    {
        void Handler(object sender, DeliverEventArgs deliverEventArgs);
    }
}
