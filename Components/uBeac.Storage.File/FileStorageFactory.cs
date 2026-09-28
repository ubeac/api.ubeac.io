/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace uBeac.Storage.File
{
    public class FileStorageFactory
    {

        private readonly IDictionary<string, IFileStorage> _fileStorages;

        public FileStorageFactory(List<IFileStorage> fileStorages)
        {
            _fileStorages = fileStorages.ToDictionary(n => n.Name, n => n);
        }

        public IFileStorage GetStorage(string name)
        {
            if (_fileStorages.TryGetValue(name, out var fileStorages))
                return fileStorages;

            // handle error
            throw new ArgumentException(nameof(name));
        }

    }
}
