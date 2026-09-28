using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using uBeac.PostProcessor.Models;
using uBeac.PostProcessor.Repositories;

namespace uBeac.PostProcessor.Services
{
    public interface IService
    {
        TeamsModel Teams { get; set; }
    }
    public class Service : IService
    {
        private readonly IRepository _repository;

        private readonly ILogger<Service> _logger;
        public TeamsModel Teams { get; set; }
        public Service(IRepository repository, ILogger<Service> logger, TeamsModel teamsModel)
        {
            _repository = repository;
            _logger = logger;
            Teams = teamsModel;
            Init().Wait();
        }

        private async Task Init()
        {
            try
            {
                var teams = await _repository.GetTeamsAsync();
                var devices = await _repository.GetDevicesAsync();
                var sensors = await _repository.GetSensorsAsync();

                foreach (var team in teams)
                {
                    Teams.Add(team.Id);
                }

                foreach (var device in devices)
                {
                    Teams.TryAddDevice(device.TeamId, device.Id, device.Uid);
                }

                foreach (var sensor in sensors)
                {
                    Teams.TryAddSensor(sensor.TeamId, sensor.DeviceId, sensor.Id, sensor.Uid, sensor.Persist, sensor.Schema.ToHashSet());
                }

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in cache initialization");
            }
        }
    }
}
