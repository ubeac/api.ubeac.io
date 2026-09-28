using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class MistSystemsGatewayProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            // https://www.mist.com/wp-content/uploads/mist-ap41-datasheet.pdf
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            var _data = JsonConvert.DeserializeObject<MistSystemsData>(gatewayData.Body);

            foreach (var item in _data.events)
            {
                var beaconDevice = new DeviceRawData();
                gatewayData.RawDevices.Add(beaconDevice);
                var currentDatetime = gatewayData.DateTime;

                try
                {
                    if (item.timestamp.Length > 10)
                        currentDatetime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item.timestamp)).UtcDateTime;
                    else
                        currentDatetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(item.timestamp)).UtcDateTime;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                try
                {
                    beaconDevice.Uid = FormatMacAddress(item.mac);
                    beaconDevice.DateTime = currentDatetime;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                //Calculating SignalStrength
                if (!string.IsNullOrEmpty(item.rssi))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData {
                            Type = SensorTypes.SignalStrength,
                            Prefix = SensorPrefixes.One,
                            Unit = SensorUnits.DecibelMilliwatts,
                            SensorUid = "SignalStrength",
                            Data = new Dictionary<string, decimal> { { "rssi", decimal.Parse(item.rssi) }, { "txPower", 0 } } });
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }

                //Calculating Temperature
                if (!string.IsNullOrEmpty(item.temperature))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData {
                            Type = SensorTypes.Temperature,
                            Prefix = SensorPrefixes.One,
                            Unit = SensorUnits.Centigrade,
                            SensorUid = "Temperature",
                            Data = new Dictionary<string, decimal> { { "value", decimal.Parse(item.temperature) } }
                        });
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
            }
        }
        /////////////////////////////////////////////////////////////////////
        public string FormatMacAddress(string MacAddress)
        {
            if (MacAddress.Contains(":")) return MacAddress;
            if (MacAddress.Contains("-")) return MacAddress;
            try
            {
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                var newformat = Regex.Replace(MacAddress, regex, replace);
                return newformat.ToUpper();
            }
            catch (Exception)
            {
                return MacAddress;
            }

        } //FormatMacAddress

        internal class MistSystemsData
        {
            public string topic { get; set; }
            public List<MistSystemsEventData> events { get; set; }
        }  

        internal class MistSystemsEventData
        {
            public string asset_id { get; set; }
            public string map_id { get; set; }
            public string timestamp { get; set; }
            public string ibeacon_major { get; set; }
            public string site_id { get; set; }
            public string ibeacon_uuid { get; set; }
            public string beam { get; set; }
            public string mac { get; set; }
            public string ibeacon_minor { get; set; }
            public string rssi { get; set; }
            public string device_id { get; set; }
            public string temperature { get; set; }
        } 
    } 
} 
