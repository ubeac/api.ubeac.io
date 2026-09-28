/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;

namespace uBeac.Mappings
{
    public class Mapper : IMapper
    {
        private readonly AutoMapper.IMapper _mapper;
        public Mapper(AutoMapper.IMapper mapper)
        {
            _mapper = mapper;
        }

        public Mapper(Action<MapperConfig> mapperConfigAction)
        {
            var mapperConfig = new MapperConfig();
            mapperConfigAction.Invoke(mapperConfig);

            var autoMapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                foreach (var item in mapperConfig.GetMaps())
                {
                    cfg.CreateMap(item.Key, item.Value);
                }
            });
            _mapper = autoMapperConfig.CreateMapper();
        }
        
        public TDestination Map<TDestination>(object source)
        {
            return _mapper.Map<TDestination>(source);
        }
    }
    public interface IMapper
    {
        TDestination Map<TDestination>(object source);
    }
}
