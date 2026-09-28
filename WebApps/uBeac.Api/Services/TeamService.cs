using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Middlewares;
using uBeac.Api.Repositories;
using uBeac.Models;

namespace uBeac.Api.Services
{
    public interface ITeamService : IBaseEntityService<Team>
    {
        Task<bool> ExistsAsync(string teamNamespace, CancellationToken cancellationToken = default);
        Task<Tuple<Guid, int>> GetAccessAsync(HttpRequest request, CancellationToken cancellationToken = default);
        Task<ResultSet<string>> InvokeTokenAsync(Guid teamId, AccessLevels accessLevel, CancellationToken cancellationToken = default);
        Task<ResultSet<bool>> RemoveTokenAsync(Guid teamId, string token, CancellationToken cancellationToken = default);
    }

    public class TeamService : BaseEntityService<Team>, ITeamService
    {
        private readonly ITeamRepository _repository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IFloorRepository _floorRepository;
        private readonly IGatewayRepository _gatewayRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ISensorRepository _sensorRepository;
        private readonly IDeviceSummaryRepository _deviceSummaryRepository;
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IWidgetRepository _widgetRepository;
        private readonly IAccessRepository _accessRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IGatewayDataRepository _gatewayDataRepository;
        private readonly ISensorDataRepository _sensorDataRepository;
        private readonly AesEncryption _encryption;

        public TeamService(ITeamRepository teamRepository,
                           IBuildingRepository buildingRepository,
                           IGatewayRepository gatewayRepository,
                           IDeviceRepository deviceRepository,
                           IDeviceSummaryRepository deviceSummaryRepository,
                           IDashboardRepository dashboardRepository,
                           IAccessRepository accessRepository,
                           IUserProfileRepository userProfileRepository,
                           IFloorRepository floorRepository,
                           IWidgetRepository widgetRepository,
                           ISensorRepository sensorRepository,
                           IGatewayDataRepository gatewayDataRepository,
                           ISensorDataRepository sensorDataRepository,
                           AesEncryption encryption) : base(teamRepository)
        {
            _repository = teamRepository;
            _buildingRepository = buildingRepository;
            _floorRepository = floorRepository;
            _gatewayRepository = gatewayRepository;
            _deviceRepository = deviceRepository;
            _deviceSummaryRepository = deviceSummaryRepository;
            _dashboardRepository = dashboardRepository;
            _accessRepository = accessRepository;
            _userProfileRepository = userProfileRepository;
            _widgetRepository = widgetRepository;
            _sensorRepository = sensorRepository;
            _gatewayDataRepository = gatewayDataRepository;
            _sensorDataRepository = sensorDataRepository;
            _encryption = encryption;
        }

        public override Task<List<Team>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            throw new Exception("This method is not allowed to be called for Team!");
        }

        public override async Task<Team> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // get team
            var team = await base.GetByIdAsync(id, cancellationToken);

            if (team is null)
                return null;

            // extracting buildings
            var buildingTask = _buildingRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting floor
            var floorTask = _floorRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting gateways
            var gatewayTask = _gatewayRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting devices
            var deviceTask = _deviceRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting sensors
            var sensorTask = _sensorRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting device summaries
            var deviceSummaryTask = _deviceSummaryRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting dashboards
            var dashboardTask = _dashboardRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting widget
            var widgetTask = _widgetRepository.GetByTeamIdAsync(id, cancellationToken);

            // extracting access
            var accessTask = _accessRepository.GetByTeamIdAsync(id, cancellationToken);

            // waiting for all results
            await Task.WhenAll(buildingTask, gatewayTask, deviceTask, dashboardTask, accessTask, floorTask, sensorTask, widgetTask);

            team.Buildings = buildingTask.Result;
            team.Gateways = gatewayTask.Result;
            team.Devices = deviceTask.Result;
            team.DeviceSummaries = deviceSummaryTask.Result;
            team.Dashboards = dashboardTask.Result;
            team.Accesses = accessTask.Result;

            deviceTask.Result.ForEach(device => device.Sensors = sensorTask.Result.Where(sensor => sensor.DeviceId == device.Id).ToList());
            dashboardTask.Result.ForEach(dashboard => dashboard.Widgets = widgetTask.Result.Where(widget => widget.DashboardId == dashboard.Id).ToList());
            buildingTask.Result.ForEach(building => building.Floors = floorTask.Result.Where(floor => floor.BuildingId == building.Id).ToList());
            team.Tokens.ForEach(token => token.AccessToken = _encryption.Encrypt(token.AccessToken));

            // extracting team users
            var userIds = accessTask.Result.Select(x => x.UserId).Distinct();
            team.Users = await _userProfileRepository.GetByIdsAsync(userIds, cancellationToken);

