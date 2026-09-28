///// <summary>
///// This is processor for: IOS SensorPhone
///// https://blogs.sap.com/2016/03/28/stream-your-iphones-sensor-data-to-hana-cloud-platform/
///// </summary>
///// 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class IosSensorPhoneProcessor : IProcessor
    {
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
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                var devicesData = JsonConvert.DeserializeObject<IosSensorPhoneProcessorgatewayData>(gatewayData.Body);

                if (devicesData is null || devicesData.Messages is null)
                    return;

                // adding product activity information
                foreach (var message in devicesData.Messages)
                {
                    var currentDatetime = DateTime.UtcNow;

                    if (message.Timestamp != null)
                    {
                        currentDatetime = DateTimeOffset.FromUnixTimeSeconds((long)message.Timestamp).UtcDateTime;
                    }

                    var beaconDevice = new DeviceRawData
                    {
                        Uid = message.Device,
                        DateTime = currentDatetime,
                        GatewayId = gatewayData.GatewayId,
                        IsValid = true
                    };
                    gatewayData.RawDevices.Add(beaconDevice);

                    // Adding Acceleration sensor 
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Acceleration", currentDatetime, SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared, SensorPrefixes.One,
                        GetSensorDecimalTripleValues("Acceleration", message.Accelerometerx.GetValueOrDefault(), message.Accelerometery.GetValueOrDefault(), message.Accelerometerz.GetValueOrDefault(), gatewayData)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }

                    // Adding Gyroscope sensor 
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Gyroscope", currentDatetime, SensorTypes.Gyroscope, SensorUnits.DegreePerSecond, SensorPrefixes.One,
                        GetSensorDecimalTripleValues("Gyroscope", message.Gyroscopex, message.Gyroscopey, message.Gyroscopez, gatewayData)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }

                    // Adding Sound sensor 
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Sound", currentDatetime, SensorTypes.Sound, SensorUnits.Decibel, SensorPrefixes.One, message.Audio.GetValueOrDefault()));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }

                    // Adding Location sensor 
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Location", currentDatetime, SensorTypes.Location, SensorUnits.Degree, SensorPrefixes.One,
                        GetSensorDecimalTripleValues("Location", message.Longitude.GetValueOrDefault(), message.Latitude.GetValueOrDefault(), message.Altitude.GetValueOrDefault(), gatewayData)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
            }
            catch (Exception)
            {
                gatewayData.LogException("Error in parsing json data");
            }

        }
    }

    public class IosSensorPhoneProcessorgatewayData
    {
        public string Mode { get; set; }
        public string MessageType { get; set; }
        public List<IosSensorPhoneProcessorMessage> Messages { get; set; }
    }

    public class IosSensorPhoneProcessorMessage
    {
        public decimal? Gyroscopex { get; set; }
        public decimal? Gyroscopey { get; set; }
        public decimal? Gyroscopez { get; set; }
        public decimal? Accelerometerx { get; set; }
        public decimal? Accelerometery { get; set; }
        public decimal? Accelerometerz { get; set; }
        public long? Timestamp { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Altitude { get; set; }
        public string Device { get; set; }
        public decimal? Audio { get; set; }
    }
}