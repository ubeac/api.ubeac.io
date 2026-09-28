/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using MessagePack;
using System.Text;

namespace uBeac.Serialization
{
    public class MessageSerilizer
    {
        public static byte[] Serialize(object message)
        {
            //var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(message, Newtonsoft.Json.Formatting.None);
            //return Encoding.UTF8.GetBytes(jsonString);
            MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            return LZ4MessagePackSerializer.Serialize(message);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            //var jsonString = Encoding.UTF8.GetString(bytes);
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
            return LZ4MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}
