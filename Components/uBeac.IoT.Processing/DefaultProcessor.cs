/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using uBeac.Models;

using MsgPack.Serialization;
using System.IO;

namespace uBeac.IoT.Processing
{
    public class DefaultProcessor : IProcessor
    {
        public void Process(GatewayData gatewayData)
        {

        }

        //public void Process(gatewayData gatewayData)
        //{
        //    ////////////////////////////////////////////////////////////////////////
        //    /**********************   based on an email from AP ********************
        //    0 - header: Starts with +CGP and ends with 32
        //    1 - tic time: is a UTC time HH:MM:SS:ms
        //    2 - GPS status: One character it could be only 'A' -> Valid or 'V' -> Invalid
        //    3 - latitude: Decimal data
        //    4 - north or south: One character it could be only 'N' -> North or 'S' -> South
        //    5 - longitude: Decimal data
        //    6 - east or west: One character it could be only 'E' -> East or 'W' -> West
        //    7 - speed over ground: ?
        //    8 - course over ground: ?
        //    9 - date: Always 6 digits that shows dd:mm:yy
        //    10 - magnetic variation: We do not use it
        //    11 - east west indicator: We do not use it
        //    12 - mode: ?
        //    13 - Sun rise time: Local time for sun rise
        //    14 - Sun set time: Local time for sun set
        //    15 - right lamps states: 65 character. starts with W80, next 2 character shows which item has fault. Each 2 character are together. 
        //         First character is used for 4 lamps and second one is used for 4 relays. First character is 4 bits that each bit is a lamp. If each lamp becomes on or off the related bit will be 0 or 1.
        //         same as lamps, that will happen for relays. Each lamp has a relay which if a relay is 0 the related lamp should be 1.
        //    16 - left lamps states: Same as right lamp
        //    */
        //    ////////////////////////////////////////////////////////////////////////

        //    var stringContent = gatewayData.Body;
        //    var splittedLines = stringContent.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        //    foreach (var itm in splittedLines)
        //    {
        //        var dataArray = itm.Split(',', StringSplitOptions.None);   //keep empty 
        //        if (dataArray.Length != 17) { continue; }

        //        var beaconDevice = new DeviceRawData();
        //        gatewayData.RawDevices.Add(beaconDevice);

        //        var sensorData = new SensorRawData
        //        {
        //            Type = 0,
        //            SensorUid = "Custom",
        //            Data = new Dictionary<string, object>()
        //        };

        //        beaconDevice.Sensors.Add(sensorData);

        //        var currentDatetime = gatewayData.DateTime;
        //        string stringDate = "";
        //        string stringTime = "";

        //        ///////////////////////////////////////////// 0 - header
        //        beaconDevice.Uid = dataArray[0];
        //        /////////////////////////////// 1 - tic time
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(dataArray[9]))
        //            {
        //                var date = dataArray[9].ToString();
        //                //string stringDate = "";
        //                for (int i = 0; i < 6; i++)
        //                {
        //                    stringDate = stringDate.Insert(0, date.Substring(i, 2));
        //                    i++;
        //                    if (i < 4)
        //                        stringDate = stringDate.Insert(0, "/");
        //                    else
        //                        stringDate += " ";
        //                }

        //                if (dataArray[1].Length == 10)
        //                {
        //                    var time = dataArray[1].ToString().Split('.');
        //                    for (int i = 0; i < 6; i++)
        //                    {
        //                        stringTime += time[0].Substring(i, 2);
        //                        i++;
        //                        if (i < 4)
        //                            stringTime += ":";
        //                    }
        //                    stringTime = stringTime + "." + time[1];
        //                    currentDatetime = DateTime.Parse(stringDate + stringTime);
        //                }
        //                else
        //                {
        //                    var time = dataArray[1].ToString().Split('.');
        //                    for (int i = 0; i < 6; i++)
        //                    {
        //                        stringTime += time[0].Substring(i, 2);
        //                        i++;
        //                        if (i < 4)
        //                            stringTime += ":";
        //                    }
        //                    currentDatetime = DateTime.Parse(stringDate + stringTime);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            gatewayData.LogException(ex);
        //        }
        //        beaconDevice.DateTime = currentDatetime;
        //        /////////////////////////////// 2 - GPS status
        //        if (!string.IsNullOrEmpty(dataArray[2]))
        //        {
        //            try
        //            {
        //                var data = dataArray[2].ToString();
        //                if (data == "A" || data == "V")
        //                    sensorData.Data.Add("GPS Status", data);
        //                else
        //                    sensorData.Data.Add("GPS Status", "");
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 3 - latitude
        //        /////////////////////////////// 5 - longitude
        //        decimal lat = 0;
        //        decimal lng = 0;
        //        decimal.TryParse(dataArray[3], out lat);
        //        decimal.TryParse(dataArray[5], out lng);

