/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace uBeac.Storage.File
{
    public interface IFileStorage
    {
        string Name { get;}
        Task WriteAsync(string path, string filename, Stream stream, CancellationToken cancellationToken = default(CancellationToken));
        Task ReadAsync(string path, string filename, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken));
        Task WriteAsync(string path, string filename, string contents, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> ExistsAsync(string path, string filename);
        Task<string> ReadAsync(string path, string filename, CancellationToken cancellationToken = default(CancellationToken));
    }
}
