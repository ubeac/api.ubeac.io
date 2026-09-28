using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using uBeac.Models;

/// <summary>
/// This is processor for: Android Data Collectore APP 
/// https://play.google.com/store/apps/details?id=tech.unismart.dc 
/// https://unismart.tech/projects/data-collector/
/// </summary>
/// 
namespace uBeac.IoT.Processing
{
    public class AndroidDataCollectorProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            ////////////////////////////////////////////////////////////////////////
            // Processor section
            ////////////////////////////////////////////////////////////////////////

            if (string.IsNullOrEmpty(gatewayData.Body))
                return;

            try
            {
                //var stringContent1 = JsonConvert.DeserializeObject(gatewayData.Body);
                var token = JToken.Parse(gatewayData.Body);

                if (token is JArray)
                {
                    List<string> listRawData = token.ToObject<List<string>>();

                    ////// If user doesn't check any of checkboxes in Datacollector settings it will send a list of values which is not acceptable and first item of that list is timestamp
                    try
                    {
                        var checkFirstItem = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(listRawData[0]));
                        gatewayData.LogException("Detacollector settings is not correct.");
                    }
                    catch (Exception)
                    {
                        ParsJsonArray(gatewayData, listRawData);
                    }                    
                }
                else if (token is JObject)
                {
                    gatewayData.LogException("Please set the 'Strict Json' property as unchecked and set 'Include UUID', 'Include task name', 'Include sensor name' and 'Include sensor units' as checked in Android Data Collector application");
                }
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }
        }

        private decimal? GetSensorDecimalValue(string sensorName, string replacePhrase, Dictionary<string, string> rawDataDict, GatewayData gatewayData)
        {
            if (rawDataDict.ContainsKey(sensorName))
            {
                try
                {
                    if (sensorName != "null")
                    {
                        string stringValue = rawDataDict[sensorName].Replace(replacePhrase, string.Empty).Trim();
                        return ToDecimal(stringValue);
                    }
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }
            }
            return null;
        }

        private Dictionary<string, decimal> GetSensorDecimalTripleValues(string sensorName, string replacePhrase, Dictionary<string, string> rawDataDict, GatewayData gatewayData)
        {
            Dictionary<string, decimal> values = new Dictionary<string, decimal>();

            if (rawDataDict.ContainsKey(sensorName))
            {
                try
                {
                    if (sensorName != "null")
                    {
                        var splitedValue = rawDataDict[sensorName].Split(';');
                        var xString = splitedValue[0].Replace(replacePhrase, string.Empty).Trim();
                        var yString = splitedValue[1].Replace(replacePhrase, string.Empty).Trim();
                        var zString = splitedValue[2].Replace(replacePhrase, string.Empty).Trim();

                        decimal? x, y, z;
                        x = ToDecimal(xString);
                        y = ToDecimal(yString);
                        z = ToDecimal(zString);

                        if (!x.HasValue || !y.HasValue)
                            return values;

                        if (sensorName == "Location")
                        {
                            values.Add("latitude", x.Value);
                            values.Add("longitude", y.Value);

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
                }
                catch (Exception ex)
                {
                    gatewayData.LogException(ex);
                }
            }
            return null;
        }

        private void ParsJsonArray(GatewayData gatewayData, List<string> listRawData)
        {
            var currentDateTime = gatewayData.DateTime;
            try
            {
                currentDateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(listRawData[2])).UtcDateTime;
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }

            var deviceData = new DeviceRawData();
            var rawDataDict = new Dictionary<string, string>();

            gatewayData.RawDevices.Add(deviceData);

            try
            {
                deviceData.IsValid = true;
                deviceData.GatewayId = gatewayData.GatewayId;
                deviceData.DateTime = currentDateTime;
                deviceData.Uid = listRawData[0];
                deviceData.DateTime = currentDateTime;
                listRawData.RemoveRange(0, 3);
                rawDataDict = listRawData.ToDictionary(x => x.Split(":")[0], y => y.Split(":")[1]);
            }
            catch (Exception ex)
            {
                gatewayData.LogException(ex);
            }

            var sensorNamesDic = new Dictionary<string, Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes>>
            {
                {"Pressure", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Pressure", "hPa",SensorTypes.Pressure, SensorUnits.Pascal,  SensorPrefixes.Hundred) },
                {"Proximity", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Proximity", "cm",SensorTypes.Proximity, SensorUnits.Meter,  SensorPrefixes.Hundredth) },
                {"Light", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Illuminance", "lux",SensorTypes.Illuminance, SensorUnits.Lux,  SensorPrefixes.One) },
                {"Step counter", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Counter", "lux",SensorTypes.Counter, SensorUnits.Count,  SensorPrefixes.One) },

            };

            var sensorNamesDicTripleValues = new Dictionary<string, Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes>>
            {
                {"Accelerometer", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Acceleration", "m/s^2",SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared,  SensorPrefixes.One) },
                {"Magnetic field", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Magnetic field", "uT",SensorTypes.MagneticField, SensorUnits.Tesla,  SensorPrefixes.Millionth) },
                {"Orientation", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Orientation", "deg",SensorTypes.Orientation, SensorUnits.Degree,  SensorPrefixes.One) },
                {"Linear acceleration", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Linear acceleration", "m/s^2",SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared,  SensorPrefixes.One) },
                {"Gravity", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Gravity", "m/s^2",SensorTypes.Acceleration, SensorUnits.MeterPerSecondSquared,  SensorPrefixes.One) },
                {"Gyroscope", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Gyroscope", "rad/s",SensorTypes.Gyroscope, SensorUnits.RadiansPerSecond,  SensorPrefixes.One) },
                {"Location", new Tuple<string, string, SensorTypes, SensorUnits, SensorPrefixes> ("Location", "deg",SensorTypes.Location, SensorUnits.Degree,  SensorPrefixes.One) }
            };

            foreach (var item in sensorNamesDic)
            {
                var decimalValue = GetSensorDecimalValue(item.Key, item.Value.Item2, rawDataDict, gatewayData);
                if (decimalValue.HasValue)
                    deviceData.Sensors.Add(new SensorRawData(item.Value.Item1, currentDateTime, item.Value.Item3, item.Value.Item4, item.Value.Item5, decimalValue.Value));
            }

            foreach (var item in sensorNamesDicTripleValues)
            {
                var dictValues = GetSensorDecimalTripleValues(item.Key, item.Value.Item2, rawDataDict, gatewayData);
                if (dictValues != null && dictValues.Count > 0)
                    deviceData.Sensors.Add(new SensorRawData(item.Value.Item1, currentDateTime, item.Value.Item3, item.Value.Item4, item.Value.Item5, dictValues));
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
}




//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using uBeac.Models;

///// <summary>
///// This is processor for: Android Data Collectore APP 
///// https://play.google.com/store/apps/details?id=tech.unismart.dc 
///// https://unismart.tech/projects/data-collector/
///// </summary>
///// 
//namespace uBeac.IoT.Processing
//{
//    public class AndroidDataCollectorProcessor : IProcessor
//    {
//        public void Process(GatewayData gatewayData)
//        {
//            ////////////////////////////////////////////////////////////////////////
//            // Processor section
//            ////////////////////////////////////////////////////////////////////////

//            try
//            {
//                var stringContent1 = JsonConvert.DeserializeObject(gatewayData.Body);
//                var token = JToken.Parse(gatewayData.Body);

//                if (token is JArray)
//                {
//                    List<string> listRawData = token.ToObject<List<string>>();
//                    var a = false;
//                    foreach (var item in token)
//                    {
//                        var xx = item.GetType();
//                        if (item is JObject)
//                            a = true;
//                    }

//                    ParsJsonArray(gatewayData, listRawData);
//                }
//                else if (token is JObject)
//                {
//                    gatewayData.LogException("Please set the 'Strict Json' property as unchecked and set 'Include UUID' as checked, 'Include task name', 'Include sensor name' and 'Include sensor units' in Android Data Collector application");
//                }
//            }
//            catch (Exception ex)
//            {
//                gatewayData.LogException(ex);
//            }
//        }

//        private void ParsJsonArray(GatewayData gatewayData, List<string> listRawData)
//        {
//            var currentDateTime = gatewayData.DateTime;

//            try
//            {
//                currentDateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(listRawData[2])).UtcDateTime;
//            }
//            catch (Exception ex)
//            {
//                gatewayData.LogException(ex);
//            }

//            var deviceData = new DeviceRawData
//            {
//                IsValid = true,
//                Sensors = new List<SensorRawData>()
//            };
//            gatewayData.RawDevices.Add(deviceData);

//            try
//            {
//                deviceData.Uid = listRawData[0];
//                deviceData.DateTime = currentDateTime;
//                listRawData.RemoveRange(0, 3);
//            }
//            catch (Exception ex)
//            {
//                gatewayData.LogException(ex);
//            }

//            foreach (var lineitem in listRawData)
//            {
//                ////////////////////////////////////////////////////////////// Pressure:989.33624 hPa
//                if (lineitem.StartsWith("Pressure:"))
//                {
//                    try
//                    {
//                        string PressureValueTxt = lineitem.Replace("Pressure:", "").Replace("hPa", "").Trim();
//                        if (PressureValueTxt == "null") { continue; }
//                        decimal PressureValue = GetDecimalValue(PressureValueTxt);
//                        deviceData.Sensors.Add(new SensorRawData { Type = 12, Unit = 19, Prefix = 3, SensorUid = "Pressure", Data = new Dictionary<string, decimal> { { "Value", PressureValue } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                /////////////////////////////////////////////////////////// Proximity:8.000183 cm"
//                if (lineitem.StartsWith("Proximity:"))
//                {
//                    try
//                    {
//                        string ProximityValueTxt = lineitem.Replace("Proximity:", "").Replace("cm", "").Trim();
//                        if (ProximityValueTxt == "null") { continue; }
//                        // todo: we may need some code to convert to m | mm 
//                        decimal ProximityValue = GetDecimalValue(ProximityValueTxt);
//                        deviceData.Sensors.Add(new SensorRawData { Type = 10, Prefix = -2, Unit = 5, SensorUid = "Proximity", Data = new Dictionary<string, decimal> { { "Value", ProximityValue } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                /////////////////////////////////////////////////////////  "Light:178.0 lux"
//                if (lineitem.StartsWith("Light:"))
//                {
//                    try
//                    {
//                        string LightValueTxt = lineitem.Replace("Light:", "").Replace("lux", "").Trim();
//                        if (LightValueTxt == "null") { continue; }
//                        decimal LightValue = GetDecimalValue(LightValueTxt);
//                        deviceData.Sensors.Add(new SensorRawData { Type = 11, Prefix = 0, Unit = 24, SensorUid = "Illuminance", Data = new Dictionary<string, decimal> { { "Value", LightValue } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// "Step counter:2779.0"
//                if (lineitem.StartsWith("Step counter:"))
//                {
//                    try
//                    {
//                        string SetpCounterValueTxt = lineitem.Replace("Step counter:", "").Trim();
//                        if (SetpCounterValueTxt == "null") { continue; }
//                        decimal SetpCounterValue = GetDecimalValue(SetpCounterValueTxt);
//                        deviceData.Sensors.Add(new SensorRawData { Type = 13, Unit = 6, Prefix = 0, SensorUid = "Counter", Data = new Dictionary<string, decimal> { { "Value", SetpCounterValue } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// //Accelerometer:-0.0023956299 m/s^2;0.14356995 m/s^2;9.920639 m/s^2",
//                if (lineitem.StartsWith("Accelerometer:"))
//                {
//                    try
//                    {
//                        string AccString = lineitem.Replace("Accelerometer:", "").Trim();
//                        var AccArray = AccString.Split(';');

//                        var tempX = AccArray[0].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = AccArray[1].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = AccArray[2].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 7, Prefix = 0, Unit = 26, SensorUid = "Acceleration", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// //Magneticfield:4.9 uT;-24.9 uT;-38.9 uT
//                if (lineitem.StartsWith("Magnetic field:"))
//                {
//                    try
//                    {
//                        string magneticString = lineitem.Replace("Magnetic field:", "").Trim();
//                        var magneticArray = magneticString.Split(';');

//                        var tempX = magneticArray[0].Replace("uT", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = magneticArray[1].Replace("uT", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = magneticArray[2].Replace("uT", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 8, Unit = 27, Prefix = -6, SensorUid = "Magnetic field", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// "Location:43.84623273 deg;-79.38754834 deg;164.4674072265625 m"                
//                if (lineitem.StartsWith("Location:"))
//                {
//                    try
//                    {
//                        var LocationString = lineitem.Replace("Location:", "").Trim();
//                        if (LocationString.Contains("null deg")) { continue; } //nothing to do 
//                        var LocationArray = LocationString.Split(';');

//                        var tempLat = LocationArray[0].Replace("deg", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempLat) || tempLat == "null") continue;

//                        var tempLong = LocationArray[1].Replace("deg", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempLong) || tempLong == "null") continue;

//                        var data = new Dictionary<string, decimal> { { "Longitude", GetDecimalValue(tempLong) }, { "Latitude", GetDecimalValue(tempLat) } };

//                        if ((!LocationString.Contains("null m")) && (!LocationString.Contains("0.0 m")))
//                        {
//                            var tempAlt = LocationArray[2].Replace("m", string.Empty).Trim();

//                            if (!string.IsNullOrEmpty(tempAlt) && tempAlt != "null")
//                                data.Add("Altitude", GetDecimalValue(tempAlt));
//                        }
//                        deviceData.Sensors.Add(new SensorRawData { Type = 2, Prefix = 0, Unit = 30, SensorUid = "Location", Data = data });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// // Orientation:337.72354 deg;-48.38163 deg;9.500354 deg
//                if (lineitem.StartsWith("Orientation:"))
//                {
//                    try
//                    {
//                        string orientaionString = lineitem.Replace("Orientation:", "").Trim();
//                        var orientationArray = orientaionString.Split(';');

//                        var tempX = orientationArray[0].Replace("deg", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = orientationArray[1].Replace("deg", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = orientationArray[2].Replace("deg", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 14, Unit = 30, Prefix = 0, SensorUid = "Orientation", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// // "Linear acceleration:0.9959502 m/s^2;-0.2672143 m/s^2;-1.3565793 m/s^2"
//                if (lineitem.StartsWith("Linear acceleration:"))
//                {
//                    try
//                    {
//                        string AccString = lineitem.Replace("Linear acceleration:", "").Trim();
//                        var AccArray = AccString.Split(';');

//                        var tempX = AccArray[0].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = AccArray[1].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = AccArray[2].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 7, Prefix = 0, Unit = 26, SensorUid = "Linear acceleration", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// // "Magnetic field uncalibrated:23.9 uT;-72.8 uT;-83.3 uT;20.0 uT;-56.8 uT;-38.4 uT"
//                //if (lineitem.StartsWith("Magnetic field uncalibrated:"))
//                //{
//                //    try
//                //    {
//                //        string magneticString = lineitem.Replace("Magnetic field uncalibrated:", "").Trim();
//                //        var magneticArray = magneticString.Split(';');

//                //        decimal u = GetDecimalValue(magneticArray[0].Replace("uT", ""));
//                //        decimal v = GetDecimalValue(magneticArray[1].Replace("uT", ""));
//                //        decimal w = GetDecimalValue(magneticArray[2].Replace("uT", ""));
//                //        decimal X = GetDecimalValue(magneticArray[3].Replace("uT", ""));
//                //        decimal Y = GetDecimalValue(magneticArray[4].Replace("uT", ""));
//                //        decimal Z = GetDecimalValue(magneticArray[5].Replace("uT", ""));

//                //        deviceData.Sensors.Add(new SensorRawData { Type = 8, Unit = 27, Prefix = -6, SensorUid = "Uncalibrated magnetic field", Data = new Dictionary<string, object> { { "U", u }, { "V", v }, { "W", w }, { "X", X }, { "Y", Y }, { "Z", Z } } });
//                //    }
//                //    catch (Exception ex)
//                //    {
//                //        gatewayData.LogException(ex);
//                //    }
//                //    continue;
//                //}
//                //////////////////////////////////////////////////////////// // "Gravity:1.0750341 m/s^2;7.3313065 m/s^2;6.4239073 m/s^2"
//                if (lineitem.StartsWith("Gravity:"))
//                {
//                    try
//                    {
//                        string magneticString = lineitem.Replace("Gravity:", "").Trim();
//                        var magneticArray = magneticString.Split(';');


//                        var tempX = magneticArray[0].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = magneticArray[1].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = magneticArray[2].Replace("m/s^2", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 7, Prefix = 0, Unit = 26, SensorUid = "Gravity", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// // "Rotation vector:0.415117;0.012238;0.158447;0.89578;246.0 rad"
//                //if (lineitem.StartsWith("Rotation vector:"))
//                //{
//                //    try
//                //    {
//                //        string magneticString = lineitem.Replace("Rotation vector:", "").Trim();
//                //        var magneticArray = magneticString.Split(';');

//                //        /* *********************************************************       Check this for unit and prefix               ************************/ 
//                //        decimal cos = decimal.Parse(magneticArray[0]);
//                //        decimal sin = decimal.Parse(magneticArray[1]);
//                //        decimal sin2 = decimal.Parse(magneticArray[2]);
//                //        decimal cos2 = decimal.Parse(magneticArray[3]);
//                //        decimal rad = decimal.Parse(magneticArray[4]);

//                //        deviceData.Sensors.Add(new SensorRawData { Type = 0, Prefix = 0, Unit = 0, SensorUid = "Rotation vector", Data = new Dictionary<string, object> { { "cos", cos }, { "sin", sin }, { "sin2", sin2 }, { "cos2", cos2 }, { "rad", rad } } });
//                //    }
//                //    catch (Exception ex)
//                //    {
//                //        gatewayData.LogException(ex);
//                //    }
//                //    continue;
//                //}
//                //////////////////////////////////////////////////////////// // "Gyroscope:0.10652645 rad/s;0.018109497 rad/s;-0.00958738 rad/s"
//                if (lineitem.StartsWith("Gyroscope:"))
//                {
//                    try
//                    {
//                        string gyroscopeString = lineitem.Replace("Gyroscope:", "").Trim();
//                        var gyroscopeArray = gyroscopeString.Split(';');

//                        var tempX = gyroscopeArray[0].Replace("rad/s", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempX) || tempX == "null") continue;

//                        var tempY = gyroscopeArray[1].Replace("rad/s", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempY) || tempY == "null") continue;

//                        var tempZ = gyroscopeArray[2].Replace("rad/s", string.Empty).Trim();
//                        if (string.IsNullOrEmpty(tempZ) || tempZ == "null") continue;

//                        deviceData.Sensors.Add(new SensorRawData { Type = 15, Prefix = 0, Unit = 7, SensorUid = "Gyroscope", Data = new Dictionary<string, decimal> { { "X", GetDecimalValue(tempX) }, { "Y", GetDecimalValue(tempY) }, { "Z", GetDecimalValue(tempZ) } } });
//                    }
//                    catch (Exception ex)
//                    {
//                        gatewayData.LogException(ex);
//                    }
//                    continue;
//                }
//                //////////////////////////////////////////////////////////// // "Gyroscope uncalibrated:-0.2130529 rad/s;0.058589544 rad/s;0.0010652645 rad/s;0.054328486 rad/s;-0.002130529 rad/s;-0.018109497 rad/s"
//                //if (lineitem.StartsWith("Gyroscope uncalibrated:"))
//                //{
//                //    try
//                //    {
//                //        string gyroscopeString = lineitem.Replace("Gyroscope uncalibrated:", "").Trim();
//                //        var gyroscopeArray = gyroscopeString.Split(';');

//                //        decimal U = decimal.Parse(gyroscopeArray[0].Replace("rad/s", ""));
//                //        decimal V = decimal.Parse(gyroscopeArray[1].Replace("rad/s", ""));
//                //        decimal W = decimal.Parse(gyroscopeArray[2].Replace("rad/s", ""));
//                //        decimal X = decimal.Parse(gyroscopeArray[3].Replace("rad/s", ""));
//                //        decimal Y = decimal.Parse(gyroscopeArray[4].Replace("rad/s", ""));
//                //        decimal Z = decimal.Parse(gyroscopeArray[5].Replace("rad/s", ""));

//                //        deviceData.Sensors.Add(new SensorRawData { Type = 16, Prefix = 0, Unit = 35, SensorUid = "Uncalibrated Gyroscope", Data = new Dictionary<string, object> { { "U", U }, { "V", V }, { "W", W }, { "X", X }, { "Y", Y }, { "Z", Z } } });
//                //    }
//                //    catch (Exception ex)
//                //    {
//                //        gatewayData.LogException(ex);
//                //    }
//                //    continue;
//                //}
//                //////////////////////////////////////////////////////////// // "Game rotation vector:0.306344;0.011708;-0.002678"
//                //if (lineitem.StartsWith("Game rotation vector:"))
//                //{
//                //    try
//                //    {
//                //        string gameString = lineitem.Replace("Game rotation vector:", "").Trim();
//                //        var gameArray = gameString.Split(';');

//                //        /* *********************************************************       Check this for unit and prefix               ************************/
//                //        decimal X = decimal.Parse(gameArray[0]);
//                //        decimal Y = decimal.Parse(gameArray[1]);
//                //        decimal Z = decimal.Parse(gameArray[2]);

//                //        deviceData.Sensors.Add(new SensorRawData { Type = 0, Prefix = 0, Unit = 0, SensorUid = "Game rotation vector", Data = new Dictionary<string, object> { { "X", X }, { "Y", Y }, { "Z", Z } } });
//                //    }
//                //    catch (Exception ex)
//                //    {
//                //        gatewayData.LogException(ex);
//                //    }
//                //    continue;
//                //}
//            }
//        }

//        private decimal GetDecimalValue(string digit)
//        {
//            var a = decimal.Parse(digit.Trim(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.Any, CultureInfo.InvariantCulture);
//            return a;
//        }
//    }
//}

