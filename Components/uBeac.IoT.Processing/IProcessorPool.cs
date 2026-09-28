/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

namespace uBeac.IoT.Processing
{
    public interface IProcessorPool
    {
        IProcessor GetProcessor(string processorId);
        void Build(ProcessorOptions processorOptions);
    }
}
