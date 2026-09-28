//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace uBeac.IoT.Models
//{
//    // based on                    --> https://en.wikipedia.org/wiki/IBeacon
//    // company-identifiers         --> https://www.bluetooth.com/specifications/assigned-numbers/company-identifiers
//    // 16-bit-uuids-for-members    --> https://www.bluetooth.com/specifications/assigned-numbers/16-bit-uuids-for-members
//    // record types                --> https://www.bluetooth.com/specifications/assigned-numbers/generic-access-profile


//    public class iBeaconData
//    {


//        public int DataLength { get; set; }
//        public string DataTypeCode { get; set; }
//        public string ManufacturerID { get; set; }
//        public string ManufacturerDesc { get; set; }
//        public string SubTypeID { get; set; }
//        public string SubTypeDesc { get; set; }
//        public int SubTypeLength { get; set; }
//        public string ProximityUUID { get; set; }
//        public string Major { get; set; }
//        public string Minor { get; set; }
//        public string SignalPower { get; set; }
//        public string Distance { get; set; }
//        public bool IsDataValid { get; set; }



//        public iBeaconData(string iBeaconString)
//        {
//            //var _ibeacondata = new iBeaconData();
//            try
//            {
//                DataLength = int.Parse(iBeaconString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
//                DataTypeCode = iBeaconString.Substring(2, 2);  // this should be FF
//                ManufacturerID = iBeaconString.Substring(6, 2) + iBeaconString.Substring(4, 2);        //Manufacturer ID : 0x4c00 (Apple) 
//                SubTypeID = iBeaconString.Substring(8, 2); // this should be 02 
//                SubTypeLength = int.Parse(iBeaconString.Substring(10, 2), System.Globalization.NumberStyles.HexNumber); // should be 0x15
//                ProximityUUID = iBeaconString.Substring(12, 32);    // proximity UUID 
//                Major = int.Parse(iBeaconString.Substring(44, 4), System.Globalization.NumberStyles.HexNumber).ToString();
//                Minor = int.Parse(iBeaconString.Substring(48, 4), System.Globalization.NumberStyles.HexNumber).ToString();
//                SignalPower = (int.Parse(iBeaconString.Substring(52, 2), System.Globalization.NumberStyles.HexNumber) - 256).ToString();

//                if (DataTypeCode != "FF" || DataLength != 26 || SubTypeID != "02") { IsDataValid = false; }
//                else { IsDataValid = true; }
//            }
//            catch (Exception ex)
//            {
//                var error = ex.Message;
//                IsDataValid = false;
//                //logging 
//            }
//        }

//    } // class
    
//}
