using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(DeviceConstants.DESCRIPTION)]
    public class DeviceController : BaseEntityController<IDeviceFacade, Device, DeviceInputModelAdd, DeviceInputModelUpdate>
    {
        public DeviceController(IDeviceFacade deviceFacade) : base(deviceFacade)
        {
        }

        [SwaggerOperation(Summary = DeviceConstants.ADD_SUMMARY, Description = DeviceConstants.ADD_DESCRIPTION)]
        public override async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("Device", Required = true)] DeviceInputModelAdd device)
        {
            return await base.Add(device);
        }

        [SwaggerOperation(Summary = DeviceConstants.UPDATE_SUMMARY, Description = DeviceConstants.UPDATE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("Device", Required = true)] DeviceInputModelUpdate device)
        {
            return await base.Update(device);
        }

        [SwaggerOperation(Summary = DeviceConstants.DELETE_SUMMARY, Description = DeviceConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }
    }
}
