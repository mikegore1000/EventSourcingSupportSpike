using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using SimpleEventStore.AzureDocumentDb;
using GraphProjection.Events;
using SimpleEventStore;
using Microsoft.Azure.Documents.Linq;

namespace GraphProjection
{
    class GraphProjectionObserver : IChangeFeedObserver
    {
        private readonly DocumentClient client;
        private readonly DocumentCollection collection;
        private ISerializationTypeMap typeMap;

        public GraphProjectionObserver(DocumentClient client, DocumentCollection collection, ISerializationTypeMap typeMap)
        {
            this.client = client;
            this.collection = collection;
            this.typeMap = typeMap;
        }

        public Task CloseAsync(ChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            Console.WriteLine($"Closing {context.PartitionKeyRangeId} - {reason}");
            return Task.CompletedTask;
        }

        public Task OpenAsync(ChangeFeedObserverContext context)
        {
            Console.WriteLine($"Opening {context.PartitionKeyRangeId}");
            return Task.CompletedTask;
        }

        public async Task ProcessChangesAsync(ChangeFeedObserverContext context, IReadOnlyList<Document> docs)
        {
            foreach (var doc in docs)
            {
                var @event = DocumentDbStorageEvent.FromDocument(doc).ToStorageEvent(typeMap);

                switch (@event.EventBody)
                {
                    case OrderCreated body:
                        await Project(@event, body);
                        break;
                    case OrderDispatched e:
                        break;
                }
            }

            Console.WriteLine($"Got {docs.Count} events!");
        }

        private async Task Project(StorageEvent @event, OrderCreated e)
        {
            string[] queries = {
                $"g.addV('{@e.OrderId}').property('id', '{@event.StreamId}')",
                $"g.addV('{@event.EventBody.GetType().Name}').property('id', '{@event.EventId}')",
                $"g.V('{@event.StreamId}').addE('contains').to(g.V('{@event.EventId}'))"
            };

            await ExecuteGremlinQueries(queries);
        }

        private async Task Project(StorageEvent @event, OrderDispatched e)
        {
            string[] queries = {
                $"g.addV('{@event.EventBody.GetType().Name}').property('id', '{@event.EventId}')",
                $"g.V('{@event.StreamId}').addE('contains').to(g.V('{@event.EventId}'))"
            };

            await ExecuteGremlinQueries(queries);
        }

        private async Task ExecuteGremlinQueries(string[] queries)
        {
            foreach (var query in queries)
            {
                IDocumentQuery<dynamic> docQuery = client.CreateGremlinQuery<dynamic>(collection, query);
                while (docQuery.HasMoreResults)
                {
                    await docQuery.ExecuteNextAsync();
                }
            }
        }
    }
}
