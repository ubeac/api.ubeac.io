/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public interface IProcessor
    {
        void Process(GatewayData gatewayData);
    }
}
