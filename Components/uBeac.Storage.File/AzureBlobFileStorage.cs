///* ------------------------------------------------------------------
// * This is stable version, audit by by Amir in 2018-10-31 
// * ------------------------------------------------------------------*/

//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Blob;
//using System;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

//namespace uBeac.Storage.File
//{
//    public class AzureBlobFileStorage : IFileStorage
//    {
//        protected readonly AzureBlobStorageOption azureBlobStorageOption;
//        protected readonly CloudStorageAccount storageAccount;
//        protected readonly CloudBlobClient blobClient;
//        protected readonly CloudBlobContainer container;
//        private CloudBlockBlob blob;

//        public string Name { get; }

//        public AzureBlobFileStorage(AzureBlobStorageOption option)
//        {
//            if (option is null)
//                throw new NullReferenceException(nameof(option));

//            azureBlobStorageOption = option;
//            storageAccount = CloudStorageAccount.Parse(azureBlobStorageOption.ConnectionString);
//            blobClient = storageAccount.CreateCloudBlobClient();
//            container = blobClient.GetContainerReference(azureBlobStorageOption.ContainerName);
//            Name = azureBlobStorageOption.Name;
//            container.CreateIfNotExistsAsync();
//        }

//        public async Task<bool> ExistsAsync(string path, string filename)
//        {
//            blob = container.GetBlockBlobReference(path + filename);
//            return await blob.ExistsAsync();
//        }

//        public async Task WriteAsync(string path, string filename, string contents, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            blob = container.GetBlockBlobReference(path + filename);
//            await blob.UploadTextAsync(contents);
//        }

//        public async Task<string> ReadAsync(string path, string filename, CancellationToken cancellationToken = default(CancellationToken))
//        {

//            blob = container.GetBlockBlobReference(path + filename);
//            return await blob.DownloadTextAsync();
//        }

//        public async Task WriteAsync(string path, string filename, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            blob = container.GetBlockBlobReference(path + filename);
//            await blob.UploadFromStreamAsync(stream);
//        }

//        public async Task ReadAsync(string path, string filename, Stream targetStream, CancellationToken cancellationToken = default(CancellationToken))
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            blob = container.GetBlockBlobReference(path + filename);
//            await blob.DownloadToStreamAsync(targetStream);
//        }

//    }
//}
