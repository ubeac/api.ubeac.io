/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using uBeac.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationServicesExtensions
    {
        public const string DEBUG_COLLECTION_NAME = "Debug";
        public const string ERROR_COLLECTION_NAME = "Error";
        public const string VERBOSE_COLLECTION_NAME = "Verbose";
        public const string FATAL_COLLECTION_NAME = "Fatal";
        public const string WARNING_COLLECTION_NAME = "Warning";
        public const string INFORMATION_COLLECTION_NAME = "Information";

        public static IServiceCollection AddMongoDBLog(this IServiceCollection services, string connectionStringName= "LogConnection", string collectionNamePrefix="")
        {

            services.TryAddSingleton(provider => 
            {
                var configuration = provider.GetService<IConfiguration>();
                return configuration;
            });

            services.AddSingleton(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var mongodbConnectionString = configuration.GetConnectionString(connectionStringName);

                Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .Enrich.WithAssemblyName()
                    .Enrich.WithAssemblyVersion()
                    .Enrich.WithThreadId()
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName()
                    .Enrich.WithProcessId()
                    .Enrich.WithProcessName()
                    .WriteTo.Logger(lc => lc.Filter.With(new ErrorLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + ERROR_COLLECTION_NAME))
                    .WriteTo.Logger(lc => lc.Filter.With(new FatalLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + FATAL_COLLECTION_NAME))
                    .WriteTo.Logger(lc => lc.Filter.With(new InformationLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + INFORMATION_COLLECTION_NAME))
                    .WriteTo.Logger(lc => lc.Filter.With(new DebugLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + DEBUG_COLLECTION_NAME))
                    .WriteTo.Logger(lc => lc.Filter.With(new VerboseLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + VERBOSE_COLLECTION_NAME))
                    .WriteTo.Logger(lc => lc.Filter.With(new WarningLogEvent()).WriteTo.MongoDB(mongodbConnectionString, collectionName: collectionNamePrefix + WARNING_COLLECTION_NAME))
                    .CreateLogger();

                return new LoggerFactory().AddSerilog();

            });

            services.AddLogging();

            return services;

        }

    }

}
