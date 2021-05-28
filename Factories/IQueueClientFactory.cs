using System.Threading.Tasks;
using Azure.Storage.Queues;

namespace CubeFileProsessor.Factories
{
    public interface IQueueClientFactory
    {
        Task<QueueClient> GetQueueClientAsync(string queueName);
    }
}
