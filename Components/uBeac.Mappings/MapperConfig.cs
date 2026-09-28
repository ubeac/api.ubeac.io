/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace uBeac.Mappings
{
    public class MapperConfig
    {
        private Dictionary<Type, Type> mappings;

        public MapperConfig()
        {
            mappings = new Dictionary<Type, Type>();
        }

        public void CreateMap<TSource, TDestination>()
        {
            mappings.Add(typeof(TSource), typeof(TDestination));
        }

        public Dictionary<Type, Type> GetMaps()
        {
            return mappings;
        }

    }
}
