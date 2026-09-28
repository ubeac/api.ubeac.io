
using System;
using uBeac.Models;

namespace uBeac.IoT.Processing
{
    public class CustomCsvGatewayProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {
            var stringContent = gatewayData.Body;
            var splittedLines = stringContent.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var date = DateTime.UtcNow;
            var deviceRawData = new DeviceRawData
            {
                DateTime = date,
                IsValid = true,
                GatewayId = gatewayData.GatewayId,
                Uid = "aa:bb:cc:dd:ee",
                Sensors = new System.Collections.Generic.List<SensorRawData>()
            };

            // sensorUid, type, unit, prefix, data
            // "temperature,4,2,0,25"

            for (int i = 0; i < splittedLines.Length; i++)
            {
                var row = splittedLines[i];
                var dataArray = row.Split(',', StringSplitOptions.None);

                if (dataArray.Length != 5)
                    return;

                var sensorRawData = new SensorRawData(dataArray[0], date, (SensorTypes)Convert.ToInt32(dataArray[1]), (SensorUnits)Convert.ToInt32(dataArray[2]),
                                    (SensorPrefixes)Convert.ToInt32(dataArray[3]), decimal.Parse(dataArray[4]));

                deviceRawData.Sensors.Add(sensorRawData);
            }

            gatewayData.RawDevices.Add(deviceRawData);
        }
    }
}





//using System;
//using System.Collections.Generic;
//using System.Linq;
//using uBeac.Models;

//namespace uBeac.IoT.Processing
//{
//    public class CustomCsvGatewayProcessor : IProcessor
//    {
//        #region main block 

//        public void Process(GatewayData gatewayData)
//        {
//            ////////////////////////////////////////////////////////////////////////
//            /**********************   based on an email from AP ********************
//            0 - header: Starts with +CGP and ends with 32
//            1 - tic time: is a UTC time HH:MM:SS:ms
//            2 - GPS status: One character it could be only 'A' -> Valid or 'V' -> Invalid
//            3 - latitude: Decimal data
//            4 - north or south: One character it could be only 'N' -> North or 'S' -> South
//            5 - longitude: Decimal data
//            6 - east or west: One character it could be only 'E' -> East or 'W' -> West
//            7 - speed over ground: ?
//            8 - course over ground: ?
//            9 - date: Always 6 digits that shows dd:mm:yy
//            10 - magnetic variation: We do not use it
//            11 - east west indicator: We do not use it
//            12 - mode: ?
//            13 - Sun rise time: Local time for sun rise
//            14 - Sun set time: Local time for sun set
//            15 - right lamps states: 65 character. starts with W80, next 2 character shows which item has fault. Each 2 character are together. 
//                 First character is used for 4 lamps and second one is used for 4 relays. First character is 4 bits that each bit is a lamp. If each lamp becomes on or off the related bit will be 0 or 1.
//                 same as lamps, that will happen for relays. Each lamp has a relay which if a relay is 0 the related lamp should be 1.
//            16 - left lamps states: Same as right lamp

//            SampleData: "+CGPSINF: 32,124535.000,A,0029.8019,N,00052.4180,E,14.835,272.08,021118,,,A,06:13:00,17:13:00,W800287C34BF0F12F000000000000000000000000000000000000000000000000,W800278C34BF0F12F000000000000000000000000000000000000000000000000"
//            */
//            ////////////////////////////////////////////////////////////////////////

//            try
//            {
//                var stringContent = gatewayData.Body;
//                var splittedLines = stringContent.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

//                for (int i = 0; i < splittedLines.Length; i++)
//                {
//                    var row = splittedLines[i];
//                    var dataArray = row.Split(',', StringSplitOptions.None);   //keep empty 

//                    if (dataArray.Length != 17)
//                    {
//                        gatewayData.LogException("Invalid row data in row number:" + (i + 1).ToString());
//                        continue;
//                    }

//                    var dateTime = ParseDateTime(dataArray[9], dataArray[1]);
//                    if (!dateTime.HasValue)
//                        dateTime = gatewayData.DateTime;


