using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class CustomJsonGatewayProcessor : IProcessor
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
                var deviceRawDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(gatewayData.Body);

                var deviceRawData = new DeviceRawData { IsValid = true };
                new BaseProcessorTemplate();
                new SesnorDefDicts();

                var sensorNamesDic = new Dictionary<string, Tuple<SensorTypes, SensorUnits, SensorPrefixes>>
                {
                    {"Pressure", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Pressure, SensorUnits.Pascal, SensorPrefixes.Hundred) },
                    {"Proximity", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Proximity, SensorUnits.Meter, SensorPrefixes.One) },
                    {"Illuminance", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Illuminance, SensorUnits.Lux, SensorPrefixes.One) },
                    {"Counter", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Counter ,SensorUnits.Count,SensorPrefixes.One)},
                    {"Temperature", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Temperature ,SensorUnits.Centigrade,SensorPrefixes.One)},
                    {"Distance", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Distance ,SensorUnits.Meter,SensorPrefixes.One)},
                    {"Voltage", new Tuple<SensorTypes, SensorUnits, SensorPrefixes>(SensorTypes.Voltage ,SensorUnits.Volt,SensorPrefixes.One)},
                };

                var currentTime = DateTime.UtcNow;
                deviceRawData.DateTime = currentTime;

                foreach (var key in deviceRawDataDict.Keys)
                {
                    if (BaseProcessorTemplate.IdDefs.ContainsKey(key.ToLower()))
                        deviceRawData.Uid = deviceRawDataDict[key].ToString();

                    if (BaseProcessorTemplate.TimeStampDefs.ContainsKey(key.ToLower()))
                    {
                        var timeStamp = deviceRawDataDict[key].ToString();
                        if (timeStamp.Length > 10)
                            deviceRawData.DateTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeStamp)).UtcDateTime;
                        else
                            deviceRawData.DateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timeStamp)).UtcDateTime;
                    }

                    if (BaseProcessorTemplate.DateTimeDefs.ContainsKey(key.ToLower()))
                    {
                        deviceRawData.DateTime = DateTime.Parse(deviceRawDataDict[key].ToString()).ToUniversalTime();
                    }

                    if (BaseProcessorTemplate.SensorDefs.ContainsKey(key.ToLower()))
                    {
                        var sensorDataList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(deviceRawDataDict[key].ToString());
                        decimal? rssi = null;
                        decimal? txPower = null;

                        foreach (var sensorDataDict in sensorDataList)
                        {                            
                            var dateTime = GetDateTime(sensorDataDict, deviceRawData.DateTime, currentTime);

                            foreach (var sensorDataKey in sensorDataDict.Keys)
                            {
                                var sensorValue = GetValues(sensorDataDict[sensorDataKey]);
                                if (sensorValue != null)
                                {
                                    if (SesnorDefDicts.TemperatureDefs.ContainsKey(sensorDataKey.ToLower()))                                    
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Temperature"].Item1.ToString(), dateTime, sensorNamesDic["Temperature"].Item1, sensorNamesDic["Temperature"].Item2, sensorNamesDic["Temperature"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.DistanceDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Distance"].Item1.ToString(), dateTime, sensorNamesDic["Distance"].Item1, sensorNamesDic["Distance"].Item2, sensorNamesDic["Distance"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.CounterDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Counter"].Item1.ToString(), dateTime, sensorNamesDic["Counter"].Item1, sensorNamesDic["Counter"].Item2, sensorNamesDic["Counter"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.IlluminanceDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Illuminance"].Item1.ToString(), dateTime, sensorNamesDic["Illuminance"].Item1, sensorNamesDic["Illuminance"].Item2, sensorNamesDic["Illuminance"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.PressureDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Pressure"].Item1.ToString(), dateTime, sensorNamesDic["Pressure"].Item1, sensorNamesDic["Pressure"].Item2, sensorNamesDic["Pressure"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.ProximityDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Proximity"].Item1.ToString(), dateTime, sensorNamesDic["Proximity"].Item1, sensorNamesDic["Proximity"].Item2, sensorNamesDic["Proximity"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.VoltageDefs.ContainsKey(sensorDataKey.ToLower()))
                                        deviceRawData.Sensors.Add(new SensorRawData(sensorNamesDic["Voltage"].Item1.ToString(), dateTime, sensorNamesDic["Voltage"].Item1, sensorNamesDic["Voltage"].Item2, sensorNamesDic["Voltage"].Item3, (decimal)sensorValue));

                                    if (SesnorDefDicts.RssiDefs.ContainsKey(sensorDataKey.ToLower()))
                                        rssi = (decimal)sensorValue;

                                    if (SesnorDefDicts.TxPowerDefs.ContainsKey(sensorDataKey.ToLower()))
                                        txPower = (decimal)sensorValue;
                                }
                            }

                            if (rssi != null && txPower != null)
                            {
                                var values = new Dictionary<string, decimal> { { "rssi", (decimal)rssi }, { "txPower", (decimal)txPower } };
                                deviceRawData.Sensors.Add(new SensorRawData(SensorTypes.SignalStrength.ToString(), dateTime, SensorTypes.SignalStrength, SensorUnits.DecibelMilliwatts, SensorPrefixes.One, values));
                            }
                        }
                    }
                }

                gatewayData.RawDevices.Add(deviceRawData);

                #region Old Code

                //var json = JsonConvert.DeserializeObject<ParentLevelData>(gatewayData.Body);

                //var _uid = "";
                //if (string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(json.id)) { _uid = json.id; }
                //if (string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(json.name)) { _uid = json.name; }
                //if (string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(json.mac)) { _uid = json.mac; }
                //if (string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(json.uid)) { _uid = json.uid; }
                //if (string.IsNullOrEmpty(_uid) && !string.IsNullOrEmpty(json.ip)) { _uid = json.ip; }

                //var _dt = "";
                //if (string.IsNullOrEmpty(_dt) && !string.IsNullOrEmpty(json.dt)) { _dt = json.dt; }
                //if (string.IsNullOrEmpty(_dt) && !string.IsNullOrEmpty(json.datetime)) { _dt = json.datetime; }
                //if (string.IsNullOrEmpty(_dt) && !string.IsNullOrEmpty(json.date)) { _dt = json.date; }
                //if (string.IsNullOrEmpty(_dt) && !string.IsNullOrEmpty(json.timestamp)) { _dt = json.timestamp; }
                //if (string.IsNullOrEmpty(_dt) && !string.IsNullOrEmpty(json.ts)) { _dt = json.ts; }

                //var _tags = new List<TagsData>();
                //if (json.tags != null) if (json.tags.Count > 0) { _tags.AddRange(json.tags); }
                //if (json.data != null) if (json.data.Count > 0) { _tags.AddRange(json.data); }
                //if (json.sensors != null) if (json.sensors.Count > 0) { _tags.AddRange(json.sensors); }
                //if (json.beacons != null) if (json.beacons.Count > 0) { _tags.AddRange(json.beacons); }
                //if (json.items != null) if (json.items.Count > 0) { _tags.AddRange(json.items); }

                //foreach (var _tag in _tags)
                //{
                //    var _mac = "";
                //    if (string.IsNullOrEmpty(_mac) && !string.IsNullOrEmpty(_tag.id)) { _mac = _tag.id; }
                //    if (string.IsNullOrEmpty(_mac) && !string.IsNullOrEmpty(_tag.name)) { _mac = _tag.name; }
                //    if (string.IsNullOrEmpty(_mac) && !string.IsNullOrEmpty(_tag.mac)) { _mac = _tag.mac; }
                //    if (string.IsNullOrEmpty(_mac) && !string.IsNullOrEmpty(_tag.uid)) { _mac = _tag.uid; }
                //    if (string.IsNullOrEmpty(_mac) && !string.IsNullOrEmpty(_tag.ip)) { _mac = _tag.ip; }

                //    var _datetime = "";
                //    if (string.IsNullOrEmpty(_datetime) && !string.IsNullOrEmpty(_tag.ts)) { _datetime = _tag.ts; }
                //    if (string.IsNullOrEmpty(_datetime) && !string.IsNullOrEmpty(_tag.dt)) { _datetime = _tag.dt; }
                //    if (string.IsNullOrEmpty(_datetime) && !string.IsNullOrEmpty(_tag.date)) { _datetime = _tag.date; }
                //    if (string.IsNullOrEmpty(_datetime) && !string.IsNullOrEmpty(_tag.datetime)) { _datetime = _tag.datetime; }
                //    if (string.IsNullOrEmpty(_datetime) && !string.IsNullOrEmpty(_tag.timestamp)) { _datetime = _tag.timestamp; }

                //    var _rssi = "";
                //    if (string.IsNullOrEmpty(_rssi) && !string.IsNullOrEmpty(_tag.rssi)) { _rssi = _tag.rssi; }
                //    if (string.IsNullOrEmpty(_rssi) && !string.IsNullOrEmpty(_tag.rx)) { _rssi = _tag.rx; }

                //    var _tx = "";
                //    if (string.IsNullOrEmpty(_tx) && !string.IsNullOrEmpty(_tag.tx)) { _tx = _tag.tx; }
                //    if (string.IsNullOrEmpty(_tx) && !string.IsNullOrEmpty(_tag.power)) { _tx = _tag.power; }
                //    if (string.IsNullOrEmpty(_tx) && !string.IsNullOrEmpty(_tag.txpower)) { _tx = _tag.txpower; }

                //    var _temperature = "";
                //    if (string.IsNullOrEmpty(_temperature) && !string.IsNullOrEmpty(_tag.tmp)) { _temperature = _tag.tmp; }
                //    if (string.IsNullOrEmpty(_temperature) && !string.IsNullOrEmpty(_tag.temp)) { _temperature = _tag.temp; }
                //    if (string.IsNullOrEmpty(_temperature) && !string.IsNullOrEmpty(_tag.temperature)) { _temperature = _tag.temperature; }

                //    var _distance = "";
                //    if (string.IsNullOrEmpty(_distance) && !string.IsNullOrEmpty(_tag.dis)) { _distance = _tag.dis; }
                //    if (string.IsNullOrEmpty(_distance) && !string.IsNullOrEmpty(_tag.dist)) { _distance = _tag.dist; }
                //    if (string.IsNullOrEmpty(_distance) && !string.IsNullOrEmpty(_tag.distance)) { _distance = _tag.distance; }

                //    var _voltage = "";
                //    if (string.IsNullOrEmpty(_voltage) && !string.IsNullOrEmpty(_tag.v)) { _voltage = _tag.v; }
                //    if (string.IsNullOrEmpty(_voltage) && !string.IsNullOrEmpty(_tag.volts)) { _voltage = _tag.volts; }
                //    if (string.IsNullOrEmpty(_voltage) && !string.IsNullOrEmpty(_tag.voltage)) { _voltage = _tag.voltage; }

                //    var _proximity = "";
                //    if (string.IsNullOrEmpty(_proximity) && !string.IsNullOrEmpty(_tag.prx)) { _proximity = _tag.prx; }
                //    if (string.IsNullOrEmpty(_proximity) && !string.IsNullOrEmpty(_tag.prox)) { _proximity = _tag.prox; }
                //    if (string.IsNullOrEmpty(_proximity) && !string.IsNullOrEmpty(_tag.proxim)) { _proximity = _tag.proxim; }
                //    if (string.IsNullOrEmpty(_proximity) && !string.IsNullOrEmpty(_tag.proximity)) { _proximity = _tag.proximity; }

                //    var _pressure = "";
                //    if (string.IsNullOrEmpty(_pressure) && !string.IsNullOrEmpty(_tag.pres)) { _pressure = _tag.pres; }
                //    if (string.IsNullOrEmpty(_pressure) && !string.IsNullOrEmpty(_tag.press)) { _pressure = _tag.press; }
                //    if (string.IsNullOrEmpty(_pressure) && !string.IsNullOrEmpty(_tag.pressure)) { _pressure = _tag.pressure; }

                //    var _lux = "";
                //    if (string.IsNullOrEmpty(_lux) && !string.IsNullOrEmpty(_tag.lum)) { _lux = _tag.lum; }
                //    if (string.IsNullOrEmpty(_lux) && !string.IsNullOrEmpty(_tag.lux)) { _lux = _tag.lux; }
                //    if (string.IsNullOrEmpty(_lux) && !string.IsNullOrEmpty(_tag.ilu)) { _lux = _tag.ilu; }
                //    if (string.IsNullOrEmpty(_lux) && !string.IsNullOrEmpty(_tag.illum)) { _lux = _tag.illum; }
                //    if (string.IsNullOrEmpty(_lux) && !string.IsNullOrEmpty(_tag.illuminance)) { _lux = _tag.illuminance; }

                //    var _steps = "";
                //    if (string.IsNullOrEmpty(_steps) && !string.IsNullOrEmpty(_tag.step)) { _steps = _tag.step; }
                //    if (string.IsNullOrEmpty(_steps) && !string.IsNullOrEmpty(_tag.steps)) { _steps = _tag.steps; }
                //    if (string.IsNullOrEmpty(_steps) && !string.IsNullOrEmpty(_tag.stepcount)) { _steps = _tag.stepcount; }
                //    if (string.IsNullOrEmpty(_steps) && !string.IsNullOrEmpty(_tag.stepcounts)) { _steps = _tag.stepcounts; }


                //    var beaconDevice = new DeviceRawData();
                //    gatewayData.RawDevices.Add(beaconDevice);

                //    beaconDevice.Uid = _mac;
                //    beaconDevice.DateTime = GetUTCfromTS(_datetime, gatewayData.DateTime);
                //    beaconDevice.IsValid = true;

                //    ///////////////// 

                //    if (!string.IsNullOrEmpty(_rssi) && !string.IsNullOrEmpty(_tx))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 17,
                //                Unit = 23,
                //                Prefix = 0,
                //                SensorUid = "SignalStrength",
                //                Data = new Dictionary<string, decimal> { { "RSSI", decimal.Parse(_rssi) }, { "TxPower", decimal.Parse(_tx) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_temperature))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 4,
                //                Prefix = 0,
                //                Unit = 1,
                //                SensorUid = "Temperature",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_temperature) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_distance))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 3,
                //                Prefix = 0,
                //                Unit = 5,
                //                SensorUid = "Distance",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_distance) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_voltage))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                SensorUid = "Battery",
                //                Type = 6,
                //                Prefix = 0,
                //                Unit = 25,
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_voltage) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_proximity))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 10,
                //                Prefix = -2,
                //                Unit = 5,
                //                SensorUid = "Proximity",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_proximity) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_pressure))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 12,
                //                Prefix = 2,
                //                Unit = 19,
                //                SensorUid = "Pressure",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_pressure) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_lux))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 11,
                //                Prefix = 0,
                //                Unit = 24,
                //                SensorUid = "Illuminance",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_lux) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //    ////////////////////////
                //    if (!string.IsNullOrEmpty(_steps))
                //    {
                //        try
                //        {
                //            beaconDevice.Sensors.Add(new SensorRawData
                //            {
                //                Type = 13,
                //                Unit = 6,
                //                Prefix = 0,
                //                SensorUid = "Counter",
                //                Data = new Dictionary<string, decimal> { { "Value", decimal.Parse(_steps) } }
                //            });
                //        }
                //        catch (Exception ex)
                //        {
                //            gatewayData.LogException(ex);
                //        }
                //    }
                //}
                #endregion

            }
            catch (Exception ex)
            {
                gatewayData.Exceptions.Add(ex.Message);
            }
        }

        private decimal? GetValues(object sensorValues)
        {
            if (decimal.TryParse(sensorValues.ToString(), out decimal value))
                return value;

            return null;
        }

        private static DateTime GetDateTime(Dictionary<string, object> sensorData, DateTime deviceDateTime, DateTime currentTime)
        {
            foreach (var key in sensorData.Keys)
            {
                if (BaseProcessorTemplate.TimeStampDefs.ContainsKey(key.ToLower()))
                {
                    return DateTime.Parse(sensorData[key].ToString()).ToUniversalTime(); ;
                }
            }
            if (deviceDateTime != null)
                return deviceDateTime;
            else
                return currentTime;
        }

        private class SesnorDefDicts
        {
            public static Dictionary<string, bool> RssiDefs;
            public static Dictionary<string, bool> TxPowerDefs;
            public static Dictionary<string, bool> TemperatureDefs;
            public static Dictionary<string, bool> DistanceDefs;
            public static Dictionary<string, bool> VoltageDefs;
            public static Dictionary<string, bool> ProximityDefs;
            public static Dictionary<string, bool> PressureDefs;
            public static Dictionary<string, bool> IlluminanceDefs;
            public static Dictionary<string, bool> CounterDefs;

            public SesnorDefDicts()
            {
                if (RssiDefs is null)
                    RssiDefs = new Dictionary<string, bool> { { "rssi", false }, { "rx", false } };

                if (TxPowerDefs is null)
                    TxPowerDefs = new Dictionary<string, bool> { { "tx", false }, { "power", false }, { "txpower", false } };

                if (TemperatureDefs is null)
                    TemperatureDefs = new Dictionary<string, bool> { { "temperature", false }, { "temp", false }, { "tmp", false } };

                if (DistanceDefs is null)
                    DistanceDefs = new Dictionary<string, bool> { { "distance", false }, { "dist", false }, { "dis", false } };

                if (VoltageDefs is null)
                    VoltageDefs = new Dictionary<string, bool> { { "voltage", false }, { "volts", false }, { "volt", false }, { "v", false } };

                if (ProximityDefs is null)
                    ProximityDefs = new Dictionary<string, bool> { { "proximity", false }, { "prx", false }, { "prox", false }, { "proxim", false } };

                if (PressureDefs is null)
                    PressureDefs = new Dictionary<string, bool> { { "pressure", false }, { "press", false }, { "pres", false } };

                if (IlluminanceDefs is null)
                    IlluminanceDefs = new Dictionary<string, bool> { { "illuminance", false }, { "lum", false }, { "lux", false }, { "ilu", false }, { "illum", false } };

                if (CounterDefs is null)
                    CounterDefs = new Dictionary<string, bool> { { "stepcounts", false }, { "stepcount", false }, { "count", false }, { "counter", false }, { "step", false }, { "steps", false } };
            }
        }

    } //class DefaultProcessor

} // namespace 
