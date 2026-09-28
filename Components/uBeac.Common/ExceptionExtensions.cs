using System;

namespace uBeac
{
    public static class ExceptionExtensions
    {

        public static void ThrowIfNull(this object source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
        }

        public static void ThrowIfNull(this string source)
        {
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
        }
        
    }
}
