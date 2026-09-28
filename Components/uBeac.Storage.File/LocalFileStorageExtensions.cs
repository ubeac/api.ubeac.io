/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using uBeac.Storage.File;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LocalFileStorageExtensions
    {
        public static IServiceCollection AddLocalFileStorages(this IServiceCollection services)
        {
            services.TryAddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();

                // Local file storage
                var storageOptions = configuration.GetSection("FileStorages").Get<List<LocalStorageOption>>();
                var fileStorages = new List<IFileStorage>();
                foreach (var option in storageOptions)
                {
                    fileStorages.Add(new LocalFileStorage(option));
                }
                 
                return fileStorages;

            });

            services.AddSingleton<FileStorageFactory>();

            return services;
        }
    }
}
