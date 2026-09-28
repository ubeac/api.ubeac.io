/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace uBeac.IoT.Processing
{
    public class ProcessorPool : IProcessorPool
    {
        private Dictionary<string, string> _codes;
        private Dictionary<string, IProcessor> instances;
        private List<string> _assemblyNames;
        private IProcessor _defaultProcessorInstance;

        public ProcessorPool()
        {            
        }

        public void Build(ProcessorOptions processorOptions)
        {
            _defaultProcessorInstance = new DefaultProcessor();
            _codes = new Dictionary<string, string>(processorOptions.IdsWithCodes);
            _assemblyNames = new List<string>(processorOptions.AssemblyNames);
            instances = new Dictionary<string, IProcessor>();
            foreach (var item in _codes)
            {
                Update(item.Key, item.Value);
            }
        }

        public IProcessor GetProcessor(string processorId)
        {
            return instances.ContainsKey(processorId) ? instances[processorId] : _defaultProcessorInstance;
        }

        private void Update(string processorId, string code)
        {
            if (string.IsNullOrEmpty(code)) 
                return;

            IProcessor processor;
            using (var compiler = new ProcessorBuilder(processorId, code, _assemblyNames))
            {
                var newType = compiler.Compile();
                if (newType != null)
                {
                    processor = (IProcessor)Activator.CreateInstance(newType);
                    if (instances.ContainsKey(processorId))
                        instances[processorId] = processor;
                    else
                        instances.Add(processorId, processor);
                }
            }
        }
    }
}
