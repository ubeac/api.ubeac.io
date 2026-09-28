/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System.Threading.Tasks;

namespace uBeac.Messaging
{
    public interface IProducer : IMessagingClient
    {
        Task Send<T>(T data);
        Task SendBytes(byte[] bytes);
    }
}
