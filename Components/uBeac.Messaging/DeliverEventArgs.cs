/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;

namespace uBeac.Messaging
{
    public class DeliverEventArgs : EventArgs
    {
        public object Object { get; set; }

        public DeliverEventArgs(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; }

        // todo: remove this method and convert it to a readonly property to be filled on constructor
        public T Get<T>() where T : class
        {
            var result = Serialization.MessageSerilizer.Deserialize<T>(Bytes);
            Object = result;
            return result;
        }
    }
}
