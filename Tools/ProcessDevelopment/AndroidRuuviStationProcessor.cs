using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class AndroidRuuviStationProcessor : IProcessor
    {
        private decimal? GetSensorDecimalValue(decimal? value)
        {
            if (value.HasValue)
                return value.Value;

            return null;
        }

        private Dictionary<string, decimal> GetSensorDecimalTripleValues(string sensorName, decimal? x, decimal? y, decimal? z, GatewayData gatewayData)
        {
            Dictionary<string, decimal> values = new Dictionary<string, decimal>();

            try
            {
                if (!x.HasValue || !y.HasValue)
                    return values;

                if (sensorName == "SignalStrength")
                {
                    values.Add("rssi", x.Value);
                    values.Add("txPower", y.Value);
                }
                else if (sensorName == "Location")
                {
                    values.Add("longitude", x.Value);
                    values.Add("latitude", y.Value);
                    //if (z.HasValue)
                    //    values.Add("altitude", z.Value);
                }
                else
                {
                    // accelerometer
                    if (!z.HasValue)
                        return values;
                    values.Add("x", x.Value);
                    values.Add("y", y.Value);
                    values.Add("z", z.Value);
                }

                return values;
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }

            return null;
        }

        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            // Ruuvi Station Andriod App --> https://play.google.com/store/apps/details?id=com.ruuvi.station
            // Ruvvi DataFormat Explanation --> https://github.com/ruuvi/ruuvi-sensor-protocols
            ////////////////////////////////////////////////////////////////////////
            var _devices = new List<AndriodRuuviStationDeviceData>();
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                var _data = JsonConvert.DeserializeObject<AndriodRuuviStationData>(gatewayData.Body);

                if (DateTime.TryParse(_data.time, out DateTime currentDateTime))
                    currentDateTime = currentDateTime.ToUniversalTime();
                else
                    currentDateTime = DateTime.UtcNow;

                if (_data.tags != null) { _devices = _data.tags; }
                if (_data.tag != null) { _devices.Add(_data.tag); }

                foreach (var device in _devices)
                {
                    var beaconDevice = new DeviceRawData();
                    beaconDevice.GatewayId = gatewayData.GatewayId;
                    beaconDevice.IsValid = true;
                    gatewayData.RawDevices.Add(beaconDevice);
                    beaconDevice.Uid = device.id;
                    beaconDevice.DateTime = currentDateTime;

                    /////////////////////////////////////
                    if (device.rssi.HasValue && device.txPower.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("SignalStrength", currentDateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, GetSensorDecimalTripleValues("SignalStrength", device.rssi, device.txPower, null, gatewayData)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.humidity.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Humidity", currentDateTime, SensorTypes.Humidity, SensorUnits.AbsoluteHumidity, SensorPrefixes.One, GetSensorDecimalValue(device.humidity).Value));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.pressure.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Pressure", currentDateTime, SensorTypes.Pressure, SensorUnits.Pascal, SensorPrefixes.Hundred, GetSensorDecimalValue(device.pressure).Value));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.temperature.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Temperature", currentDateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, GetSensorDecimalValue(device.temperature).Value));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.voltage.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Voltage", currentDateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, GetSensorDecimalValue(device.voltage).Value));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.accelX.HasValue && device.accelY.HasValue && device.accelZ.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Acceleration", currentDateTime, SensorTypes.Acceleration, SensorUnits.GravitationalForce, SensorPrefixes.One, GetSensorDecimalTripleValues("Acceleration", device.accelX, device.accelY, device.accelZ, gatewayData)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (device.movementCounter.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Counter", currentDateTime, SensorTypes.Counter, SensorUnits.Count, SensorPrefixes.One, GetSensorDecimalValue(device.voltage).Value));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                    /////////////////////////////////////
                    if (_data.location.longitude.HasValue && _data.location.latitude.HasValue)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Location", currentDateTime, SensorTypes.Location, SensorUnits.Degree, SensorPrefixes.One, GetSensorDecimalTripleValues("Location", _data.location.longitude, _data.location.latitude, _data.location.accuracy, gatewayData)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }
        }

        internal class AndriodRuuviStationData
        {
            public string deviceId { get; set; }
            public string eventId { get; set; }
            public AndriodRuuviStationDeviceData tag { get; set; }
            public List<AndriodRuuviStationDeviceData> tags { get; set; }
            public string time { get; set; }
            public Location location { get; set; }
        }

        internal class AndriodRuuviStationDeviceData
        {
            public decimal? accelX { get; set; }
            public decimal? accelY { get; set; }
            public decimal? accelZ { get; set; }
            public int? dataFormat { get; set; }
            public int? defaultBackground { get; set; }
            public bool favorite { get; set; }
            public string gatewayUrl { get; set; }
            public decimal? humidity { get; set; }
            public string id { get; set; }
            public string measurementSequenceNumber { get; set; }
            public decimal? movementCounter { get; set; }
            public decimal? pressure { get; set; }
            public object rawDataBlob { get; set; }
            public decimal? rssi { get; set; }
            public decimal? temperature { get; set; }
            public decimal? txPower { get; set; }
            public string updateAt { get; set; }
            public string url { get; set; }
            public decimal? voltage { get; set; }
        }

        internal class Location
        {
            public decimal? accuracy { get; set; }
            public decimal? latitude { get; set; }
            public decimal? longitude { get; set; }
        }

    }
}
