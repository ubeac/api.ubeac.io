using System;
using System.Linq;

namespace uBeac.Models
{

    //0x00  UID        --> https://github.com/google/eddystone/tree/master/eddystone-uid
    //0x10  URL        --> https://github.com/google/eddystone/tree/master/eddystone-url
    //0x20  TLM        --> https://github.com/google/eddystone/blob/master/eddystone-tlm/tlm-plain.md   (unencrypted) 
    //0x30  EID        --> https://github.com/google/eddystone/tree/master/eddystone-eid

    public class EddyStoneData
    {
        public string TxPower { get; set; }
        public string Url { get; set; }
        public string Namespace { get; set; }
        public string InstanceID { get; set; }
        public string RFU { get; set; }
        public string BatteryVoltage { get; set; }
        public string Temperature { get; set; }
        public string AdvertisingPduCount { get; set; }
        public string TimeSincePower { get; set; }
        public string ServiceType { get; set; }

        public bool IsDataValid { get; set; }

        public EddyStoneData() { IsDataValid = false; }


        public EddyStoneData(string EddyStoneString)
        {
            try
            {

                IsDataValid = false;

                //var _eddystone = new EddyStoneData();
                var _length = Int16.Parse(EddyStoneString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var _type = EddyStoneString.Substring(2, 2);   // this should be 16  What if it is FF ?
                var _data = EddyStoneString.Substring(8);
                var _svc_data_type = _data.Substring(0, 2);



                if (_svc_data_type == "00")    // UID packet - length should be 23 
                {
                    //var _svc_data_type = _data.Substring(0, 2); // this is 00
                    var _tx = (int.Parse(_data.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - 256 - 41).ToString();
                    var _namespace = _data.Substring(4, 20);
                    var _instance_id = _data.Substring(24, 12);
                    var _rfu = ""; if (_data.Length > 36) { _data.Substring(36, 4); } // this might not be provided by all products 

                    TxPower = _tx;
                    Namespace = _namespace;
                    InstanceID = _instance_id;
                    RFU = _rfu;
                    IsDataValid = true;
                }

                else if (_svc_data_type == "10")    // website - packet size is not fixed 
                {
                    //var _svc_data_type = _data.Substring(0, 2);
                    var _tx = (int.Parse(_data.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) - 256 - 41).ToString();
                    var _url_prefix = _data.Substring(4, 2);
                    var _url_encoded = _data.Substring(6, _length * 2 - 14);
                    var _url_expansion = _data.Substring((_length + 1) * 2 - 10);

                    TxPower = _tx;
                    Url = LookupUrlPrefixCode(_url_prefix) + HextoString(_url_encoded) + LookupUrlExpansionCode(_url_expansion);
                    IsDataValid = true;
                }
                else if (_svc_data_type == "20")    // TLM packet 
                {
                    try
                    {
                        //var _svc_data_type = _data.Substring(0, 2);
                        var _tlm_version = _data.Substring(2, 2);
                        if (_tlm_version == "00") //Unencrypted
                        {
                            var _battery_voltage = int.Parse(_data.Substring(4, 4), System.Globalization.NumberStyles.HexNumber);   // MiliVolts 
                            var _temperature = decimal.Parse(int.Parse(_data.Substring(8, 2), System.Globalization.NumberStyles.HexNumber).ToString()
                                                             + "."
                                                             + int.Parse(_data.Substring(10, 2), System.Globalization.NumberStyles.HexNumber).ToString()
                                                            );             // Degree-celcuis based on 8.8 system
                            var _adv_pdu_count = int.Parse(_data.Substring(12, 4), System.Globalization.NumberStyles.HexNumber);
                            var _time_since_poweron = int.Parse(_data.Substring(16, 4), System.Globalization.NumberStyles.HexNumber); // this is 0.1 sec

                            BatteryVoltage = (_battery_voltage / 1000.0).ToString();  //convert to volts 
                            Temperature = _temperature.ToString();
                            AdvertisingPduCount = _adv_pdu_count.ToString();
                            TimeSincePower = _time_since_poweron.ToString();
                            IsDataValid = true;
                        }
                        else
                        {
                            // Not implemented yet 
                        }
                    }
                    catch (Exception)
                    {
                        //logging 
                    }
                }

                else if (_svc_data_type == "30")    // EID packet 
                {
                    var _tx = int.Parse(_data.Substring(2, 1), System.Globalization.NumberStyles.HexNumber).ToString();
                    TxPower = _tx;
                    IsDataValid = true;
                }
                //return _eddystone;


            }
            catch (Exception)
            {
                IsDataValid = false;
                //throw;
            }
        }



        ////////////////////////////////////////////////////
        public string LookupUrlPrefixCode(string PrefixCode)
        {
            if (PrefixCode == "00") { return "http://www."; }
            if (PrefixCode == "01") { return "https://www."; }
            if (PrefixCode == "02") { return "http://"; }
            if (PrefixCode == "03") { return "https://"; }
            return "";
        }
        ////////////////////////////////////////////////////
        public string LookupUrlExpansionCode(string ExpansionCode)
        {
            if (ExpansionCode == "00") { return ".com"; }
            if (ExpansionCode == "01") { return ".org"; }
            if (ExpansionCode == "02") { return ".edu"; }
            if (ExpansionCode == "03") { return ".net"; }
            if (ExpansionCode == "04") { return ".info"; }
            if (ExpansionCode == "05") { return ".biz"; }
            if (ExpansionCode == "06") { return ".gov"; }
            if (ExpansionCode == "07") { return ".com"; }
            if (ExpansionCode == "08") { return ".org"; }
            if (ExpansionCode == "09") { return ".edu"; }
            if (ExpansionCode == "0A") { return ".net"; }
            if (ExpansionCode == "0B") { return ".info"; }
            if (ExpansionCode == "0C") { return ".biz"; }
            if (ExpansionCode == "0D") { return ".gov"; }
            return "";
        }
        ////////////////////////////////////////////////////
        public static string HextoString(string InputText)
        {

            byte[] bb = Enumerable.Range(0, InputText.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(InputText.Substring(x, 2), 16))
                             .ToArray();
            return System.Text.Encoding.ASCII.GetString(bb);
        }

    }




}
