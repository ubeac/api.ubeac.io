using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndexCreator
{
    class Program
    {
        private static string gatewayDataconnectionString = "mongodb://ubeacuser:kCsR!hd739F$nAL{wt@mongodb1.ubeac.io:27017,mongodb2.ubeac.io:27017,mongodb3.ubeac.io:27017,mongodb4.ubeac.io:27017/GatewayData?replicaSet=rs0&authSource=admin&retryWrites=true&serverSelectionTimeoutMS=5000&readPreference=secondary";
        private static string sensorDataconnectionString = "mongodb://ubeacuser:kCsR!hd739F$nAL{wt@mongodb1.ubeac.io:27017,mongodb2.ubeac.io:27017,mongodb3.ubeac.io:27017,mongodb4.ubeac.io:27017/SensorData?replicaSet=rs0&authSource=admin&retryWrites=true&serverSelectionTimeoutMS=5000&readPreference=secondary";
        //private static string deviceSummaryDataconnectionString = "mongodb://ubeacuser:kCsR!hd739F$nAL{wt@mongodb1.ubeac.io:27017,mongodb2.ubeac.io:27017,mongodb3.ubeac.io:27017,mongodb4.ubeac.io:27017/DeviceSummary?replicaSet=rs0&authSource=admin&retryWrites=true&serverSelectionTimeoutMS=5000&readPreference=secondary";
       
        private static IMongoClient _gatewayDataclient = new MongoClient(gatewayDataconnectionString);
        private static IMongoClient _sensorDataclient = new MongoClient(sensorDataconnectionString);
        //private static IMongoClient _deviceSummaryclient = new MongoClient(deviceSummaryDataconnectionString);
        
        private static IMongoDatabase _gatewayDataDB = _gatewayDataclient.GetDatabase("GatewayData");
        private static IMongoDatabase _sensorDataDB = _sensorDataclient.GetDatabase("SensorData");
        
        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to start ...");
            Console.ReadLine();

            GatewayDataIndexCreator();
            SensorDataIndexCreator();
            
            Console.WriteLine("Press Enter to exit ...");
            Console.ReadLine();
        }

        static void GatewayDataIndexCreator()
        {
            var collectionNames = _gatewayDataDB.ListCollectionNames().ToList();
            var tasks = new List<Task>();

            foreach (var collectionName in collectionNames)
            {
                if (collectionName != "temp")
                {
                    var collection = _gatewayDataDB.GetCollection<BsonDocument>(collectionName);
                    var indexBuilder = Builders<BsonDocument>.IndexKeys;
                    var indexModel = new CreateIndexModel<BsonDocument>(indexBuilder.Descending("DateTime"), new CreateIndexOptions { Background = true, Name = "DateTime_Index" });

                    tasks.Add(collection.Indexes.CreateOneAsync(indexModel));

                    Console.WriteLine("Task was created for " + collectionName);
                }
            }
            Task.WhenAll(tasks).Wait();
            Console.WriteLine("========== Indexes created for GatewayData ==========");
        }

        static void SensorDataIndexCreator()
        {
            var collectionNames = _sensorDataDB.ListCollectionNames().ToList();
            var tasks = new List<Task>();

            foreach (var collectionName in collectionNames)
            {
                if (collectionName != "temp")
                {
                    var collection = _sensorDataDB.GetCollection<BsonDocument>(collectionName);
                    var indexBuilder = Builders<BsonDocument>.IndexKeys;
                    var indexModel = new CreateIndexModel<BsonDocument>(indexBuilder.Descending("DateTime").Ascending("SensorId"), 
                        new CreateIndexOptions { Name="Main_Index", Background = true });
                    
                    tasks.Add(collection.Indexes.CreateOneAsync(indexModel));

                    Console.WriteLine("Task was created for " + collectionName);
                }
            }
            Task.WhenAll(tasks).Wait();
            Console.WriteLine("========== Indexes created for SensorData ==========");
        }

        //static void DeviceSummaryIndexCreator()
        //{
        //    var collectionNames = _deviceSummaryDB.ListCollectionNames().ToList();
        //    var tasks = new List<Task>();

        //    foreach (var collectionName in collectionNames)
        //    {
        //        if (collectionName != "temp")
        //        {
        //            var collection = _deviceSummaryDB.GetCollection<BsonDocument>(collectionName);
        //            var indexBuilder = Builders<BsonDocument>.IndexKeys;
        //            var indexModel = new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("TeamId").Ascending("FloorId").Ascending("DeviceUid").Ascending("SensorType"),
        //                new CreateIndexOptions { Name = "Main_Index", Background = true });

        //            tasks.Add(collection.Indexes.CreateOneAsync(indexModel));

        //            Console.WriteLine("Task was created for " + collectionName);
        //        }
        //    }
        //    Task.WhenAll(tasks).Wait();
        //    Console.WriteLine("========== Indexes created for TagSummary ==========");
        //}
    }
}