using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FileRemover
{
    class Program
    {
        #region Connection strings
                
        static string filePath = "D:\\Files\\ubeac-publicfiles\\";

        static string uBeacConnection = "mongodb://ubeacuser:kCsR!hd739F$nAL{wt@mongodb1.ubeac.io:27017,mongodb2.ubeac.io:27017,mongodb3.ubeac.io:27017,mongodb4.ubeac.io:27017/uBeac?replicaSet=rs0&authSource=admin&retryWrites=true&serverSelectionTimeoutMS=5000&readPreference=secondary";
        //static string uBeacConnection = "mongodb://192.168.0.1:27017/uBeac";
        private static IMongoClient _client = new MongoClient(uBeacConnection);
        private static IMongoDatabase _db = _client.GetDatabase("uBeac");
        private static IMongoCollection<BsonDocument> collectionManufacturer = _db.GetCollection<BsonDocument>("Manufacturer");
        private static IMongoCollection<BsonDocument> collectionProduct = _db.GetCollection<BsonDocument>("Product");
        private static IMongoCollection<BsonDocument> collectionFirmware = _db.GetCollection<BsonDocument>("Firmware");
        private static IMongoCollection<BsonDocument> collectionFloor = _db.GetCollection<BsonDocument>("Floor");
        private static IMongoCollection<BsonDocument> collectionUserProfile = _db.GetCollection<BsonDocument>("UserProfile");
        private static IMongoCollection<File> collectionFile = _db.GetCollection<File>("File");

        #endregion

        static void Main(string[] args)
        {
            var fileNames = GetStorageFileList();
            var validFilenames = ListGenerator();
            var deleteFromDB = new List<Guid>();

            foreach (var item in fileNames)
            {
                if (!validFilenames.Contains(item))
                {
                    deleteFromDB.Add(new Guid(item));
                    System.IO.File.Delete(filePath + item);

                    Console.WriteLine(item + "was deleted!");
                    Console.WriteLine("----------------------------------------------------");
                }
            }
                       
            Console.WriteLine("Starting to delete from Database");

            var filter = Builders<File>.Filter.In(x => x._id, deleteFromDB);
            var result = collectionFile.DeleteManyAsync(filter).Result;

            Console.WriteLine(result.DeletedCount.ToString() + " items were deleted. press any key to exit...");
            Console.ReadKey();
        }

        public static List<string> ListGenerator()
        {
            var filter = Builders<BsonDocument>.Filter.Empty;
            var projectionProduct = Builders<BsonDocument>.Projection.Include(x => x["ManualFile"]).Include(x => x["TechnicalSpecFile"]).Include(x => x["Image"]);

            var ManufacturerFiles = collectionManufacturer.Find(filter).ToList().Select(x => x["Logo"].AsGuid.ToString()).Distinct().ToList();

            var FirmwareFiles = collectionFirmware.Find(filter).ToList().Select(x => x["TechnicalSpecFile"].AsString).Distinct().ToList();

            var FloorFiles = collectionFloor.Find(filter).ToList().Select(x => x["PlanFileId"].AsGuid.ToString()).Distinct().ToList();

            var UserProfileFiles = collectionUserProfile.Find(filter).ToList().Select(x => x["Picture"].AsGuid.ToString()).Distinct().ToList();

            var productRawFiles = collectionProduct.Find(filter).Project(projectionProduct).ToList();
            var productManualFiles = productRawFiles.Select(x => x["ManualFile"].AsGuid.ToString()).Distinct().ToList();
            var productTechnicalSpecFile = productRawFiles.Select(x => x["TechnicalSpecFile"].AsGuid.ToString()).Distinct().ToList();
            var productImages = productRawFiles.Select(x => x["Image"].AsGuid.ToString()).Distinct().ToList();
            var productFiles = (productManualFiles.Union(productTechnicalSpecFile).Union(productImages)).ToList();

            var validFilenames = (ManufacturerFiles.Union(FirmwareFiles).Union(FloorFiles).Union(UserProfileFiles).Union(productFiles)).ToList();

            return validFilenames;
        }

        public static List<string> GetStorageFileList()
        {
            var fileNames = Directory.GetFiles(filePath).Select(Path.GetFileName).ToList();

            return fileNames;
        }
    }
}
