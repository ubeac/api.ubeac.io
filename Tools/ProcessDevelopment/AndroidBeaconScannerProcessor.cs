///// <summary>
///// This is processor for: Android Beacon Scanner
///// https://github.com/Bridouille/android-beacon-scanner
///// </summary>
///// 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class AndroidBeaconScannerProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            var beaconsData = JsonConvert.DeserializeObject<AndroidBeaconScannergatewayData>(gatewayData.Body);

            if (beaconsData is null || beaconsData.Beacons is null)
                return;

            // adding product activity information
            foreach (var beacon in beaconsData.Beacons)
            {
                var currentDatetime = DateTimeOffset.FromUnixTimeMilliseconds((long)beacon.LastSeen).UtcDateTime;

                var beaconDevice = new DeviceRawData
                {
                    Uid = beacon.BeaconAddress,
                    DateTime = currentDatetime,
                    GatewayId = gatewayData.GatewayId,
                    IsValid = false
                };
                gatewayData.RawDevices.Add(beaconDevice);

                // Adding beacon sensor including RSSI and TxPower
                try
                {
                    var data = new Dictionary<string, decimal> { { "rssi", beacon.Rssi }, { "txPower", beacon.TxPower } };
                    beaconDevice.Sensors.Add(new SensorRawData("SignalStrength", currentDatetime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, data));
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                // Adding distance sensor 
                try
                {
                    beaconDevice.Sensors.Add(new SensorRawData("Distance", currentDatetime, SensorTypes.Distance, SensorUnits.Meter, SensorPrefixes.One, beacon.Distance));
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                // Adding Temperature sensor 
                try
                {
                    if (beacon.TelemetryData != null && beacon.TelemetryData.Temperature.HasValue)
                        beaconDevice.Sensors.Add(new SensorRawData("Temperature", currentDatetime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, beacon.TelemetryData.Temperature.Value));
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                // Adding Voltage sensor 
                try
                {
                    if (beacon.TelemetryData != null && beacon.TelemetryData.BatteryMilliVolts.HasValue)
                        beaconDevice.Sensors.Add(new SensorRawData("Battery", currentDatetime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, beacon.TelemetryData.BatteryMilliVolts.Value / 1000));
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

            }
        }
    }

    public class AndroidBeaconScannergatewayData
    {
        public string Reader { get; set; }
        public List<AndroidBeaconScannerBeaconData> Beacons { get; set; }
        public AndroidBeaconScannergatewayData()
        {
            Beacons = new List<AndroidBeaconScannerBeaconData>();
        }
    }

    public class AndroidBeaconScannerBeaconData
    {
        public string BeaconAddress { get; set; }
        public string BeaconType { get; set; }
        public decimal Distance { get; set; }
        public long Hashcode { get; set; }
        public bool IsBlocked { get; set; }
        public ulong LastMinuteSeen { get; set; }
        public ulong LastSeen { get; set; }
        public int Manufacturer { get; set; }
        public int Rssi { get; set; }
        public int TxPower { get; set; }
        public string ReaderName { get; set; }
        public AndroidBeaconScannerTelemetryData TelemetryData { get; set; }
    }

    public class AndroidBeaconScannerTelemetryData
    {
        public uint? BatteryMilliVolts { get; set; }
        public uint? PDUCount { get; set; }
        public decimal? Temperature { get; set; }
        public ulong? UptimeSeconds { get; set; }
        public int? Version { get; set; }
    }
}