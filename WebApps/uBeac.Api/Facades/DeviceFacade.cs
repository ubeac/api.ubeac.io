using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IDeviceFacade : IBaseEntityFacade<Device>
    {
    }
    public class DeviceFacade : BaseEntityFacade<Device>, IDeviceFacade
    {
        public DeviceFacade(IDeviceService deviceService, ISecurityContext securityContext) : base(deviceService, securityContext)
        {
        }    
    }
}
