using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;

namespace GraphProjection
{
    class GraphProjectionObserver : IChangeFeedObserver
    {
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

        public Task ProcessChangesAsync(ChangeFeedObserverContext context, IReadOnlyList<Document> docs)
        {
            Console.WriteLine($"Got {docs.Count} events!");
            return Task.CompletedTask;
        }
    }
}
