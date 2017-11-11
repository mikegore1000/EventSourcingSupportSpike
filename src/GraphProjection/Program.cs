using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using SimpleEventStore.AzureDocumentDb;

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
                auxCollection,
                new ChangeFeedOptions { StartFromBeginning = true},
                new ChangeFeedHostOptions()
            );

            var typeMap = new ConfigurableSerializationTypeMap()
                .RegisterTypes(
                    typeof(Program).GetTypeInfo().Assembly,
                    t => t.Namespace.EndsWith("Events"),
                    t => t.Name);

            var client = new DocumentClient(new Uri(config["GraphUri"]), config["GraphMasterKey"]);
            var collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("Support", "SupportGraph"));

            await host.RegisterObserverFactoryAsync(new GraphProjectionObserverFactory(client, collection, typeMap));
            Console.WriteLine("Running - press any key to exit");
            Console.Read();
        }
    }
}
