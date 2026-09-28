using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;
using uBeac.Storage.File;

namespace uBeac.Api.Services
{
    public interface IFileService
    {
        Task<Guid> UploadAsync(File file, System.IO.Stream stream, CancellationToken cancellationToken = default);
        Task DownloadAsync(Guid id, System.IO.Stream targetStream, CancellationToken cancellationToken = default);
        Task<File> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly FileStorageFactory _fileStorageFactory;
        private readonly IFileStorage _fileStorage;

        public FileService(IFileRepository fileRepository, FileStorageFactory fileStorageFactory)
        {
            _fileRepository = fileRepository;
            _fileStorageFactory = fileStorageFactory;
            _fileStorage = _fileStorageFactory.GetStorage("PublicFileStorage");
        }

        public async Task<Guid> UploadAsync(File file, System.IO.Stream stream, CancellationToken cancellationToken = default)
        {
            var createDate = DateTime.UtcNow;
            file.CreateDate = createDate;
            file.UpdateDate = createDate;
            file.Id = Guid.NewGuid();

            try
            {
                var fileId = await _fileRepository.InsertAsync(file, cancellationToken);
                await _fileStorage.WriteAsync(string.Empty, fileId.ToString(), stream, cancellationToken);
                return fileId;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public async Task DownloadAsync(Guid id, System.IO.Stream targetStream, CancellationToken cancellationToken = default)
        {
            await _fileStorage.ReadAsync(string.Empty, id.ToString(), targetStream);
        }

        public async Task<File> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _fileRepository.GetByIdAsync(id, cancellationToken);
        }
    }
}
