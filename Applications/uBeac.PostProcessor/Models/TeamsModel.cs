using System;
using System.Collections.Generic;
using System.Linq;

namespace uBeac.PostProcessor.Models
{
    public class TeamsModel
    {
        private readonly Dictionary<Guid, DevicesModel> devicesModelByTeamId;
        private Dictionary<Guid, DeviceModel> Devices { get; } // key: deviceId
        private Dictionary<Guid, SensorModel> Sensors { get; }// key: sensorId

        private object locker;
        public TeamsModel()
        {
            devicesModelByTeamId = new Dictionary<Guid, DevicesModel>();
            locker = new object();
            Devices = new Dictionary<Guid, DeviceModel>();
            Sensors = new Dictionary<Guid, SensorModel>();
        }

        public void Remove(Guid teamId)
        {
            lock (locker)
            {
                if (!devicesModelByTeamId.ContainsKey(teamId))
                    return;

                devicesModelByTeamId.Remove(teamId, out DevicesModel devicesModel);
                var deviceIds = devicesModel.GetDeviceIds();
                foreach (var id in deviceIds)
                {
                    Devices.Remove(id);
                }

                var sensorIds = devicesModel.GetSensorIds();
                foreach (var id in sensorIds)
                {
                    Sensors.Remove(id);
                }
            }
        }

        public void Add(Guid teamId)
        {
            devicesModelByTeamId.Add(teamId, new DevicesModel());
        }

        public bool TryGetById(Guid id, out DevicesModel deviceModel)
        {
            return devicesModelByTeamId.TryGetValue(id, out deviceModel);
        }

        public void RemoveDevice(Guid deviceId)
        {
            lock (locker)
            {
                if (!Devices.ContainsKey(deviceId))
                    return;

                var device = Devices[deviceId];

                if (!devicesModelByTeamId.ContainsKey(device.TeamId))
                {
                    Devices.Remove(deviceId);
                    return;
                }

                if (devicesModelByTeamId[device.TeamId].TryGetByUid(device.Uid, out DeviceModel deviceModel))
                {
                    foreach (var sensorId in deviceModel.SensorsModel.GetSensorIds())
                    {
                        Sensors.Remove(sensorId);
                    }

                    devicesModelByTeamId[device.TeamId].Remove(device.Uid);
                }
                Devices.Remove(deviceId);
            }
        }

        public bool TryAddDevice(Guid teamId, Guid id, string uid)
        {
            lock (locker)
            {
                if (Devices.ContainsKey(id))
                    return false;

                if (!devicesModelByTeamId.ContainsKey(teamId))
                    return false;


                var deviceModel = new DeviceModel(teamId, id, uid);
                devicesModelByTeamId[teamId].Add(deviceModel);
                Devices.Add(id, deviceModel);

                return true;
            }
        }

        public bool TryAddSensor(Guid teamId, Guid deviceId, Guid sensorId, string sensorUid, bool persist, HashSet<string> schema)
        {
            lock (locker)
            {
                if (Sensors.ContainsKey(sensorId))
                    return false;

                if (!devicesModelByTeamId.ContainsKey(teamId))
                    return false;

                if (!Devices.ContainsKey(deviceId))
                    return false;

                var tempDeviceModel = Devices[deviceId];

                if (devicesModelByTeamId[teamId].TryGetByUid(tempDeviceModel.Uid, out DeviceModel deviceModel))
                {
                    if (deviceModel.SensorsModel.TryGetByUid(sensorUid, out SensorModel existingSensorModel))
                        return false;

                    var sensorModel = new SensorModel(teamId, deviceModel.Id, sensorId, sensorUid, persist, schema);
                    
                    deviceModel.SensorsModel.Add(sensorModel);
                    Sensors.Add(sensorId, sensorModel);

                    return true;
                }

                return false;
            }
        }

        public bool TryUpdateSensor(Guid id, bool persist, HashSet<string> schema)
        {
            lock (locker)
            {
                if (!Sensors.ContainsKey(id))
                    return false;

                var tempModel = Sensors[id];
                var deviceModel = Devices[tempModel.DeviceId];

                if (devicesModelByTeamId[tempModel.TeamId].TryGetByUid(deviceModel.Uid, out deviceModel))
                {
                    if (deviceModel.SensorsModel.TryGetByUid(tempModel.Uid, out SensorModel sensorModel))
                    {
                        sensorModel.Update(persist, schema);
                        return true;
                    }
                }

                return false;
            }
        }

        public void RemoveSensor(Guid sensorId)
        {
            lock (locker)
            {
                if (!Sensors.ContainsKey(sensorId))
                    return;

                var tempModel = Sensors[sensorId];

                if (!Devices.ContainsKey(tempModel.DeviceId))
                {
                    Sensors.Remove(sensorId);
                    return;
                }

                var tempDeviceModel = Devices[tempModel.DeviceId];
                if (devicesModelByTeamId[tempModel.TeamId].TryGetByUid(tempDeviceModel.Uid, out DeviceModel deviceModel))
                    deviceModel.SensorsModel.Remove(tempModel.Uid);

                Sensors.Remove(sensorId);
            }
        }

