/*------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace uBeac.IoT.Processing
{
    public static class ProcessorExtentions
    {
        public static IServiceProvider UserProcessorPool(this IServiceProvider serviceProvider, Dictionary<string, string> idsWithCodes)
        {
            var configuration = serviceProvider.GetService<IConfiguration>();

            var processorOptions = new ProcessorOptions()
            {
                AssemblyNames = configuration.GetSection("ProcessorPoolAssemblies").Get<List<string>>(),
                IdsWithCodes = idsWithCodes
            };

            var _processorPool = serviceProvider.GetService<IProcessorPool>();
            _processorPool.Build(processorOptions);
            return serviceProvider;
        }
    }
}
