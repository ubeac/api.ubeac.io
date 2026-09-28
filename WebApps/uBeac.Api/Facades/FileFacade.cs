using System;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IFileFacade
    {
        Task<ResultSet<File>> UploadAsync(File file, System.IO.Stream stream, CancellationToken cancellationToken = default);
        Task DownloadAsync(Guid id, System.IO.Stream targetStream, CancellationToken cancellationToken = default);
        Task<ResultSet<File>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
    public class FileFacade : IFileFacade
    {
        private readonly IFileService _fileService;
        private readonly ISecurityContext _securityContext;
        public FileFacade(IFileService fileService, ISecurityContext securityContext)
        {
            _fileService = fileService;
            _securityContext = securityContext;
        }

        public async Task DownloadAsync(Guid id, System.IO.Stream targetStream, CancellationToken cancellationToken = default)
        {
            await _fileService.DownloadAsync(id, targetStream, cancellationToken);
        }

        public async Task<ResultSet<File>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var file = await _fileService.GetByIdAsync(id, cancellationToken);
            if (file is null)
                return new ResultSet<File>(ResponseCodes.NotFound);

            return new ResultSet<File>(file);
        }

        public async Task<ResultSet<File>> UploadAsync(File file, System.IO.Stream stream, CancellationToken cancellationToken = default)
        {
            file.CreateBy = _securityContext.Identity.UserId;
            file.UpdateBy = _securityContext.Identity.UserId;
            await _fileService.UploadAsync(file, stream, cancellationToken);
            return new ResultSet<File>(file);
        }
    }
}