//                    // adding GPS data
//                    var gpsDeviceData = GetGpsDeviceData(dataArray, gatewayData, i, dateTime.Value);
//                    if (gpsDeviceData is null)
//                    {
//                        gatewayData.LogException("Invalid GPS Data:" + (i + 1).ToString());
//                        continue;
//                    }
//                    gatewayData.RawDevices.Add(gpsDeviceData);

//                    gatewayData.RawDevices.AddRange(PopulateColumnData(dataArray, gatewayData, i, dateTime.Value, true));
//                    gatewayData.RawDevices.AddRange(PopulateColumnData(dataArray, gatewayData, i, dateTime.Value, false));
//                }
//            }
//            catch (Exception ex)
//            {
//                gatewayData.LogException(ex);
//            }
//        }

//        // timeString : 124535.000
//        // dateString: 021118
//        private DateTime? ParseDateTime(string dateString, string timeString)
//        {

//            if (dateString.Length != 6)
//                return null;

//            if (timeString.Length != 10)
//                return null;

//            int year, month, day, hour, minute, second, milliSecond;

//            if (!int.TryParse(dateString.Substring(0, 2), out day))
//                return null;

//            if (!int.TryParse(dateString.Substring(2, 2), out month))
//                return null;

//            if (!int.TryParse(dateString.Substring(4, 2), out year))
//                return null;


//            if (!int.TryParse(timeString.Substring(0, 2), out hour))
//                return null;

//            if (!int.TryParse(timeString.Substring(2, 2), out minute))
//                return null;

//            if (!int.TryParse(timeString.Substring(4, 2), out second))
//                return null;

//            if (!int.TryParse(timeString.Substring(7, 3), out milliSecond))
//                return null;

//            year += 2000;


//            try
//            {
//                return new DateTime(year, month, day, hour, minute, second, milliSecond, DateTimeKind.Utc);
//            }
//            catch (Exception)
//            {

//                return null;
//            }

//        }

//        #endregion

//        #region Lamps and Relays

//        private List<DeviceRawData> PopulateColumnData(string[] dataArray, GatewayData gatewayData, int rowIndex, DateTime dateTime, bool isRight)
//        {
//            var result = new List<DeviceRawData>();

//            int deviceCount;
//            var dataString = isRight ? dataArray[15] : dataArray[16];
//            var sideName = isRight ? "Right" : "Left";

//            if (dataString.Length != 65)
//            {
//                gatewayData.LogException("Invalid " + sideName + " data:" + (rowIndex + 1).ToString());
//                return result;
//            }

//            try
//            {
//                deviceCount = int.Parse(dataString.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) - 1;
//            }
//            catch (Exception)
//            {
//                gatewayData.LogException("Invalid Fault number:" + (rowIndex + 1).ToString());
//                return result;
//            }

//            if (deviceCount < 1)
//            {
//                gatewayData.LogException("There is no valid data (fault = 0 or 1) in row:" + (rowIndex + 1).ToString());
//                return result;
//            }

//            for (int deviceIndex = 0; deviceIndex < deviceCount; deviceIndex++)
//            {
//                var deviceData = new DeviceRawData()
//                {
//                    Uid = "Column " + (deviceIndex + 1).ToString(),
//                    DateTime = dateTime,
//                    IsValid = true
//                };
//                result.Add(deviceData);

//                var binaryLampValues = GetStatusForSensors(dataString.Substring(5 + deviceIndex, 1));
//                var rightLampSensorData = new SensorRawData
//                {
//                    SensorUid = sideName + "Lamps",
//                    DateTime = deviceData.DateTime
//                };
//                deviceData.Sensors.Add(rightLampSensorData);

//                for (int j = 0; j < binaryLampValues.Length; j++)
//                {
//                    rightLampSensorData.Data.Add((j + 1).ToString(), binaryLampValues[j]);
//                }

//                var binaryRelayValues = GetStatusForSensors(dataString.Substring(6 + deviceIndex, 1));
//                var rightRelaySensorData = new SensorRawData
//                {
//                    SensorUid = sideName + "Relays",
//                    DateTime = deviceData.DateTime
//                };
//                deviceData.Sensors.Add(rightRelaySensorData);

//                for (int j = 0; j < binaryRelayValues.Length; j++)
//                {
//                    rightRelaySensorData.Data.Add((j + 1).ToString(), binaryRelayValues[j]);
//                }

