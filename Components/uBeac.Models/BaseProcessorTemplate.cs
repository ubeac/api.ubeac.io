using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
// todo: what is this doing here!!!!!!
namespace uBeac.Models
{
    public class BaseProcessorTemplate
    {
        public static Dictionary<string, bool> IdDefs;
        public static Dictionary<string, bool> TimeStampDefs;
        public static Dictionary<string, bool> DataDefs;
        public static Dictionary<string, bool> SensorDefs;
        public static Dictionary<string, bool> DateTimeDefs;
        public static Dictionary<string, bool> UnitDefs;
        public static Dictionary<string, bool> TypeDefs;
        public static Dictionary<string, bool> PrefixDefs;

        public BaseProcessorTemplate()
        {
            if (IdDefs is null)
                IdDefs = new Dictionary<string, bool> { { "id", false }, { "uid", false }, { "name", false }, { "mac", false }, { "title", false }, { "ip", false } };

            if (TimeStampDefs is null)
                TimeStampDefs = new Dictionary<string, bool> { { "ts", false }, { "timestamp", false } };

            if (DateTimeDefs is null)
                DateTimeDefs = new Dictionary<string, bool> { { "dt", false }, { "date", false }, { "datetime", false }, { "time", false } };

            if (DataDefs is null)
                DataDefs = new Dictionary<string, bool> { { "data", false }, { "item", false }, { "items", false }, { "value", false }, { "values", false } };

            if (SensorDefs is null)
                SensorDefs = new Dictionary<string, bool> { { "data", false }, { "item", false }, { "items", false }, { "value", false }, { "values", false }, { "sensor", false }, { "sensors", false } };

            if (UnitDefs is null)
                UnitDefs = new Dictionary<string, bool> { { "unit", false } };

            if (TypeDefs is null)
                TypeDefs = new Dictionary<string, bool> { { "type", false } };

            if (PrefixDefs is null)
                PrefixDefs = new Dictionary<string, bool> { { "prefix", false } };
        }

