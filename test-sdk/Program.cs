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

                var options = new ClusterOptions()
                {
                    Logging = loggerFactory,
                    UserName = "Administrator",
                    Password = "YStv9AQU2rtHGUUr",
                    ConnectionString = "couchbase://cb-dev.farmhub.falckrenewables.com"
                };

                var bucketName = "semantic";

                var cluster = await Cluster.ConnectAsync(options);
                var bucket = await cluster.BucketAsync(bucketName);
                var collection = bucket.DefaultCollection();

                var result = await cluster.QueryAsync<dynamic>(
                    $"SELECT t.* FROM `{bucketName}` t WHERE t.type=$1",
                    options => options.Parameter("measure")
                );

                await foreach (var row in result)
                {
                    // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                    var name = row.area;                        // "TECH"
                    var address = row.semanticArtifactKey;      // NVRTR
                    Console.WriteLine($"{name},{address}");
                }

                Console.WriteLine(result.Rows.ToString());

                await collection.UpsertAsync<string>("beer-sample-101", "logging example 101");

                var returnVal = await collection.GetAsync("beer-sample-101");

                Console.WriteLine(returnVal.ContentAs<string>());
            }
        }
    }
