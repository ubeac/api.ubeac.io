using System.Threading.Tasks;
using uBeac.Models;

namespace uBeac.PostProcessor.ServiceProcessors
{
    public interface IProcessor
    {
        Task ProcessAsync(GatewayData gatewayData, DeviceRawData deviceRawData);
    }
}
