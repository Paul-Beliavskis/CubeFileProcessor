using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CubeFileProsessor.Factories
{
    public class QueueClientFactory : IQueueClientFactory
    {
        private readonly string _connectionStr;

        public QueueClientFactory(string connectionString)
        {
            _connectionStr = connectionString;
        }

        public async Task<QueueClient> GetQueueClientAsync(string queueName)
        {
            var client = new QueueClient(_connectionStr, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

            await client.CreateIfNotExistsAsync();

            return client;
        }
    }
}