            return team;
        }

        public override async Task<Guid> AddAsync(Team model, CancellationToken cancellationToken = default)
        {
            // checking for namespace
            if (string.IsNullOrEmpty(model.Namespace))
                return Guid.Empty;

            var team = await _repository.GetByNamespaceAsync(model.Namespace, cancellationToken);
            if (team != null)
                return Guid.Empty;

            // we should set the default base properties because we don't want to use base method
            // we need team id to be set for TeamId property before insertion
            var createDate = DateTime.UtcNow;
            model.CreateDate = createDate;
            model.UpdateDate = createDate;
            model.Id = Guid.NewGuid();
            model.TeamId = model.Id;

            var newId = await _repository.InsertAsync(model, cancellationToken);

            var gatewayCollectionTask = _gatewayDataRepository.CreateCollectionByTeamIdAsync(newId, cancellationToken);
            var sensorCollectionTask = _sensorDataRepository.CreateCollectionByTeamIdAsync(newId, cancellationToken);
            var newAccess = new Access(model.CreateBy, newId, AccessLevels.Admin);
            var accessTask = _accessRepository.InsertAsync(newAccess, cancellationToken);

            await Task.WhenAll(gatewayCollectionTask, sensorCollectionTask, accessTask);

            return newId;
        }

        public override async Task<bool> UpdateAsync(Team model, CancellationToken cancellationToken = default)
        {
            var team = await _repository.GetByNamespaceAsync(model.Namespace, cancellationToken);
            if (team != null && team.Id != model.Id)
                return false;

            var oldModel = await _repository.GetByIdAsync(model.Id, cancellationToken);
            if (oldModel == null)
                return false;

            model.CreateBy = oldModel.CreateBy;
            model.CreateDate = oldModel.CreateDate;
            model.UpdateDate = DateTime.UtcNow;
            model.Tokens = oldModel.Tokens;

            return await _repository.UpdateAsync(model, cancellationToken);
        }

        public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>
            {
                _buildingRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting buildings
                _floorRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting floors
                _gatewayRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting gateways
                _gatewayDataRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting gateways data
                _deviceRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting devices
                _sensorRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting sensors
                _sensorDataRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting sensors data
                _deviceSummaryRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting device summary
                _dashboardRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting dashboards
                _widgetRepository.DeleteByTeamIdAsync(id, cancellationToken), // deleting widgets
                _accessRepository.DeleteByTeamIdAsync(id, cancellationToken)
            };

            // waiting for all results
            await Task.WhenAll(tasks);

            return await base.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string teamNamespace, CancellationToken cancellationToken = default)
        {
            if (teamNamespace.ToLower().StartsWith("momentaj") || teamNamespace.ToLower().StartsWith("ubeac"))
                return true;

            var team = await _repository.GetByNamespaceAsync(teamNamespace, cancellationToken);
            return team != null;
        }

        public async Task<Tuple<Guid, int>> GetAccessAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var headers = request.Headers;

            if (headers.ContainsKey(TeamConstants.ACCESSTOKEN))
            {
                var encodedToken = headers[TeamConstants.ACCESSTOKEN];
                var token = _encryption.Decrypt(encodedToken);
                if (!string.IsNullOrEmpty(token) && token.Length > 32)
                {
                    if (Guid.TryParse(token.Substring(0, 32), out Guid teamId))
                    {
                        var role = await _repository.HasAccessAsync(teamId, token, cancellationToken);
                        return Tuple.Create(teamId, role);
                    }
                }
            }

            return null;
        }

        public async Task<ResultSet<string>> InvokeTokenAsync(Guid teamId, AccessLevels accessLevel, CancellationToken cancellationToken = default)
        {
            var tokenString = teamId.ToString().Replace("-", "") + Helper.RandomString(224);
            var code = _encryption.Encrypt(tokenString);

            if (!string.IsNullOrEmpty(code))
            {
                var model = await _repository.GetByIdAsync(teamId, cancellationToken);
                if (model == null)
                    return new ResultSet<string>(string.Empty, ResponseCodes.BadRequest);

                model.UpdateDate = DateTime.UtcNow;
                model.Tokens.Add(new Token { AccessToken = tokenString, Role = accessLevel });

                if (await _repository.UpdateAsync(model, cancellationToken))
                    return new ResultSet<string>(code);
            }

            //if (await _repository.UpdateTokensAsync(teamId, new Token { AccessToken = tokenString, Role = accessLevel }, cancellationToken))
            //    return new ResultSet<string>(code);

            return new ResultSet<string>(string.Empty, ResponseCodes.BadRequest);
        }

        public async Task<ResultSet<bool>> RemoveTokenAsync(Guid teamId, string token, CancellationToken cancellationToken = default)
        {
            var plainToken = _encryption.Decrypt(token);

            if (!string.IsNullOrEmpty(plainToken))
            {
                var model = await _repository.GetByIdAsync(teamId, cancellationToken);
                if (model == null)
                    return new ResultSet<bool>(false, ResponseCodes.BadRequest);

                model.UpdateDate = DateTime.UtcNow;
                model.Tokens.RemoveAll(x => x.AccessToken == plainToken);

                return new ResultSet<bool>(await _repository.UpdateAsync(model, cancellationToken));
            }

            return new ResultSet<bool>(false, ResponseCodes.BadRequest);
        }
    }
}