        //        try
        //        {
        //            sensorData.Data.Add("GPS", new { Latitude = lat, Longitude = lng });
        //        }
        //        catch (Exception ex)
        //        {
        //            gatewayData.LogException(ex);
        //        }
        //        ////////////////////////////////// 4 - north or south
        //        if (!string.IsNullOrEmpty(dataArray[4]))
        //        {
        //            try
        //            {
        //                var data = dataArray[4].ToString();
        //                if (data == "N" || data == "S")
        //                    sensorData.Data.Add("Vertical Direction", data);
        //                else
        //                    sensorData.Data.Add("Vertical Direction", "");
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 6 - east or west
        //        if (!string.IsNullOrEmpty(dataArray[6]))
        //        {
        //            try
        //            {
        //                var data = dataArray[6].ToString();
        //                if (data == "W" || data == "E")
        //                    sensorData.Data.Add("Horizontal Direction", data);
        //                else
        //                    sensorData.Data.Add("Horizontal Direction", "");
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 7 - speed over ground
        //        if (!string.IsNullOrEmpty(dataArray[7]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Speed Over Ground", decimal.Parse(dataArray[7]));
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 8 - course over ground
        //        if (!string.IsNullOrEmpty(dataArray[8]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Course Over Ground", decimal.Parse(dataArray[8]));
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 9 - date

        //        /////////////////////////////// 10 - magnetic variation
        //        if (!string.IsNullOrEmpty(dataArray[10]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Magnetic Variation", dataArray[10].ToString());
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 11 - east west indicator
        //        if (!string.IsNullOrEmpty(dataArray[11]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("East West Indicator", dataArray[11].ToString());
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 12 - mode
        //        if (!string.IsNullOrEmpty(dataArray[12]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Mode", dataArray[12].ToString());
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 13 - Sun rise time
        //        if (!string.IsNullOrEmpty(dataArray[13]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Sun rise time", DateTime.Parse(stringDate + dataArray[13].ToString()).ToUniversalTime());
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 14 - Sun set time
        //        if (!string.IsNullOrEmpty(dataArray[14]))
        //        {
        //            try
        //            {
        //                sensorData.Data.Add("Sun set time", DateTime.Parse(stringDate + dataArray[14].ToString()).ToUniversalTime());
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 15 - right lamps states
        //        if (!string.IsNullOrEmpty(dataArray[15]))
        //        {
        //            try
        //            {
        //                var data = dataArray[15].ToString();
        //                if (data.Length == 65)
        //                {
        //                    sensorData.Data.Add("RightCode", data.Substring(0, 3));
        //                    sensorData.Data.Add("RightFault", data.Substring(3, 2));

        //                    for (int i = 5; i < data.Length; i++)
        //                    {
        //                        var valueLamp = Convert.ToString(Convert.ToInt16(data.Substring(i, 1), 16), 2).PadLeft(4, '0').ToCharArray();
        //                        ++i;
        //                        var valueRelay = Convert.ToString(Convert.ToInt16(data.Substring(i, 1), 16), 2).PadLeft(4, '0').ToCharArray();
        //                        for (int j = 0; j < valueLamp.Length; j++)
        //                        {
        //                            sensorData.Data.Add(i.ToString() + j.ToString(), new { Lamp = Int16.Parse(valueLamp[j].ToString()), Relay = Int16.Parse(valueRelay[j].ToString()) });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    sensorData.Data.Add("Code Right Lamp", "Invalid right lamps states");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //        /////////////////////////////// 16 - left lamps states
        //        if (!string.IsNullOrEmpty(dataArray[16]))
        //        {
        //            try
        //            {
        //                var data = dataArray[16].ToString();
        //                if (data.Length == 65)
        //                {
        //                    sensorData.Data.Add("LeftCode", data.Substring(0, 3));
        //                    sensorData.Data.Add("LeftFault", data.Substring(3, 2));

        //                    for (int i = 5; i < data.Length; i++)
        //                    {
        //                        var valueLamp = Convert.ToString(Convert.ToInt16(data.Substring(i, 1), 16), 2).PadLeft(4, '0').ToCharArray();
        //                        ++i;
        //                        var valueRelay = Convert.ToString(Convert.ToInt16(data.Substring(i, 1), 16), 2).PadLeft(4, '0').ToCharArray();
        //                        for (int j = 0; j < valueLamp.Length; j++)
        //                        {
        //                            sensorData.Data.Add(i.ToString() + j.ToString(), new { Lamp = valueLamp[j], Relay = valueRelay[j] });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    sensorData.Data.Add("Code Left Lamp", "Invalid left lamps states");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                gatewayData.LogException(ex);
        //            }
        //        }
        //    }
        //}
    }
}