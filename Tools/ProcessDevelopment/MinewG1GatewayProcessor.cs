using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Linq;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class MinewG1GatewayProcessor : IProcessor
    {
        #region main block 
        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            // MineW
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body)) { return; }
            var _tags = new List<MinewTagData>();

            try
            {
                _tags = JsonConvert.DeserializeObject<List<MinewTagData>>(gatewayData.Body);
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }            

            foreach (var tag in _tags)
            {
                var beaconDevice = new DeviceRawData();
                gatewayData.RawDevices.Add(beaconDevice);
                var currentDateTime = gatewayData.DateTime;
                beaconDevice.GatewayId = gatewayData.GatewayId;

                try
                {
                    currentDateTime = DateTime.Parse(tag.timestamp).ToUniversalTime();
                    beaconDevice.Uid = FormatMacAddress(tag.mac);
                    beaconDevice.DateTime = currentDateTime;
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }

                //Calculating SignalStrength
                if (!string.IsNullOrEmpty(tag.ibeaconTxPower) && !string.IsNullOrEmpty(tag.rssi))  // only do tx if rssi exists & VV
                    AddSensorMultiValue(beaconDevice, "SignalStrength", currentDateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, tag.rssi, tag.ibeaconTxPower, string.Empty, gatewayData);

                //Calculating Temperature
                if (!string.IsNullOrEmpty(tag.temperature))
                    AddSensor(beaconDevice, "Temperature", currentDateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, tag.temperature, gatewayData);

                //Calculating Humidity
                if (!string.IsNullOrEmpty(tag.humidity))
                    AddSensor(beaconDevice, "Humidity", currentDateTime, SensorTypes.Humidity, SensorUnits.AbsoluteHumidity, SensorPrefixes.One, tag.humidity, gatewayData);

                //Calculating Voltage
                if (tag.type == "Unknown")
                {
                    if (!string.IsNullOrEmpty(tag.battery))                    
                        AddSensor(beaconDevice, "BatteryLevel", currentDateTime, SensorTypes.Voltage, SensorUnits.Percent, SensorPrefixes.One, tag.battery, gatewayData);
                    
                }

                //Calculating SignalStrength
                if (!string.IsNullOrEmpty(tag.rawData))
                {
                    try
                    {
                        var parsingData = ParseData(tag.rawData);
                        if (parsingData.IsDataValid)
                        {
                            if (parsingData.EddyStoneList != null && parsingData.EddyStoneList.Count > 0)
                            {
                                foreach (var ed in parsingData.EddyStoneList)
                                {
                                    if (ed.IsDataValid)
                                    {
                                        //Calculating SignalStrength
                                        if (!string.IsNullOrEmpty(ed.TxPower) && !string.IsNullOrEmpty(tag.rssi))
                                            AddSensorMultiValue(beaconDevice, "SignalStrength", currentDateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, tag.rssi, ed.TxPower, string.Empty, gatewayData);

                                        //Calculating Voltage
                                        if (!string.IsNullOrEmpty(ed.BatteryVoltage))                                        
                                            AddSensor(beaconDevice, "Voltage", currentDateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, ed.BatteryVoltage, gatewayData);
                                        

                                        //Calculating Temperature
                                        if (!string.IsNullOrEmpty(ed.Temperature))
                                            AddSensor(beaconDevice, "Temperature", currentDateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, ed.Temperature, gatewayData);
                                    }
                                }
                            }

                            if (parsingData.iBeaconList != null)
                            {
                                foreach (var ib in parsingData.iBeaconList)
                                {
                                    if (ib.IsDataValid)
                                    {
                                        //Calculating SignalStrength
                                        if (!string.IsNullOrEmpty(ib.SignalPower) && !string.IsNullOrEmpty(tag.rssi))
                                            AddSensorMultiValue(beaconDevice, "SignalStrength", currentDateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, tag.rssi, ib.SignalPower, string.Empty, gatewayData);
                                        
                                        //Calculating Distance
                                        if (!string.IsNullOrEmpty(ib.Distance))
                                            AddSensor(beaconDevice, "Distance", currentDateTime, SensorTypes.Distance, SensorUnits.Meter, SensorPrefixes.One, ib.Distance, gatewayData);
                                    }
                                }
                            }
                            if (parsingData.MinewData != null)
                            {
                                //Calculating Temperature
                                if (!string.IsNullOrEmpty(parsingData.MinewData.temprature))
                                    AddSensor(beaconDevice, "Temperature", currentDateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, parsingData.MinewData.temprature, gatewayData);

                                //Calculating Illuminance
                                if (!string.IsNullOrEmpty(parsingData.MinewData.lux))
                                    AddSensor(beaconDevice, "Illuminance", currentDateTime, SensorTypes.Illuminance, SensorUnits.Lux, SensorPrefixes.One, parsingData.MinewData.lux, gatewayData);

                                //Calculating Acceleration
                                if (!string.IsNullOrEmpty(parsingData.MinewData.xaxis) && !string.IsNullOrEmpty(parsingData.MinewData.yaxis) && !string.IsNullOrEmpty(parsingData.MinewData.zaxis))
                                    AddSensorMultiValue(beaconDevice, "Acceleration", currentDateTime, SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared, SensorPrefixes.One, parsingData.MinewData.xaxis, parsingData.MinewData.yaxis, parsingData.MinewData.zaxis, gatewayData);
                                
                                //Calculating Voltage
                                if (!string.IsNullOrEmpty(parsingData.MinewData.battery_level))
                                    AddSensor(beaconDevice, "BatteryLevel", currentDateTime, SensorTypes.Voltage, SensorUnits.Percent, SensorPrefixes.One, parsingData.MinewData.battery_level, gatewayData);

                                //Calculating Humidity
                                if (!string.IsNullOrEmpty(parsingData.MinewData.humidity))
                                    AddSensor(beaconDevice, "Humidity", currentDateTime, SensorTypes.Humidity, SensorUnits.AbsoluteHumidity, SensorPrefixes.One, parsingData.MinewData.humidity, gatewayData);

                            } // if minew data 
                        }

                        var Ruuvi = new RuuviData(tag.rawData);
                        if (Ruuvi != null)
                        {
                            if (Ruuvi.IsDataValid)
                            {
                                //Calculating Voltage
                                if (Ruuvi.BatteryVoltage != null)
                                    AddSensor(beaconDevice, "Voltage", currentDateTime, SensorTypes.Voltage, SensorUnits.Volt, SensorPrefixes.One, Ruuvi.BatteryVoltage, gatewayData);

                                //Calculating Acceleration
                                if (Ruuvi.AccelerationX != null && Ruuvi.AccelerationY != null && Ruuvi.AccelerationZ != null)
                                    AddSensorMultiValue(beaconDevice, "Acceleration", currentDateTime, SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared, SensorPrefixes.One, Ruuvi.AccelerationX, Ruuvi.AccelerationY, Ruuvi.AccelerationZ, gatewayData);
                                
                                //Calculating Pressure
                                if (Ruuvi.Pressure != null)
                                    AddSensor(beaconDevice, "Pressure", currentDateTime, SensorTypes.Pressure, SensorUnits.Atmosphere, SensorPrefixes.One, Ruuvi.Pressure, gatewayData);

                                //Calculating Temperature
                                if (Ruuvi.Temperature != null)
                                    AddSensor(beaconDevice, "Temperature", currentDateTime, SensorTypes.Temperature, SensorUnits.Centigrade, SensorPrefixes.One, Ruuvi.Temperature, gatewayData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        gatewayData.LogException(ex);
                    }
                }
            }

            gatewayData.RawDevices = DeduplicateDevicesAndSensors(gatewayData.RawDevices);
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

        internal class MinewTagData
        {
            public string timestamp { get; set; }
            public string type { get; set; }
            public string mac { get; set; }
            public string bleName { get; set; }
            public string ibeaconUuid { get; set; }
            public string ibeaconMajor { get; set; }
            public string ibeaconMinor { get; set; }
            public string rssi { get; set; }
            public string ibeaconTxPower { get; set; }
            public string battery { get; set; }
            public string gatewayFree { get; set; }
            public string gatewayLoad { get; set; }
            public string rawData { get; set; }
            public string temperature { get; set; }
            public string humidity { get; set; }

        }  // class MinewTagData

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
                    var _length = short.Parse(data.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    var _type = data.Substring(2, 2);
                    var _uuid = data.Substring(4, 4);
                    var _frameType = data.Substring(8, 2);
                    var _data = data.Substring(4, (_length - 1) * 2);

                    if (_type == "16" && _uuid == "E1FF" && _frameType == "A1") //  Find if it is Minew Specific 
                    {
                        var Minew = ParseMinewData(data);
                        if (Minew != null)
                        {
                            if (Minew.IsDataValid)
                            {
                                parsingData.MinewData = Minew;
                                parsingData.IsDataValid = true;
                            }
                        }
                    }
                    else if (_type == "16") //Eddystone
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
                        }
                    }

                    data = data.Substring((_length + 1) * 2);
                }
            }
            catch (Exception ex)
            {
                var error = ex;
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
            public MinewExtendedData MinewData { get; set; }
            public string rssi { get; set; }

            public bool IsDataValid { get; set; }
        }

        private MinewExtendedData ParseMinewData(string data)
        {
            var MinewData = new MinewExtendedData();
            MinewData.IsDataValid = false;

            try
            {
                var _length = short.Parse(data.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var _type = data.Substring(2, 2); // service data
                var _uuid = data.Substring(4, 4);
                var _frameType = data.Substring(8, 2);
                var _versionNumber = data.Substring(10, 2);

                switch (_versionNumber)
                {
                    case "01":

                    default:
                        break;
                }

                if (_versionNumber == "01")  // bat level + hum + temp
                {
                    var battery_level = short.Parse(data.Substring(12, 2), System.Globalization.NumberStyles.HexNumber);
                    var temperature = decimal.Parse(short.Parse(data.Substring(14, 2), System.Globalization.NumberStyles.HexNumber).ToString() + "." + (short.Parse(data.Substring(16, 2), System.Globalization.NumberStyles.HexNumber) * 100 / 256).ToString());
                    var humidity = short.Parse(data.Substring(18, 2), System.Globalization.NumberStyles.HexNumber).ToString() + "." + (short.Parse(data.Substring(20, 2), System.Globalization.NumberStyles.HexNumber) * 100 / 256).ToString();

                    MinewData.battery_level = battery_level.ToString();
                    MinewData.temprature = (temperature > 127 ? temperature - 256 : temperature).ToString();
                    MinewData.humidity = humidity.ToString();
                    MinewData.IsDataValid = true;
                }
                else if (_versionNumber == "03") //xyz axis + batt level
                {
                    var battery_level = int.Parse(data.Substring(12, 2), System.Globalization.NumberStyles.HexNumber);
                    //var battery_level = Convert8_8ToDecimal(data.Substring(12, 2));
                    var xaxis = Convert8_8ToDecimal(data.Substring(14, 4));
                    var yaxis = Convert8_8ToDecimal(data.Substring(18, 4));
                    var zaxis = Convert8_8ToDecimal(data.Substring(22, 4));

                    MinewData.battery_level = battery_level.ToString();
                    MinewData.xaxis = xaxis.ToString();
                    MinewData.yaxis = yaxis.ToString();
                    MinewData.zaxis = zaxis.ToString();
                    MinewData.IsDataValid = true;

                }
                else if (_versionNumber == "05") // batt level + lux 
                {
                    var batter_level = short.Parse(data.Substring(12, 2), System.Globalization.NumberStyles.HexNumber);
                    var lux = short.Parse(data.Substring(14, 4), System.Globalization.NumberStyles.HexNumber);

                    MinewData.battery_level = batter_level.ToString();
                    MinewData.lux = lux.ToString();
                    MinewData.IsDataValid = true;
                }
                else if (_versionNumber == "08") // batt level  
                {
                    var battery_level = short.Parse(data.Substring(12, 2), System.Globalization.NumberStyles.HexNumber);
                    //var battery_level = Convert8_8ToDecimal(data.Substring(10, 4));
                    MinewData.battery_level = battery_level.ToString();
                    MinewData.IsDataValid = true;
                }
                return MinewData;
            }
            catch (Exception)
            {
                return null;
            }

        }//ParseMinewData

        internal class MinewExtendedData
        {
            public string battery_level { get; set; }
            public string temprature { get; set; }
            public string humidity { get; set; }
            public string xaxis { get; set; }
            public string yaxis { get; set; }
            public string zaxis { get; set; }
            public string lux { get; set; }
            public string mac { get; set; }
            public bool IsDataValid { get; set; }
        }

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

            return beforeDecimal + (decimal)afterDecimal / 256;
        }

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

            public RuuviData(string RuuviString)
            {
                try
                {

                    IsDataValid = false;
                    if (!RuuviString.StartsWith("02010611FF9904")) { return; }
                    RuuviString = RuuviString.Substring(14);

                    if (string.IsNullOrEmpty(RuuviString)) { return; }

                    var _data_format = RuuviString.Substring(0, 2);
                    //Int16.Parse(RuuviString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);

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

        private void AddSensor(DeviceRawData beaconDevice, string Uid, DateTime currentDateTime, SensorTypes type, SensorUnits unit, SensorPrefixes prefix, string value, GatewayData gatewayData)
        {
            try
            {
                beaconDevice.Sensors.Add(new SensorRawData(Uid, currentDateTime, type, unit, prefix, decimal.Parse(value)));
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }
        }

        private void AddSensorMultiValue(DeviceRawData beaconDevice, string Uid, DateTime currentDateTime, SensorTypes type, SensorUnits unit, SensorPrefixes prefix, string x, string y, string z, GatewayData gatewayData)
        {
            try
            {
                if (type == SensorTypes.SignalStrength)
                    beaconDevice.Sensors.Add(new SensorRawData(Uid, currentDateTime, type, unit, prefix,
                                                    new Dictionary<string, decimal> { { "rssi", decimal.Parse(x) }, { "txPower", decimal.Parse(y) } }));
                else
                    beaconDevice.Sensors.Add(new SensorRawData(Uid, currentDateTime, type, unit, prefix,
                                                    new Dictionary<string, decimal> { { "x", decimal.Parse(x) }, { "y", decimal.Parse(y) }, { "z", decimal.Parse(z) } }));
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }
        }

        private string GetBatteryLevel(double voltage)
        {
            if (voltage > 3)
                return "100";

            else if (voltage > 2.9 && voltage <= 3) 
                return (100 - (3 - voltage) * 58 / 0.1).ToString();

            else if (voltage > 2.74 && voltage <= 2.9)            
                return (42 - (2.9 - voltage) * 24 / 0.16).ToString();
            
            else if (voltage > 2.44 && voltage <= 2.74)            
                return (18 - (2.74 - voltage) * 12 / 0.3).ToString();
            
            else if (voltage > 2.1 && voltage <= 2.44)            
                return (6 - (2.44 - voltage) * 6 / 0.34).ToString();

            else return "0";
        }

        #endregion 

    } //class  

} // namespace
