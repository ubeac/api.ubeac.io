using System;
using uBeac.Mappings;

namespace uBeac.Api
{
    public static class Mapping
    {
        static IMapper _mapper;
        public static IMapper Mapper
        {
            get => _mapper;
            set => _mapper = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

}
