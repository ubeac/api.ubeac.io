/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace uBeac.Storage.File
{
    public class LocalFileStorage : IFileStorage
    {
        public string Name { get; }
        public string FilePath { get; }
        protected readonly LocalStorageOption localStorageOption;

        public LocalFileStorage(LocalStorageOption option)
        {
            if (option is null)
                throw new NullReferenceException(nameof(option));

            localStorageOption = option;
            Name = localStorageOption.Name;
            FilePath = localStorageOption.FilePath;
        }

        public async Task<bool> ExistsAsync(string path, string filename)
        {
            var fullPath = FilePath + filename;
            return await Task.FromResult(System.IO.File.Exists(fullPath));
        }

        public async Task WriteAsync(string path, string filename, string content, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = FilePath + filename;
            await System.IO.File.WriteAllTextAsync(fullPath, content, cancellationToken);
        }

        public async Task<string> ReadAsync(string path, string filename, CancellationToken cancellationToken = default(CancellationToken))
        {            
            var fullPath = FilePath + filename;
            return await System.IO.File.ReadAllTextAsync(fullPath);
        }

        public async Task WriteAsync(string path, string filename, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = FilePath + filename;

            using (var fileStream = new MemoryStream())
            {
                stream.CopyTo(fileStream);
                await System.IO.File.WriteAllBytesAsync(fullPath, fileStream.ToArray(), cancellationToken);
            }                                   
        }

        public async Task ReadAsync(string path, string filename, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = FilePath + filename;
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            await stream.CopyToAsync(targetStream);
        }

    }
}
