using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace GraphProjection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables("GraphProjection_").Build();

            var docCollection = new DocumentCollectionInfo()
            {
                CollectionName = config["SourceCollection"],
                Uri = new Uri(config["Uri"]),
                MasterKey = config["MasterKey"],
                DatabaseName = config["SourceDatabase"]
            };

            var auxCollection = new DocumentCollectionInfo()
            {
                CollectionName = config["CheckpointCollection"],
                Uri = new Uri(config["Uri"]),
                MasterKey = config["MasterKey"],
                DatabaseName = config["CheckpointDatabase"]
            };

            var host = new ChangeFeedEventHost(
                "GraphProjectionObserver",
                docCollection,
                auxCollection
            );

            await host.RegisterObserverAsync<GraphProjectionObserver>();
            Console.WriteLine("Running - press any key to exit");
            Console.Read();
        }
    }
}
