using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class IngicsIGS01BleWifiProcessor : IProcessor
    {
        #region main block 

        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section --> BLE wifi gateway igs01 
            // https://fccid.io/2AH2IIGS01/User-Manual/Users-Manual-2981726
            // https://github.com/google/eddystone/tree/master/eddystone-uid
            // company-identifiers        --> https://www.bluetooth.com/specifications/assigned-numbers/company-identifiers
            // 16-bit-uuids-for-members   --> https://www.bluetooth.com/specifications/assigned-numbers/16-bit-uuids-for-members
            // record types               --> https://www.bluetooth.com/specifications/assigned-numbers/generic-access-profile
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            var stringContent = gatewayData.Body;
            var splittedLines = stringContent.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var errors = new List<string>();

            foreach (var line in splittedLines)
            {
                var currentDatetime = gatewayData.DateTime;
                var beaconDevice = new DeviceRawData();
                gatewayData.RawDevices.Add(beaconDevice);

                var report_type = "";
                var tag_id = "";
                var gateway_id = "";
                var rssi = "";
                var packet = "";

                try
                {
                    var lineItems = line.Split(',', StringSplitOptions.None);
                    if (lineItems.Length != 5)
                    {
                        errors.Add("invalid number of components per line ->" + line);
                        // todo: This case need to be handeled and logged 
                    }

                    report_type = lineItems[0];
                    tag_id = lineItems[1];
                    gateway_id = lineItems[2];
                    rssi = lineItems[3];
                    packet = lineItems[4];

                    beaconDevice.Uid = FormatMacAddress(tag_id);
                    beaconDevice.DateTime = currentDatetime;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                var _packet = ParseLineItem(packet);
                if (_packet == null)
                {
                    errors.Add("error in parsing line -> " + line);
                    continue;
                }

                _packet.Minew = new MinewData(packet);
                _packet.Ruuvi = new RuuviData(packet);
                ////////////////////////////////////////////////////////////////// rssi 

                if (!string.IsNullOrEmpty(rssi))
                {
                    try
                    {
                        var beaconData = new SensorRawData
                        {
                            Type = SensorTypes.SignalStrength,
                            Unit = SensorUnits.DecibelMilliwatts,
                            Prefix = SensorPrefixes.One,
                            SensorUid = "SignalStrength",
                            Data = { { "rssi", decimal.Parse(rssi) } }
                        };

                        if (_packet.EddyStone != null)
                        {
                            if (_packet.EddyStone.TxPower != null)
                            {
                                try
                                {
                                    beaconData.Data.Add("txPower", decimal.Parse(_packet.EddyStone.TxPower));
                                }
                                catch (Exception ex)
                                {
                                    gatewayData.LogException(ex);
                                }
                            }
                        }
                        else if (_packet.iBeacon != null)
                        {
                            if (_packet.iBeacon.SignalPower != null)
                            {
                                try
                                {
                                    beaconData.Data.Add("txPower", decimal.Parse(_packet.iBeacon.SignalPower));
                                }
                                catch (Exception ex)
                                {
                                    gatewayData.LogException(ex);
                                }
                            }
                        }
                        beaconDevice.Sensors.Add(beaconData);
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
                //////////////////////////////////////////////////////////////////  

                _packet.rssi = rssi;
                _packet.tag_id = tag_id;
                _packet.gateway_id = gateway_id;

                if (_packet.EddyStone != null)
                {
                    if (_packet.EddyStone.BatteryVoltage != null)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData { Type = SensorTypes.Voltage, Prefix = SensorPrefixes.One, Unit = SensorUnits.Volt, SensorUid = "Battery", Data = { { "value", decimal.Parse(_packet.EddyStone.BatteryVoltage) } } });
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }

                    if (_packet.EddyStone.Temperature != null)
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData { Type = SensorTypes.Temperature, Prefix = SensorPrefixes.One, Unit = SensorUnits.Centigrade, SensorUid = "Temperature", Data = { { "value", decimal.Parse(_packet.EddyStone.Temperature) } } });
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
                else
                {
                    var unhandled_packet_type = packet;
                    errors.Add("unhandled packet type (not Eddy or iB) -> " + packet);
                    // todo : log and handle this  
                }

                /////////////////////////// Minew 

                if (_packet.Minew != null)
                {
                    if (_packet.Minew.IsDataValid)
                    {
                        if (_packet.Minew.BatteryLevel != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData { Type = SensorTypes.Voltage, Prefix = SensorPrefixes.One, Unit = SensorUnits.Volt, SensorUid = "Battery", Data = { { "value", decimal.Parse(_packet.Minew.BatteryLevel) } } });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }
                        if (_packet.Minew.AccelerationX != null && _packet.Minew.AccelerationY != null && _packet.Minew.AccelerationZ != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData
                                {
                                    Type = SensorTypes.Acceleration,
                                    Prefix = SensorPrefixes.One,
                                    SensorUid = "Acceleration",
                                    Data = { { "x", decimal.Parse(_packet.Minew.AccelerationX) },
                                        { "y", decimal.Parse(_packet.Minew.AccelerationY) },
                                        { "z", decimal.Parse(_packet.Minew.AccelerationZ) } }
                                });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }
                    }
                }
                /////////////////////////// Ruuvi 

                if (_packet.Ruuvi != null)
                {
                    if (_packet.Ruuvi.IsDataValid)
                    {
                        if (_packet.Ruuvi.BatteryVoltage != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData
                                {
                                    Type = SensorTypes.Voltage,
                                    Prefix = SensorPrefixes.One,
                                    Unit = SensorUnits.Volt,
                                    SensorUid = "Battery",
                                    Data = { { "value", decimal.Parse(_packet.Ruuvi.BatteryVoltage) } }
                                });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }

                        if (_packet.Ruuvi.AccelerationX != null && _packet.Ruuvi.AccelerationY != null && _packet.Ruuvi.AccelerationZ != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData
                                {
                                    Type = SensorTypes.Acceleration,
                                    Prefix = SensorPrefixes.One,
                                    Unit = SensorUnits.MeterPerSecondSquared,
                                    SensorUid = "Acceleration",
                                    Data = { { "x", decimal.Parse(_packet.Ruuvi.AccelerationX) },
                                        { "y", decimal.Parse(_packet.Ruuvi.AccelerationY) },
                                        { "z", decimal.Parse(_packet.Ruuvi.AccelerationZ) } }
                                });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }

                        if (_packet.Ruuvi.Pressure != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData
                                {
                                    Type = SensorTypes.Pressure,
                                    Prefix = SensorPrefixes.Hundred,
                                    Unit = SensorUnits.Pascal,
                                    SensorUid = "Pressure",
                                    Data = { { "value", decimal.Parse(_packet.Ruuvi.Pressure) } }
                                });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }

                        if (_packet.Ruuvi.Temperature != null)
                        {
                            try
                            {
                                beaconDevice.Sensors.Add(new SensorRawData
                                {
                                    Type = SensorTypes.Temperature,
                                    Prefix = SensorPrefixes.One,
                                    Unit = SensorUnits.Centigrade,
                                    SensorUid = "Temperature",
                                    Data = { { "value", decimal.Parse(_packet.Ruuvi.Temperature) } }
                                });
                            }
                            catch (Exception ex)
                            {
                                gatewayData.LogException(ex);
                            }
                        }
                    }
                }

                //if (_packet.EddyStone != null || _packet.iBeacon != null)
                //{
                //    //success_list.Add(_packet);
                //}                
            }
            gatewayData.RawDevices = DeduplicateDevicesAndSensors(gatewayData.RawDevices);
        }
        ////////////////////////////////////////////////////
        public IngicsParsingClass ParseLineItem(string LineString)
        {
            IngicsParsingClass _packet = new IngicsParsingClass();

            try
            {
                while (LineString.Length != 0)

                {
                    var _length = Int16.Parse(LineString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var _type = LineString.Substring(2, 2);
                    var _data = LineString.Substring(4, (_length - 1) * 2);

                    if (_type == "01") { _packet.flags_01 = _data; }
                    else if (_type == "02") { _packet.uuid16_02 = _data; _packet.uuid = _data; }
                    else if (_type == "03") { _packet.uuid16_03 = _data; _packet.uuid = _data; }
                    else if (_type == "05") { _packet.uuid32_05 = _data; _packet.uuid = _data; }
                    else if (_type == "07") { _packet.uuid128_07 = _data; _packet.uuid = _data; }
                    else if (_type == "08") { _packet.shortened_local_name_08 = _data; _packet.local_name = _data; }
                    else if (_type == "09") { _packet.complete_local_name_09 = _data; _packet.local_name = _data; }
                    else if (_type == "16") //Eddystone
                    {
                        _packet.servcie_uuid_16 = _data.Substring(0, 4);
                        _packet.uuid = _data.Substring(0, 4);
                        _packet.EddyStone = new EddyStoneData(LineString); //ParseEddyStoneString(LineString);
                    }
                    else if (_type == "19") { _packet.appearance_19 = _data; _packet.uuid = _data; }
                    else if (_type == "FF")
                    {
                        var _sub_type = _data.Substring(4, 2);
                        if (_sub_type == "02") // iBeacon 
                        {
                            if (_length == 26)  // valid iBeacon
                            {
                                _packet.iBeacon = new iBeaconData(LineString); //ParseIbeaconString(LineString);
                            }
                            else
                            {
                                var invalid_iBeacon_length = LineString;
                                // todo: Log and handle this
                            }
                        }
                        else
                        {
                            var unknown_sub_type = LineString;
                            // todo: Log and handle this  
                        }
                    }
                    else
                    {
                        var _not_handled_type = _type;
                    }

                    LineString = LineString.Substring((_length + 1) * 2);
                }
                return _packet;
            }
            catch (Exception ex)
            {
                var error = ex;
                // todo: Log and handle this
                return null;
            }
        }//IngicsParsingClass

        ////////////////////////////////////////////////////////////////////

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

        } //FormatMacAddress

        ////////////////////////////////////////////////////////////////////

        public class IngicsParsingClass
        {
            public string tag_id { get; set; }
            public string gateway_id { get; set; }
            public string flags_01 { get; set; }
            public string uuid { get; set; }
            public string uuid16_02 { get; set; }
            public string uuid16_03 { get; set; }
            public string uuid32_05 { get; set; }
            public string uuid128_07 { get; set; }
            public string local_name { get; set; }
            public string shortened_local_name_08 { get; set; }
            public string complete_local_name_09 { get; set; }
            public string servcie_uuid_16 { get; set; }
            public string appearance_19 { get; set; }
            public string data { get; set; }
            public EddyStoneData EddyStone { get; set; }
            public iBeaconData iBeacon { get; set; }
            public MinewData Minew { get; set; }
            public RuuviData Ruuvi { get; set; }
            public string rssi { get; set; }

        } // class IngicsParsingClass

        ////////////////////////////////////////////////////////////////////

        static List<DeviceRawData> DeduplicateDevicesAndSensors(List<DeviceRawData> InputDevices)
        {
            var distinctDevices = InputDevices.Select(x => new { x.Uid, x.DateTime }).Distinct().Select(x => new { x.Uid, x.DateTime, sensors = new List<SensorRawData>() }).ToList();
            foreach (var device in InputDevices)
            {
                foreach (var sensor in device.Sensors)
                {
                    if (distinctDevices.FirstOrDefault(x => x.Uid == device.Uid && x.DateTime == device.DateTime).sensors.FirstOrDefault(r => r.Type == sensor.Type) == null)
                    {
                        distinctDevices.FirstOrDefault(x => x.Uid == device.Uid && x.DateTime == device.DateTime).sensors.Add(sensor);
                    }
                }
            }
            return distinctDevices.Select(x => InputDevices.First(r => r.Uid == x.Uid && r.DateTime == x.DateTime)).ToList(); ;
        } //DeduplicateTagsAndSensors

        //////////////////////////////////////////////////////

        public class MinewData
        {
            public string BatteryLevel { get; set; }
            public string AccelerationX { get; set; }
            public string AccelerationY { get; set; }
            public string AccelerationZ { get; set; }
            public string MAC { get; set; }
            public bool IsDataValid { get; set; }

            public MinewData() { IsDataValid = false; }

            //020106 0303E1FF 1216E1FFA103640026001E010272C7253F23AC
            public MinewData(string MinewString)
            {
                try
                {
                    IsDataValid = false;
                    if (!MinewString.StartsWith("0201060303E1FF")) { return; }
                    MinewString = MinewString.Substring(14);

                    if (string.IsNullOrEmpty(MinewString)) { return; }

                    var _length = Int16.Parse(MinewString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var _datatype = MinewString.Substring(2, 2);   // this should be 16 - Service data ?
                    var _uuid = MinewString.Substring(4, 4);  //0xE1FF
                    var _frameType = MinewString.Substring(8, 2);  // Should be A1 
                    var _version = MinewString.Substring(10, 2);  // Should be 01|03|05|07|08  

                    if (_uuid != "E1FF" || _frameType != "A1" || _datatype != "16") { return; }

                    var _data = MinewString.Substring(12);

                    if (_version == "03")
                    {
                        var _battery_level = int.Parse(_data.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);   // Percentage 
                        var _accelX = Convert8_8ToDecimal(_data.Substring(2, 4));
                        var _accelY = Convert8_8ToDecimal(_data.Substring(6, 4));
                        var _accelZ = Convert8_8ToDecimal(_data.Substring(10, 4));
                        var _mac = FormatMac(_data.Substring(14, 12));

                        BatteryLevel = _battery_level.ToString();
                        AccelerationX = _accelX.ToString();
                        AccelerationY = _accelY.ToString();
                        AccelerationZ = _accelZ.ToString();
                        MAC = _mac;
                        IsDataValid = true;
                    }
                }
                catch (Exception)
                {
                    IsDataValid = false;
                    //throw;
                }
            }

            internal decimal Convert8_8ToDecimal(string Txt88) // This works just for Minew 
            {
                var beforeDecimal = int.Parse(Txt88.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var afterDecimal = int.Parse(Txt88.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                if (beforeDecimal > 127)
                {
                    beforeDecimal -= 256;
                }

                return (decimal)beforeDecimal + (decimal)((int)(((decimal)afterDecimal / 256) * 100)) / 100;
            }

            internal string FormatMac(string MacReverse) // This works just for Minew 
            {
                return string.Format("{0}:{1}:{2}:{3}:{4}:{5}", MacReverse.Substring(10, 2)
                                                               , MacReverse.Substring(8, 2)
                                                               , MacReverse.Substring(6, 2)
                                                               , MacReverse.Substring(4, 2)
                                                               , MacReverse.Substring(2, 2)
                                                               , MacReverse.Substring(0, 2));
            }
        }
        //////////////////////////////////////////////////////
        public class RuuviData
        {
            public string BatteryVoltage { get; set; }
            public string AccelerationX { get; set; }
            public string AccelerationY { get; set; }
            public string AccelerationZ { get; set; }
            public string Temperature { get; set; }
            public string Humidity { get; set; }
            public string Pressure { get; set; }

            public bool IsDataValid { get; set; }

            public RuuviData() { IsDataValid = false; }

            public RuuviData(string RuuviString)
            {
                try
                {
                    IsDataValid = false;
                    if (!RuuviString.StartsWith("02010611FF9904")) { return; }
                    RuuviString = RuuviString.Substring(14);

                    if (string.IsNullOrEmpty(RuuviString)) { return; }

                    var _data_format = RuuviString.Substring(0, 2);

                    if (_data_format == "03")
                    {
                        var _humidity = int.Parse(RuuviString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) * 0.5;   //  
                        var _temperature = ConvertToDecimal_TwoPlusTwoBytes(RuuviString.Substring(4, 4));
                        var _pressure = (decimal)(int.Parse(RuuviString.Substring(8, 4), System.Globalization.NumberStyles.HexNumber) + 50000) / 100;
                        var _accelX = ConvertToDecimal_FourBytes(RuuviString.Substring(12, 4)) / 1000;
                        var _accelY = ConvertToDecimal_FourBytes(RuuviString.Substring(16, 4)) / 1000;
                        var _accelZ = ConvertToDecimal_FourBytes(RuuviString.Substring(20, 4)) / 1000;
                        var _battery = (decimal)int.Parse(RuuviString.Substring(24, 4), System.Globalization.NumberStyles.HexNumber) / 1000;

                        BatteryVoltage = _battery.ToString();
                        AccelerationX = _accelX.ToString();
                        AccelerationY = _accelY.ToString();
                        AccelerationZ = _accelZ.ToString();
                        Pressure = _pressure.ToString();
                        Temperature = _temperature.ToString();
                        Humidity = _humidity.ToString();

                        IsDataValid = true;
                    }
                }
                catch (Exception)
                {
                    IsDataValid = false;
                    //throw;
                }
            }
            /////////////////////////////////////
            internal decimal ConvertToDecimal_TwoPlusTwoBytes(string Txt88) // This works just for Ruuvi 
            {
                var beforeDecimal = int.Parse(Txt88.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var afterDecimal = int.Parse(Txt88.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                if (beforeDecimal > (int)0x8000)
                {
                    beforeDecimal = (beforeDecimal ^ 0x8000) - 0x8000;
                }
                return (decimal)beforeDecimal + ((decimal)afterDecimal / 100);
            }
            /////////////////////////////////////
            internal decimal ConvertToDecimal_FourBytes(string Txt88) // This works just for Ruuvi 
            {
                var value = int.Parse(Txt88, System.Globalization.NumberStyles.HexNumber);
                if (value > (int)0x8000)
                {
                    value = (value ^ 0x8000) - 0x8000;
                }
                return (decimal)value;
            }
        }
        #endregion 
    }
}
