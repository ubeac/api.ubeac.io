using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class JaaleeGatewayProcessor : IProcessor
    {
        #region main block

        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            // JAALEE
            ////////////////////////////////////////////////////////////////////////
            
            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            var _data = JsonConvert.DeserializeObject<JaaleeData>(gatewayData.Body);

            foreach (var tag in _data.devices)
            {
                var beaconDevice = new DeviceRawData();
                beaconDevice.GatewayId = gatewayData.GatewayId;
                gatewayData.RawDevices.Add(beaconDevice);
                var currentDateTime = gatewayData.DateTime;

                try
                {
                    beaconDevice.Uid = FormatMacAddress(tag.mac);
                    beaconDevice.DateTime = currentDateTime;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                if (!string.IsNullOrEmpty(tag.data))
                {
                    var parsingData = ParseData(tag.data, gatewayData);
                    var Minew = new MinewData(tag.data, gatewayData); // Try to read Minew Data 
                    var Ruuvi = new RuuviData(tag.data, gatewayData); // Try to read Ruuvi Data 

                    if (parsingData.EddyStoneList != null && parsingData.EddyStoneList.Count > 0)
                        PopulateEddyStoneSensors(parsingData.EddyStoneList, beaconDevice, tag, gatewayData);

                    if (parsingData.iBeaconList != null && parsingData.iBeaconList.Count > 0)
                        PopulateIBeaconSensors(parsingData.iBeaconList, beaconDevice, tag, gatewayData);

                    if (Minew != null)
                        PopulateMinewSensors(Minew, beaconDevice, gatewayData);

                    if (Ruuvi != null)
                        PopulateRuuviSensors(Ruuvi, beaconDevice, gatewayData);
                }

            }
            gatewayData.RawDevices = DeduplicateDevicesAndSensors(gatewayData.RawDevices);
        }

        private void PopulateEddyStoneSensors(List<EddyStoneData> eddyStoneList, DeviceRawData beaconDevice, JaaleeTagData tag, GatewayData gatewayData)
        {
            foreach (var eddyStone in eddyStoneList)
            {
                if (eddyStone.IsDataValid)
                {
                    if (!string.IsNullOrEmpty(eddyStone.TxPower) && !string.IsNullOrEmpty(tag.rssi))
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("SignalStrength", beaconDevice.DateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One,
                                new Dictionary<string, decimal> { { "rssi", decimal.Parse(tag.rssi) }, { "txPower", decimal.Parse(eddyStone.TxPower) } }));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }

                    if (!string.IsNullOrEmpty(eddyStone.BatteryVoltage))
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Voltage", beaconDevice.DateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, decimal.Parse(eddyStone.BatteryVoltage)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }

                    if (!string.IsNullOrEmpty(eddyStone.Temperature))
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("Temperature", beaconDevice.DateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, decimal.Parse(eddyStone.Temperature)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
            }
        }

        private void PopulateMinewSensors(MinewData minew, DeviceRawData beaconDevice, GatewayData gatewayData)
        {
            if (minew.IsDataValid)
            {
                if (!string.IsNullOrEmpty(minew.BatteryLevel))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Voltage", beaconDevice.DateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, decimal.Parse(minew.BatteryLevel)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }

                if (!string.IsNullOrEmpty(minew.AccelerationX) && !string.IsNullOrEmpty(minew.AccelerationY) && !string.IsNullOrEmpty(minew.AccelerationZ))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Acceleration", beaconDevice.DateTime, SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared, SensorPrefixes.One,
                            new Dictionary<string, decimal> { { "x", decimal.Parse(minew.AccelerationX) }, { "y", decimal.Parse(minew.AccelerationY) }, { "z", decimal.Parse(minew.AccelerationZ) } }));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
            }
        }

        private void PopulateRuuviSensors(RuuviData ruuvi, DeviceRawData beaconDevice, GatewayData gatewayData)
        {
            if (ruuvi.IsDataValid)
            {
                if (!string.IsNullOrEmpty(ruuvi.BatteryVoltage))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Voltage", beaconDevice.DateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, decimal.Parse(ruuvi.BatteryVoltage)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }

                if (!string.IsNullOrEmpty(ruuvi.AccelerationX) && !string.IsNullOrEmpty(ruuvi.AccelerationY) && !string.IsNullOrEmpty(ruuvi.AccelerationZ))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Acceleration", beaconDevice.DateTime, SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared, SensorPrefixes.One,
                            new Dictionary<string, decimal> { { "x", decimal.Parse(ruuvi.AccelerationX) }, { "y", decimal.Parse(ruuvi.AccelerationY) }, { "z", decimal.Parse(ruuvi.AccelerationZ) } }));                        
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }

                if (!string.IsNullOrEmpty(ruuvi.Pressure))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Pressure", beaconDevice.DateTime, SensorTypes.Pressure, SensorUnits.Pascal, SensorPrefixes.One, decimal.Parse(ruuvi.Pressure)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }

                if (!string.IsNullOrEmpty(ruuvi.Temperature))
                {
                    try
                    {
                        beaconDevice.Sensors.Add(new SensorRawData("Temperature", beaconDevice.DateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, decimal.Parse(ruuvi.Temperature)));
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
            }
        }

        private void PopulateIBeaconSensors(List<iBeaconData> iBeaconList, DeviceRawData beaconDevice, JaaleeTagData tag, GatewayData gatewayData)
        {
            foreach (var ib in iBeaconList)
            {
                if (ib.IsDataValid)
                {
                    if (!string.IsNullOrEmpty(ib.SignalPower) && !string.IsNullOrEmpty(tag.rssi))
                    {
                        try
                        {
                            beaconDevice.Sensors.Add(new SensorRawData("SignalStrength", beaconDevice.DateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One,
                                new Dictionary<string, decimal> { { "rssi", decimal.Parse(tag.rssi) }, { "txPower", decimal.Parse(ib.SignalPower) } }));
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
                            beaconDevice.Sensors.Add(new SensorRawData("Distance", beaconDevice.DateTime, SensorTypes.Distance, SensorUnits.Meter, SensorPrefixes.One, decimal.Parse(ib.Distance)));
                        }
                        catch (Exception ex)
                        {
                            gatewayData.LogException(ex);
                        }
                    }
                }
            }
        }

        private ParsingData ParseData(string data, GatewayData gatewayData)
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
                        }
                    }

                    data = data.Substring((_length + 1) * 2);
                }
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
                parsingData.IsDataValid = false;
                // todo: Log and handle this
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

        internal class JaaleeData
        {
            public string wifi_mac { get; set; }
            public string route_mac { get; set; }
            public List<JaaleeTagData> devices { get; set; }
            public string allCount { get; set; }


        }  // class JaaleeDat

        internal class JaaleeTagData
        {
            public string mac { get; set; }
            public string data { get; set; }
            public string rssi { get; set; }
        }  // class JaaleeTag

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
        }

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
            public MinewData(string MinewString, GatewayData gatewayData)
            {
                try
                {
                    IsDataValid = false;
                    if (!MinewString.StartsWith("0201060303E1FF")) { return; }
                    MinewString = MinewString.Substring(14);

                    if (string.IsNullOrEmpty(MinewString)) { return; }

                    var _length = short.Parse(MinewString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
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
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                    IsDataValid = false;
                }
            }


            internal decimal Convert8_8ToDecimal(string Txt88) // This works just for Minew 
            {
                var beforeDecimal = int.Parse(Txt88.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var afterDecimal = int.Parse(Txt88.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                //var sign = 1;
                if (beforeDecimal > 127)
                {
                    //    sign = -1;
                    beforeDecimal -= 256;
                }

                return (decimal)beforeDecimal + (decimal)((int)(((decimal)afterDecimal / 256) * 100)) / 100;
            }

            //009078563412 --> 12:34:56:78:90:00
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

            // 020106 11 FF 9904 03 44 19 16 BFFD 003B FFBD FBF0 0B89
            // 11: size 
            // FF: 
            // 9904 : Ruuvi 
            // 03 : protocol version 
            // 44 humidity(multply by 0.5)
            // 19 temp 1  --> 25 
            // 16 temp 2  --> .22
            // BFFD pressure 
            // 003B Acc-x
            // FFBD Acc-Y
            // FBF0 Acc-Z     
            // 0B89 battery   --> mili volt

            public RuuviData(string RuuviString, GatewayData gatewayData)
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
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                    IsDataValid = false;
                    //throw;
                }
            }

            /////////////////////////////////////

            internal decimal ConvertToDecimal_TwoPlusTwoBytes(string Txt88) // This works just for Ruuvi 
            {
                var beforeDecimal = int.Parse(Txt88.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var afterDecimal = int.Parse(Txt88.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                if (beforeDecimal > 0x8000)
                {
                    beforeDecimal = (beforeDecimal ^ 0x8000) - 0x8000;
                }
                return beforeDecimal + ((decimal)afterDecimal / 100);
            }
            /////////////////////////////////////
            internal decimal ConvertToDecimal_FourBytes(string Txt88) // This works just for Ruuvi 
            {
                var value = int.Parse(Txt88, System.Globalization.NumberStyles.HexNumber);
                if (value > 0x8000)
                {
                    value = (value ^ 0x8000) - 0x8000;
                }
                return value;
            }
        }
        #endregion 
    }
}
