using Microsoft.Azure.Documents.ChangeFeedProcessor;
using SimpleEventStore.AzureDocumentDb;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace GraphProjection
{
    class GraphProjectionObserverFactory : IChangeFeedObserverFactory
    {
        private readonly DocumentClient client;
        private readonly DocumentCollection collectionInfo;
        private readonly ISerializationTypeMap typeMap;

        public GraphProjectionObserverFactory(DocumentClient client, DocumentCollection collectionInfo, ISerializationTypeMap typeMap)
        {
            this.client = client;
            this.collectionInfo = collectionInfo;
            this.typeMap = typeMap;
        }

        public IChangeFeedObserver CreateObserver()
        {
            return new GraphProjectionObserver(client, collectionInfo, typeMap);
        }
    }
}