//            }

//            return result;

//        }

//        private short[] GetStatusForSensors(string hexChar)
//        {
//            var charArray = Convert.ToString(Convert.ToInt16(hexChar, 16), 2).PadLeft(4, '0').ToCharArray();
//            return charArray.Select(x => short.Parse(x.ToString())).ToArray();
//        }

//        #endregion

//        #region GPSDataExtraction

//        private DeviceRawData GetGpsDeviceData(string[] dataArray, GatewayData gatewayData, int rowIndex, DateTime dateTime)
//        {
//            var deviceData = new DeviceRawData()
//            {
//                DateTime = dateTime,
//                IsValid = true,
//                Uid = "Location"
//            };

//            var gpsSensorData = new SensorRawData()
//            {
//                DateTime = dateTime,
//                SensorUid = "Location",
//                Type = 2
//            };

//            var gpsStatus = GetGpsStatus(dataArray[2]);
//            if (!gpsStatus.HasValue)
//            {
//                gatewayData.LogException("Invalid GPS status in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            var gpsData = GetGpsData(dataArray);
//            if (!gpsData.HasValue)

//            {
//                gatewayData.LogException("Invalid GPS data in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            var gpsVerticalDir = GetVerticalDirection(dataArray[4]);
//            if (string.IsNullOrEmpty(gpsVerticalDir))
//            {
//                gatewayData.LogException("Invalid GPS Vertical Direction in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            var gpsHorizontalDir = GetHorizontalDirection(dataArray[6]);
//            if (string.IsNullOrEmpty(gpsHorizontalDir))
//            {
//                gatewayData.LogException("Invalid GPS Horizontal Direction in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            decimal speed;
//            if (!decimal.TryParse(dataArray[7], out speed))
//            {
//                gatewayData.LogException("Invalid GPS Speed in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            decimal course;
//            if (!decimal.TryParse(dataArray[8], out course))
//            {
//                gatewayData.LogException("Invalid GPS Course in row number:" + (rowIndex + 1).ToString());
//                return null;
//            }

//            gpsSensorData.Data.Add("Status", gpsStatus.Value);
//            gpsSensorData.Data.Add("Latitude", gpsData.Value.Key);
//            gpsSensorData.Data.Add("Longitude", gpsData.Value.Value);
//            gpsSensorData.Data.Add("Vertical", gpsStatus.Value);
//            gpsSensorData.Data.Add("Horizontal", gpsStatus.Value);
//            gpsSensorData.Data.Add("Speed", speed);
//            gpsSensorData.Data.Add("Course", course);
//            gpsSensorData.Data.Add("Mode", dataArray[12]);
//            gpsSensorData.Data.Add("Sunrise", dataArray[13]);
//            gpsSensorData.Data.Add("Sunset", dataArray[14]);

//            deviceData.Sensors.Add(gpsSensorData);
//            return deviceData;

//        }

//        private bool? GetGpsStatus(string gpsStatus)
//        {
//            switch (gpsStatus)
//            {
//                case "A":
//                    return true;
//                case "V":
//                    return false;
//                default:
//                    return null;
//            }
//        }

//        private KeyValuePair<decimal, decimal>? GetGpsData(string[] dataArray)
//        {
//            /////////////////////////////// 3 - latitude
//            /////////////////////////////// 5 - longitude
//            decimal lat = 0;
//            decimal lng = 0;

//            if (!decimal.TryParse(dataArray[3], out lat))
//                return null;

//            if (!decimal.TryParse(dataArray[5], out lng))
//                return null;

//            return new KeyValuePair<decimal, decimal>(lat, lng);

//        }

//        private string GetVerticalDirection(string data)
//        {
//            switch (data)
//            {
//                case "N":
//                    return "North";
//                case "S":
//                    return "South";
//                default:
//                    return string.Empty;
//            }
//        }

//        private string GetHorizontalDirection(string data)
//        {
//            switch (data)
//            {
//                case "E":
//                    return "East";
//                case "W":
//                    return "West";
//                default:
//                    return string.Empty;
//            }
//        }

//        #endregion

//    }
//}
