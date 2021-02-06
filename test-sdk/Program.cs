using System;
using System.Threading.Tasks;
using Couchbase;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace test_sdk
{
    class Program
    {
        [Obsolete]
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ILoggerFactory loggerFactory = new LoggerFactory();

            _ = loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });

            LogManager.LoadConfiguration("Nlog.config");

            // connection string and credentials are passed as arguments

            var options = new ClusterOptions()
            {
                Logging = loggerFactory,
                UserName = args[1],
                Password = args[2],
                ConnectionString = args[0]
            };

            var bucketName = "semantic";

            await using (var cluster = await Cluster.ConnectAsync(options))
            {
                var bucket = await cluster.BucketAsync(bucketName);
                var collection = bucket.DefaultCollection();

                try
                {
                    var result = await cluster.QueryAsync<dynamic>(
                        $"SELECT t.* FROM `{bucketName}` t WHERE t.type=$1 LIMIT 10",
                        ops => ops.Parameter("measure")
                    );

                    Console.WriteLine("############ START PRINTING QUERY RESULT ############");

                    await foreach (var row in result)
                    {
                        // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                        var name = row.createdBy;                        
                        var address = row.updateDate;
                        Console.WriteLine($"{name},{address}");
                    }

                    Console.WriteLine("############ END PRINTING QUERY RESULT ############");

                    Console.WriteLine(result.Rows.ToString());
                
                    await collection.UpsertAsync<string>("beer-sample-101", "logging example 101");

                    var returnVal = await collection.GetAsync("beer-sample-101");

                    Console.WriteLine(returnVal.ContentAs<string>());
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("################## READ THIS EXCEPTION ##################");
                    Console.WriteLine($"Exception: {ex.Message}, stack: {ex.StackTrace}");
                    Console.WriteLine();
                }
            }
        }
    }
}
