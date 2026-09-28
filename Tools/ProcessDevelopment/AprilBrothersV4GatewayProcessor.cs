using System;
using System.Collections.Generic;
using MsgPack.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class AprilBrothersV4GatewayProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            byte[] byteArray = gatewayData.Bytes;
            var serializer = MessagePackSerializer.Get<AprilBrothersData>();
            var objAprilBortherV4 = serializer.Unpack(new MemoryStream(byteArray));
            if (objAprilBortherV4.devices != null)
            {
                foreach (var tag in objAprilBortherV4.devices)
                {
                    var beaconDevice = new DeviceRawData();
                    gatewayData.RawDevices.Add(beaconDevice);
                    var currentDateTime = gatewayData.DateTime;

                    string data = "";
                    string advertising_type;
                    string mac;
                    int rssi = 0;
                    string advertising_data = "";

                    try
                    {
                        data = String.Join("", Array.ConvertAll(tag, x => x.ToString("X2")));
                        advertising_type = data.Substring(0, 2);  // byte1 
                        mac = data.Substring(2, 12); // bytes2-7 
                        rssi = int.Parse(data.Substring(14, 2), System.Globalization.NumberStyles.HexNumber) - 256;   // byte8 minus 256
                        advertising_data = data.Substring(16); //byte9 

                        beaconDevice.Uid = FormatMacAddress(mac);
                        beaconDevice.DateTime = currentDateTime;
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }

                    /////////////////////////////////////

                    if (!string.IsNullOrEmpty(advertising_data))
                    {
                        try
                        {
                            var parsingData = ParseData(advertising_data);
                            if (parsingData.IsDataValid)
                            {
                                if (parsingData.EddyStoneList != null)
                                {
                                    foreach (var ed in parsingData.EddyStoneList)
                                    {
                                        if (ed.IsDataValid)
                                        {
                                            if (!string.IsNullOrEmpty(ed.TxPower) && (rssi != 0))
                                            {
                                                try
                                                {
                                                    beaconDevice.Sensors.Add(new SensorRawData
                                                    {
                                                        Type = SensorTypes.SignalStrength,
                                                        Unit = SensorUnits.DecibelMilliwatts,
                                                        Prefix = SensorPrefixes.One,
                                                        SensorUid = "SignalStrength",
                                                        Data = new Dictionary<string, decimal>
                                                    {
                                                        { "rssi", rssi },
                                                        { "txPower", decimal.Parse(ed.TxPower) }
                                                    }
                                                    });
                                                }
                                                catch (Exception ex)
                                                {
                                                    gatewayData.LogException(ex);
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(ed.BatteryVoltage))
                                            {
                                                try
                                                {
                                                    beaconDevice.Sensors.Add(new SensorRawData
                                                    {
                                                        Type = SensorTypes.Voltage,
                                                        Prefix = SensorPrefixes.One,
                                                        Unit = SensorUnits.Volt,
                                                        SensorUid = "Battery",
                                                        Data = new Dictionary<string, decimal>
                                                    {
                                                        { "value", decimal.Parse(ed.BatteryVoltage) }
                                                    }
                                                    });
                                                }
                                                catch (Exception ex)
                                                {
                                                    gatewayData.LogException(ex);
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(ed.Temperature))
                                            {
                                                try
                                                {
                                                    beaconDevice.Sensors.Add(new SensorRawData
                                                    {
                                                        Type = SensorTypes.Temperature,
                                                        Prefix = SensorPrefixes.One,
                                                        Unit = SensorUnits.Centigrade,
                                                        SensorUid = "Temperature",
                                                        Data = new Dictionary<string, decimal>
                                                    {
                                                        { "value", decimal.Parse(ed.Temperature) }
                                                    }
                                                    });
                                                }
                                                catch (Exception ex)
                                                {
                                                    gatewayData.LogException(ex);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parsingData.iBeaconList != null)
                                {
                                    foreach (var ib in parsingData.iBeaconList)
                                    {
                                        if (ib.IsDataValid)
                                        {
                                            if (!string.IsNullOrEmpty(ib.SignalPower) && (rssi != 0))
                                            {
                                                try
                                                {
                                                    beaconDevice.Sensors.Add(new SensorRawData
                                                    {
                                                        Type = SensorTypes.SignalStrength,
                                                        Unit = SensorUnits.DecibelMilliwatts,
                                                        Prefix = SensorPrefixes.One,
                                                        SensorUid = "SignalStrength",
                                                        Data = new Dictionary<string, decimal>
                                                    {
                                                        { "rssi", rssi },
                                                        { "txPower", decimal.Parse(ib.SignalPower) }
                                                    }
                                                    });
                                                }
                                                catch (Exception ex)
                                                {
                                                    gatewayData.LogException(ex);
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(ib.Distance))
                                            {
                                                try
                                                {
                                                    beaconDevice.Sensors.Add(new SensorRawData
                                                    {
                                                        Type = SensorTypes.Distance,
                                                        Prefix = SensorPrefixes.One,
                                                        Unit = SensorUnits.Meter,
                                                        SensorUid = "Distance",
                                                        Data = new Dictionary<string, decimal>
                                                    {
                                                        { "value", decimal.Parse(ib.Distance) }
                                                    }
                                                    });
                                                }
                                                catch (Exception ex)
                                                {
                                                    gatewayData.LogException(ex);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
            }
        }

        private string FormatMacAddress(string MacAddress)
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
                return MacAddress.ToUpper();
            }

        } //FormatMacAddress

        public class AprilBrothersData
        {
            [MessagePackMember(0)]
            public string v { get; set; }
            [MessagePackMember(1)]
            public int mid { get; set; }
            [MessagePackMember(2)]
            public int time { get; set; }
            [MessagePackMember(3)]
            public string ip { get; set; }
            [MessagePackMember(4)]
            public string mac { get; set; }
            [MessagePackMember(5)]
            public List<byte[]> devices { get; set; }
        }

        private ParsingData ParseData(string data)
        {
            var parsingData = new ParsingData();
            parsingData.IsDataValid = false;
            parsingData.iBeaconList = new List<iBeaconData>();
            parsingData.EddyStoneList = new List<EddyStoneData>();

            try
            {
                while (data.Length != 0)
                {
                    var _length = Int16.Parse(data.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var _type = data.Substring(2, 2);
                    var _data = data.Substring(4, (_length - 1) * 2);

                    if (_type == "16") //Eddystone
                    {
                        //parsingData.servcie_uuid_16 = _data.Substring(0, 4);
                        parsingData.uuid = _data.Substring(0, 4);
                        var ed = new EddyStoneData(data);
                        if (ed.IsDataValid)
                        {
                            parsingData.EddyStoneList.Add(ed);
                            parsingData.IsDataValid = true;
                        }
                    }
                    else if (_type == "FF")
                    {
                        var _sub_type = _data.Substring(4, 2);
                        if (_sub_type == "02") // iBeacon 
                        {
                            if (_length == 26)  // valid iBeacon
                            {
                                var ib = new iBeaconData(data);
                                if (ib.IsDataValid)
                                {
                                    parsingData.iBeaconList.Add(ib);
                                    parsingData.IsDataValid = true;
                                }
                            }
                            else
                            {
                                //var invalid_iBeacon_length = data;
                            }
                        }
                        else
                        {
                            //var unknown_sub_type = data;
                        }
                    }
                    else
                    {
                        //var _not_handled_type = _type;
                    }

                    data = data.Substring((_length + 1) * 2);
                }
            }
            catch (Exception ex)
            {
                var error = ex;
                parsingData.IsDataValid = false;
                // TODO: Log and handle this
            }

            return parsingData;
        }//ParsingData

        internal class ParsingData
        {

            public string uuid { get; set; }
            public List<EddyStoneData> EddyStoneList { get; set; }
            public List<iBeaconData> iBeaconList { get; set; }
            public string rssi { get; set; }

            public bool IsDataValid { get; set; }
        }

    } //Class AprilBrothersV4GatewayProcessor

} //NS
