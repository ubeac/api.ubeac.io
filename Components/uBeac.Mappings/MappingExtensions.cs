/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using AutoMapper;
using System;
using uBeac.Mappings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MappingExtensions
    {
        public static IServiceCollection AddMapper(this IServiceCollection services, Action<MapperConfig> mapperConfigAction)
        {
            var mapperConfig = new MapperConfig();
            mapperConfigAction.Invoke(mapperConfig);

            services.AddAutoMapper(cfg =>
            {
                foreach (var item in mapperConfig.GetMaps())
                {
                    cfg.CreateMap(item.Key, item.Value);
                }
            });

            services.AddScoped<uBeac.Mappings.IMapper, uBeac.Mappings.Mapper>();

            return services;
        }
    }
}