        public static void ExtractRawDevices(GatewayData gatewayData, Dictionary<string, object> rawDic)
        {
            var deviceRawData = new DeviceRawData { IsValid = true };
            var deviceData = new BaseProcessorTemplate();

            foreach (var key in rawDic.Keys)
            {
                deviceRawData.GatewayId = gatewayData.GatewayId;

                if (IdDefs.ContainsKey(key.ToLower()))
                    deviceRawData.Uid = rawDic[key].ToString();

                if (TimeStampDefs.ContainsKey(key.ToLower()))
                {
                    var timeStamp = rawDic[key].ToString();
                    if (timeStamp.Length > 10)
                        deviceRawData.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeStamp)).UtcDateTime;
                    else
                        deviceRawData.DateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timeStamp)).UtcDateTime;
                }

                if (DateTimeDefs.ContainsKey(key.ToLower()))
                {
                    deviceRawData.DateTime = DateTime.Parse(rawDic[key].ToString()).ToUniversalTime();
                }

                if (SensorDefs.ContainsKey(key.ToLower()))
                {
                    var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(rawDic[key].ToString());

                    foreach (var item in list)
                    {
                        var sensorData = new SensorRawData();
                        sensorData.DateTime = deviceRawData.DateTime;
                        ProcessSensorData(gatewayData, item, deviceRawData, sensorData, true);

                        if (string.IsNullOrEmpty(sensorData.SensorUid))
                            sensorData.SensorUid = Enum.GetName(typeof(SensorTypes), sensorData.Type);

                        if (sensorData.Data.Count > 0)
                            deviceRawData.Sensors.Add(sensorData);
                    }
                }
            }

            gatewayData.RawDevices.Add(deviceRawData);
        }

        public static void ProcessSensorData(GatewayData gatewayData, Dictionary<string, object> rawDic, DeviceRawData deviceRawData, SensorRawData sensorRawData, bool isMultiple)
        {
            try
            {
                foreach (var key in rawDic.Keys)
                {
                    if (IdDefs.ContainsKey(key.ToLower()))
                    {
                        if (isMultiple)
                            sensorRawData.SensorUid = rawDic[key].ToString();

                        if (string.IsNullOrEmpty(deviceRawData.Uid))
                            deviceRawData.Uid = rawDic[key].ToString();
                        continue;
                    }

                    if (TimeStampDefs.ContainsKey(key.ToLower()))
                    {
                        var timeStamp = rawDic[key].ToString();
                        if (timeStamp.Length > 10)
                            deviceRawData.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeStamp)).UtcDateTime;
                        else
                            deviceRawData.DateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timeStamp)).UtcDateTime;
                        continue;
                    }

                    if (DateTimeDefs.ContainsKey(key.ToLower()))
                    {                        
                        sensorRawData.DateTime = DateTime.Parse(rawDic[key].ToString()).ToUniversalTime();
                        continue;
                    }

                    if (TypeDefs.ContainsKey(key.ToLower()))
                    {
                        sensorRawData.Type = (SensorTypes)Convert.ToInt16(rawDic[key]);                        
                        if (!isMultiple)
                            sensorRawData.SensorUid = Enum.GetName(typeof(SensorTypes), Convert.ToInt16(rawDic[key])) ?? deviceRawData.Uid ?? rawDic[key].ToString();

                        continue;
                    }

                    if (UnitDefs.ContainsKey(key.ToLower()))
                    {
                        sensorRawData.Unit = (SensorUnits)Convert.ToInt16(rawDic[key]);
                        continue;
                    }

                    if (PrefixDefs.ContainsKey(key.ToLower()))
                    {
                        sensorRawData.Prefix = (SensorPrefixes)Convert.ToInt16(rawDic[key]);
                        continue;
                    }

                    if (DataDefs.ContainsKey(key.ToLower()))
                    {
                        try
                        {
                            if (rawDic[key] is Newtonsoft.Json.Linq.JObject)
                            {
                                sensorRawData.Data = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(rawDic[key].ToString());

                                bool invalidKey = false;
                                foreach (var data in sensorRawData.Data.ToList())
                                {
                                    if (data.Key.Contains("."))
                                    {
                                        gatewayData.Exceptions.Add(string.Format("Invalid name \"{0}\" for sensor value. Using \'.\' is not allowed in sensor value name.", data.Key));
                                        invalidKey = true;
                                    }

                                    //break;
                                }
                                if (invalidKey)
                                    sensorRawData.Data.Clear();
                            }
                            else if (decimal.TryParse(rawDic[key].ToString(), out decimal value))
                                sensorRawData.Data = new Dictionary<string, decimal> { { "value", value } };
                            else
                            {
                                sensorRawData.Data = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(GetGpsData(rawDic[key].ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            gatewayData.Exceptions.Add(string.Format("Cannot convert value {0} to decimal", rawDic[key].ToString()));
                        }
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                gatewayData.LogException("Exception in processing SensorData");
            }
        }

        private static string GetGpsData(string gpsString)
        {
            try
            {
                var gpsParts = gpsString.Split(',');

                decimal tempLat, tempLong, latitude, longitude;

                if (gpsParts[0].ToUpper() == "$GPRMC")
                {
                    tempLat = ExtractCoordinates(gpsParts[3].Trim());
                    tempLong = ExtractCoordinates(gpsParts[5].Trim());

                    latitude = gpsParts[4].Trim().ToLower() == "n" ? tempLat : -tempLat;
                    longitude = gpsParts[6].Trim().ToLower() == "e" ? tempLong : -tempLong;
                }
                else
                {
                    tempLat = ExtractCoordinates(gpsParts[2].Trim());
                    tempLong = ExtractCoordinates(gpsParts[4].Trim());

                    latitude = gpsParts[3].Trim().ToLower() == "n" ? tempLat : -tempLat;
                    longitude = gpsParts[5].Trim().ToLower() == "e" ? tempLong : -tempLong;
                }

                return JsonConvert.SerializeObject(new GpsData(latitude, longitude));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static decimal ExtractCoordinates(string coordinate)
        {
            var parts = coordinate.Split('.');

            // Getting the last 2 digits before comma to calculate minutes
            var strMinute = parts[0].Substring(parts[0].Length - 2, 2) + "." + parts[1];
            var minute = double.Parse(strMinute) / 60;

            // getting the digits before minute string to calculate degree
            var degree = Convert.ToInt32(parts[0].Substring(0, parts[0].Length - 2));

            return (decimal)(degree + minute);
        }

        private class GpsData
        {
            public decimal latitude { get; set; }
            public decimal longitude { get; set; }

            public GpsData(decimal _lat, decimal _long)
            {
                latitude = _lat;
                longitude = _long;
            }
        }
    }
}
