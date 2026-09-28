using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.HttpMqttCommon.Models
{
    public class TeamsModel
    {
        private readonly Dictionary<string, TeamModel> teamByNamespace;
        private readonly Dictionary<Guid, TeamModel> teamById;
        private object locker;
        public TeamsModel()
        {
            teamByNamespace = new Dictionary<string, TeamModel>();
            teamById = new Dictionary<Guid, TeamModel>();
            locker = new object();
            Gateways = new Dictionary<Guid, Gateway>();
        }

        public Dictionary<Guid, Gateway> Gateways { get; }

        public void AddOrUpdate(Guid id, string _namespace)
        {
            lock (locker)
            {
                if (teamById.ContainsKey(id))
                {
                    var oldNamespace = teamById[id].Namespace;
                    teamById[id].Namespace = _namespace;

                    if (teamByNamespace.ContainsKey(oldNamespace))
                        teamByNamespace.Remove(oldNamespace);

                    teamByNamespace.Add(_namespace, teamById[id]);
                }
                else
                {
                    if (teamByNamespace.ContainsKey(_namespace))
                        teamByNamespace.Remove(_namespace);

                    var team = new TeamModel(id, _namespace, this);
                    teamById.Add(id, team);
                    teamByNamespace.Add(_namespace, team);
                }
            }
        }

        public void Remove(Guid id)
        {
            lock (locker)
            {
                if (teamById.ContainsKey(id))
                {
                    var _namespace = teamById[id].Namespace;
                    teamById.Remove(id);

                    if (teamByNamespace.ContainsKey(_namespace))
                        teamByNamespace.Remove(_namespace);
                }
            }
        }

        public bool TryGetByNamespace(string _namespace, out TeamModel teamModel)
        {
            lock (locker)
            {
                return teamByNamespace.TryGetValue(_namespace, out teamModel);
            }
        }

        public bool TryGetById(Guid id, out TeamModel teamModel)
        {
            lock (locker)
            {
                return teamById.TryGetValue(id, out teamModel);
            }
        }

        public Gateway FindGateway(Guid gatewayId)
        {
            return Gateways[gatewayId];
        }

    }

    public class TeamModel
    {
        public TeamsModel Teams { get; }
        public Guid Id { get; }
        public string Namespace { get; set; }
        public GatewaysModel Gateways { get; }

        public TeamModel(Guid id, string _namespace, TeamsModel teams)
        {
            Id = id;
            Namespace = _namespace;
            Gateways = new GatewaysModel(this);
            Teams = teams;
        }
    }

    public class GatewaysModel
    {
        public TeamModel Team { get; }
        private readonly Dictionary<Guid, Gateway> gatewayById;
        private readonly Dictionary<string, Gateway> gatewayByUrl;
        private object locker;

        public GatewaysModel(TeamModel team)
        {
            Team = team;
            gatewayById = new Dictionary<Guid, Gateway>();
            gatewayByUrl = new Dictionary<string, Gateway>();
            locker = new object();
        }

        public bool TryGetByUrl(string gatewayUrl, out Gateway gateway)
        {
            lock (locker)
            {
                return gatewayByUrl.TryGetValue(gatewayUrl, out gateway);
            }
        }

        public void AddOrUpdate(Guid id, Gateway gateway)
        {
            lock (locker)
            {
                if (gatewayById.ContainsKey(id))
                {
                    var oldUrl = gatewayById[id].Url;
                    gatewayById[id] = gateway;

                    if (!Team.Teams.Gateways.ContainsKey(id))
                        Team.Teams.Gateways.Add(id, gateway);

                    if (gatewayByUrl.ContainsKey(oldUrl))
                        gatewayByUrl.Remove(oldUrl);

                    gatewayByUrl.Add(gateway.Url, gateway);

                }
                else
                {
                    if (gatewayByUrl.ContainsKey(gateway.Url))
                        gatewayByUrl.Remove(gateway.Url);

                    if (Team.Teams.Gateways.ContainsKey(id))
                        Team.Teams.Gateways.Remove(id);

                    gatewayById.Add(id, gateway);
                    gatewayByUrl.Add(gateway.Url, gateway);
                    Team.Teams.Gateways.Add(id, gateway);
                }
            }
        }

        public void Remove(Guid id)
        {
            lock (locker)
            {
                if (gatewayById.ContainsKey(id))
                {
                    var url = gatewayById[id].Url;
                    gatewayByUrl.Remove(url);

                    if (Team.Teams.Gateways.ContainsKey(id))
                        Team.Teams.Gateways.Remove(id);

                    gatewayById.Remove(id);
                }
            }
        }

    }
}
