/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System.Collections.Generic;

namespace uBeac.IoT.Processing
{
    public class ProcessorOptions
    {
        public IDictionary<string, string> IdsWithCodes { get; set; }
        public IEnumerable<string> AssemblyNames { get; set; }
    }
}
