using System.Collections.Generic;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.WebSocket;

namespace uBeac.SocketApi
{
    public static class SocketExtensions
    {
        const string DELIMITER = "/";
        const string STAR = "*";
        const string GATEWAY_DATATYPE_PREFIX = "GatewayData";
        const string GATEWAY_TYPE_PREFIX = "Gateway";
        const string DEVICE_RAWDATATYPE_PREFIX = "DeviceRawData";
        const string DEVICE_DATATYPE_PREFIX = "DeviceData";
        const string SENSOR_DATATYPE_PREFIX = "SensorData";

        public static void PublishGateway(this List<Task> tasks, ISocket socket, GatewayData gatewayData)
        {
            var teamId = gatewayData.TeamId.ToString();
            var gatewayId = gatewayData.GatewayId.ToString();

            var gatewayGroupId = GATEWAY_TYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId;
            var gatewayStarGroupId = GATEWAY_TYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR;

            tasks.Add(socket.SendToGroupAsync(gatewayGroupId, new { Id = gatewayId, gatewayData.DateTime }));
            tasks.Add(socket.SendToGroupAsync(gatewayStarGroupId, new { Id = gatewayId, gatewayData.DateTime }));

        }

        public static void PublishGatewayData(this List<Task> tasks, ISocket socket, GatewayData gatewayData)
        {
            var teamId = gatewayData.TeamId.ToString();
            var gatewayId = gatewayData.GatewayId.ToString();

            var gatewayDataGroupId = GATEWAY_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId;
            var gatewayDataStarGroupId = GATEWAY_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR;

            tasks.Add(socket.SendToGroupAsync(gatewayDataGroupId, gatewayData));
            tasks.Add(socket.SendToGroupAsync(gatewayDataStarGroupId, gatewayData));

        }

        public static void PublishDeviceRawData(this List<Task> tasks, ISocket socket, GatewayData gatewayData)
        {
            var teamId = gatewayData.TeamId.ToString();
            var gatewayId = gatewayData.GatewayId.ToString();

            foreach (var deviceRawData in gatewayData.RawDevices)
            {
                var deviceUid = deviceRawData.Uid;

                var deviceRawDataGroupId = DEVICE_RAWDATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + deviceUid;
                tasks.Add(socket.SendToGroupAsync(deviceRawDataGroupId, deviceRawData));

                deviceRawDataGroupId = DEVICE_RAWDATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + deviceUid;
                tasks.Add(socket.SendToGroupAsync(deviceRawDataGroupId, deviceRawData));

                deviceRawDataGroupId = DEVICE_RAWDATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + STAR;
                tasks.Add(socket.SendToGroupAsync(deviceRawDataGroupId, deviceRawData));

                deviceRawDataGroupId = DEVICE_RAWDATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + STAR;
                tasks.Add(socket.SendToGroupAsync(deviceRawDataGroupId, deviceRawData));

            }

        }

        public static void PublishDeviceData(this List<Task> tasks, ISocket socket, string teamId, string gatewayId, DeviceData deviceData)
        {
            var deviceId = deviceData.Id.ToString();

            var deviceDataGroupId = DEVICE_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + deviceId;
            tasks.Add(socket.SendToGroupAsync(deviceDataGroupId, deviceData));

            deviceDataGroupId = DEVICE_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + deviceId;
            tasks.Add(socket.SendToGroupAsync(deviceDataGroupId, deviceData));

            deviceDataGroupId = DEVICE_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(deviceDataGroupId, deviceData));

            deviceDataGroupId = DEVICE_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(deviceDataGroupId, deviceData));
        }

        public static void PublishSensorData(this List<Task> tasks, ISocket socket, string teamId, string gatewayId, string deviceId, SensorData sensorData)
        {
            var sensorId = sensorData.SensorId.ToString();

            //todo: remove this lines later
            tasks.Add(socket.SendToGroupAsync("public", sensorData));

            var sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + deviceId + DELIMITER + sensorId;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + STAR + DELIMITER + sensorId;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + deviceId + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + gatewayId + DELIMITER + STAR + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + deviceId + DELIMITER + sensorId;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + STAR + DELIMITER + sensorId;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + deviceId + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

            sensorDataGroupId = SENSOR_DATATYPE_PREFIX + DELIMITER + teamId + DELIMITER + STAR + DELIMITER + STAR + DELIMITER + STAR;
            tasks.Add(socket.SendToGroupAsync(sensorDataGroupId, sensorData));

        }

    }
}
