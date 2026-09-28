using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Models;

namespace uBeac.Api.Controllers
{
    [SwaggerTag(SensorConstants.DESCRIPTION)]
    public class SensorController : BaseEntityController<ISensorFacade, Sensor, SensorInputModelAdd, SensorInputModelUpdate>
    {
        private readonly ISensorFacade _sensorFacade;
        public SensorController(ISensorFacade sensorFacade) : base(sensorFacade)
        {
            sensorFacade.ThrowIfNull();
            _sensorFacade = sensorFacade;
        }

        [SwaggerOperation(Summary = SensorConstants.DELETE_SUMMARY, Description = SensorConstants.DELETE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Remove(Guid id)
        {
            return await base.Remove(id);
        }

        [HttpGet]
        [ProducesJson]
        [SwaggerOperation(Summary = SensorConstants.GETDATA_SUMMARY, Description = SensorConstants.GETDATA_DESCRIPTION)]
        public async Task<ResultSet<List<SensorData>>> GetData(Guid teamId, DateTime? fromDate = null, DateTime? toDate = null, List<Guid> deviceIds = null, List<Guid> sensorIds = null,
                                                                int pageNumber = 1, int pageSize = 20)
        {
            var FromDate = fromDate != null ? (DateTime)fromDate : DateTime.UtcNow.AddDays(-1);
            var ToDate = toDate != null ? (DateTime)toDate : DateTime.UtcNow;
            if (teamId == Guid.Empty)
                return new ResultSet<List<SensorData>>(new List<SensorData>());

            return await _sensorFacade.GetDataAsync(teamId, deviceIds, sensorIds, FromDate, ToDate, pageSize, pageNumber);
        }

        [SwaggerOperation(Summary = SensorConstants.ADD_SUMMARY, Description = SensorConstants.ADD_DESCRIPTION)]
        public override async Task<ResultSet<Guid>> Add([FromBody, SwaggerParameter("Sensor", Required = true)] SensorInputModelAdd sensor)
        {
            if (sensor.DeviceId == Guid.Empty || sensor.DeviceId == null)
                return new ResultSet<Guid>(ResponseCodes.BadRequest);

            return await base.Add(sensor);
        }

        [SwaggerOperation(Summary = SensorConstants.UPDATE_SUMMARY, Description = SensorConstants.UPDATE_DESCRIPTION)]
        public override async Task<ResultSet<bool>> Update([FromBody, SwaggerParameter("Sensor", Required = true)] SensorInputModelUpdate sensor)
        {
            if (sensor.DeviceId == Guid.Empty || sensor.DeviceId == null)
                return new ResultSet<bool>(ResponseCodes.BadRequest);

            return await base.Update(sensor);
        }
    }
}