        public bool TryGetDeviceByUid(Guid teamId, string deviceUid, out DeviceModel deviceModel)
        {
            if (!devicesModelByTeamId.ContainsKey(teamId))
            {
                deviceModel = null;
                return false;
            }

            return devicesModelByTeamId[teamId].TryGetByUid(deviceUid, out deviceModel);
        }

        public bool TryGetSensorByUid(Guid teamId, string deviceUid, string sensorUid, out SensorModel sensorModel)
        {
            if (devicesModelByTeamId[teamId].TryGetByUid(deviceUid, out DeviceModel deviceModel))
                return deviceModel.SensorsModel.TryGetByUid(sensorUid, out sensorModel);

            sensorModel = null;
            return false;
        }

        public bool TryGetSensorById(Guid teamId, string deviceUid, Guid sensorId, out SensorModel sensorModel)
        {
            if(Sensors.ContainsKey(sensorId) && devicesModelByTeamId[teamId].TryGetByUid(deviceUid, out DeviceModel deviceModel))
                return deviceModel.SensorsModel.TryGetByUid(Sensors[sensorId].Uid, out sensorModel);

            sensorModel = null;
            return false;
        }

    }

    public class DevicesModel
    {
        private Dictionary<string, DeviceModel> _deviceModelByUid;
        private object locker;

        public DevicesModel()
        {
            locker = new object();
            _deviceModelByUid = new Dictionary<string, DeviceModel>();
        }
        public void Add(DeviceModel deviceModel)
        {
            lock (locker)
            {
                if (!_deviceModelByUid.ContainsKey(deviceModel.Uid))
                    _deviceModelByUid.TryAdd(deviceModel.Uid, deviceModel);
            }
        }
        public void Remove(string deviceUid)
        {
            if (_deviceModelByUid.ContainsKey(deviceUid))
                _deviceModelByUid.Remove(deviceUid);
        }

        public bool TryGetByUid(string deviceUid, out DeviceModel deviceModel)
        {
            lock (locker)
            {
                if (_deviceModelByUid.ContainsKey(deviceUid))
                {
                    deviceModel = _deviceModelByUid[deviceUid];
                    return true;
                }
            }
            deviceModel = null;
            return false;
        }

        public IEnumerable<Guid> GetDeviceIds()
        {
            return _deviceModelByUid.Values.Select(x => x.Id);
        }

        public IEnumerable<Guid> GetSensorIds()
        {
            lock (locker)
            {
                var sensorIds = new List<Guid>();
                foreach (var deviceModel in _deviceModelByUid.Values)
                {
                    sensorIds.AddRange(deviceModel.SensorsModel.GetSensorIds());
                }

                return sensorIds;
            }
        }
    }

    public class DeviceModel
    {
        public Guid TeamId { get; set; }
        public Guid Id { get; }
        public string Uid { get; }
        public SensorsModel SensorsModel { get; }

        public DeviceModel(Guid teamId, Guid id, string uid)
        {
            TeamId = teamId;
            Id = id;
            Uid = uid;
            SensorsModel = new SensorsModel();
        }

    }

    public class SensorsModel
    {
        private Dictionary<string, SensorModel> _sensorsByUid;

        public SensorsModel()
        {
            _sensorsByUid = new Dictionary<string, SensorModel>();
        }
        public void Add(SensorModel sensorModel)
        {
            _sensorsByUid.Add(sensorModel.Uid, sensorModel);
        }
        public void Remove(string sensorUid)
        {
            _sensorsByUid.Remove(sensorUid);
        }

        public bool TryGetByUid(string sensorUid, out SensorModel sensorModel)
        {
            return _sensorsByUid.TryGetValue(sensorUid, out sensorModel);
        }

        public IEnumerable<Guid> GetSensorIds()
        {
            return _sensorsByUid.Values.Select(x => x.Id);
        }
    }

    public class SensorModel
    {
        public Guid Id { get; }
        public string Uid { get; }
        public Guid TeamId { get; }
        public Guid DeviceId { get; }
        public bool Persist { get; set; }
        public HashSet<string> Schema { get; set; }

        public SensorModel(Guid teamId, Guid deviceId, Guid sensorId, string sensorUid, bool persist, HashSet<string> schema)
        {
            Id = sensorId;
            Uid = sensorUid;
            Persist = persist;
            TeamId = teamId;
            DeviceId = deviceId;
            Schema = schema;
        }

        public void Update(bool persist, HashSet<string> keys)
        {
            Schema = keys;
            Persist = persist;
        }
    }

}
