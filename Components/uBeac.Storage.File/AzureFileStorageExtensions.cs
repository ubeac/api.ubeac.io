///* ------------------------------------------------------------------
// * This is stable version, audit by by Amir in 2018-10-31 
// * ------------------------------------------------------------------*/

//using Microsoft.Extensions.Configuration;
//using System.Collections.Generic;
//using uBeac.Storage.File;

//namespace Microsoft.Extensions.DependencyInjection
//{
//    public static class AzureFileStorageExtensions
//    {
//        public static IServiceCollection AddAzureFileStorages(this IServiceCollection services, IConfiguration configuration)
//        {
//            // Azure BLOB file storage
//            var storageOptions = configuration.GetSection("FileStorages").Get<List<AzureBlobStorageOption>>();
//            var fileStorages = new List<IFileStorage>();
//            foreach (var option in storageOptions)
//            {
//                fileStorages.Add(new AzureBlobFileStorage(option));
//            }
//            services.AddSingleton(fileStorages);
//            services.AddSingleton<FileStorageFactory>();

//            return services;
//        }
//    }
//}
