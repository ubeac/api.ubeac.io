using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using uBeac.Api.Facades;
using uBeac.Models;
using uBeac.Web.Filters;

namespace uBeac.Api.Controllers
{
    [Route("[controller]/[action]/")]
    [TypeFilter(typeof(ResultSetResponseCodeFilter))]
    [TypeFilter(typeof(ModelStateValidationAsyncActionFilter))]
    [SwaggerTag(FileConstants.DESCRIPTION)]
    public class FileController
    {
        private readonly IFileFacade _fileFacade;

        public FileController( IFileFacade fileFacade)
        {
            _fileFacade = fileFacade;
        }

        [HttpPost]
        [SwaggerOperation(Summary = FileConstants.UPLOAD_SUMMARY, Description = FileConstants.UPLOAD_DESCRIPTION)]
        public async Task<ResultSet<File>> Upload(IFormFile file)
        {
            if (file is null)
                return new ResultSet<File>();
            

            if (file.Length > 3145728) //3MB
                return new ResultSet<File>();

            var _file = new File()
            {
                Name = System.IO.Path.GetFileName(file.FileName),
                Size = file.Length,
                Extension = System.IO.Path.GetExtension(file.FileName)
            };

            return await _fileFacade.UploadAsync(_file, file.OpenReadStream());
        }

        [HttpGet("{id}")]
        [ResponseCache(VaryByQueryKeys = new string[] { "id" }, Location = ResponseCacheLocation.Client, Duration = 3600)]
        [SwaggerOperation(Summary = FileConstants.DOWNLOAD_SUMMARY, Description = FileConstants.DOWNLOAD_DESCRIPTION)]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var fileResultSet = await _fileFacade.GetByIdAsync(id);
                var file = fileResultSet.Data;

                var targetStream = new System.IO.MemoryStream();
                var mimeType = FileType.GetMimeType(file.Extension);
                await _fileFacade.DownloadAsync(id, targetStream);
                targetStream.Seek(0, 0);

                return new FileStreamResult(targetStream, mimeType)
                {
                    FileDownloadName = file.Name
                };
            }
            catch (Exception)
            {
                return new NotFoundResult();
            }
        }

    }
}
