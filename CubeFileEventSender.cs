using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using CubeFileProsessor.Constants;
using CubeFileProsessor.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CubeFileProsessor
{
    public class CubeFileEventSender
    {
        private readonly IQueueClientFactory _queueClientFactory;

        public CubeFileEventSender(IQueueClientFactory queueClientFactory)
        {
            _queueClientFactory = queueClientFactory;
        }

        [FunctionName("CubeFileEventSender")]
        public async Task Run([TimerTrigger("0 0 0 * * 0")] TimerInfo myTimer,
        ILogger log)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable(ConfigConstants.saConnectStr);
                var shareName = Environment.GetEnvironmentVariable(ConfigConstants.saCubeShareName);
                var sourceDirName = Environment.GetEnvironmentVariable(ConfigConstants.sourceDirName);

                // Get a reference to the file
                var shareClient = new ShareClient(connectionString, shareName);
                await shareClient.CreateIfNotExistsAsync();

                var shareDirectoryClient = shareClient.GetDirectoryClient(sourceDirName);
                await shareDirectoryClient.CreateIfNotExistsAsync();

                var filesAndDirectories = shareDirectoryClient.GetFilesAndDirectoriesAsync();

                var queueClient = await _queueClientFactory.GetQueueClientAsync("cube-file-process");

                await foreach (var fileOrDir in filesAndDirectories)
                {
                    if (!fileOrDir.IsDirectory)
                    {
                        try
                        {
                            //Send each file name to a queue to be processes by a separate function
                            queueClient.SendMessage(fileOrDir.Name);
                        }
                        catch (Exception e)
                        {
                            log.LogError($"Failed to add {fileOrDir.Name} to the queue");
                            log.LogError(e.StackTrace);
                            throw;
                        }
                    }
                }

                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            }
            catch (Exception e)
            {
                log.LogError(e.StackTrace);
                throw;
            }

        }
    }
}