using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using uBeac.Models;
using System.Globalization;

/// <summary>
/// This is processor for: Android Data Collectore APP 
/// https://play.google.com/store/apps/details?id=tech.unismart.dc 
/// </summary>
/// 
namespace uBeac.IoT.Processing
{
    public class BlueCatEdgeRelayProcessor : IProcessor
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
            // Processor section --> https://www.bluecats.com/knowledgebase/bluecats-edge-applications-overview/
            ////////////////////////////////////////////////////////////////////////
            var beaconsData = new BlueCatData();
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                beaconsData = JsonConvert.DeserializeObject<BlueCatData>(gatewayData.Body);
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }
            // adding gateway activity information
            var devicelist = new List<BlueCatEvent>();
            if (beaconsData.devices.Count > 0)
                devicelist = beaconsData.devices;
            else
                devicelist = beaconsData.events;

            foreach (var beacon in devicelist)
            {
                var beaconDevice = new DeviceRawData();
                gatewayData.RawDevices.Add(beaconDevice);
                var currentDatetime = DateTime.Parse(beacon.ts).ToUniversalTime();

                try
                {
                    beaconDevice.Uid = FormatMacAddress(beacon.mac);
                    beaconDevice.DateTime = currentDatetime;
                    beaconDevice.GatewayId = gatewayData.GatewayId;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }
                ////////////////////////////////////////////////////////////////// rssi 
                if (beacon.rssi != null && beacon.mPow != null)
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("SignalStrength", currentDatetime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One,
                            GetSensorDecimalTripleValues("SignalStrength", ToDecimal(beacon.rssi), ToDecimal(beacon.mPow), null, gatewayData)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
                ///////////////////////////////////////////////////////////////// temp
                if (beacon.temp != null)
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Temperature", currentDatetime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, ToDecimal(beacon.temp).Value));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
                ///////////////////////////////////////////////////////////////// location
                if (beacon.gps != null)
                {
                    if (beacon.gps.ContainsKey("long") && beacon.gps.ContainsKey("lat"))
                    {
                        decimal? alt = null;
                        if (beacon.gps.ContainsKey("alt"))
                            alt = ToDecimal(beacon.gps["alt"]);
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Location", currentDatetime, SensorTypes.Location, SensorUnits.Degree, SensorPrefixes.One,
                                GetSensorDecimalTripleValues("Location", ToDecimal(beacon.gps["long"]), ToDecimal(beacon.gps["lat"]), alt, gatewayData)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
            }
        }

        public string FormatMacAddress(string MacAddress)
        {
            if (MacAddress.Contains(":")) return MacAddress;
            if (MacAddress.Contains("-")) return MacAddress;
            try
            {
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                var newformat = Regex.Replace(MacAddress, regex, replace);
                return newformat;
            }
            catch (Exception)
            {
                return MacAddress;
            }
        }

        private decimal? ToDecimal(string value)
        {
            try
            {
                return decimal.Parse(value.Trim(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class BlueCatData
    {
        public string jsonVer { get; set; }
        public string edgeMAC { get; set; }
        public object gps { get; set; }  // unknow data as I have it not populated 
        public List<BlueCatEvent> devices { get; set; }
        public List<BlueCatEvent> events { get; set; }
        public BlueCatData()
        {
            devices = new List<BlueCatEvent>();
        }
    }

    public class BlueCatEvent
    {
        public string mac { get; set; }
        public string mPow { get; set; }
        public string rssi { get; set; }
        public string rssiSmooth { get; set; }
        public string Event { get; set; }
        public string ts { get; set; }
        public string temp { get; set; }
        public Dictionary<string, string> gps { get; set; }
    }
}






