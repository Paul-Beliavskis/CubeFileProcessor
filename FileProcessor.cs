using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using CubeFileProsessor.Constants;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CubeFileProsessor
{
    public static class FileProcessor
    {
        [FunctionName("FileProcessor")]
        public static async Task RunAsync([QueueTrigger("cube-file-process", Connection = "saConnectStr")] CloudQueueMessage myQueueItem, ILogger log)
        {
            try
            {
                var connectionString = Environment.GetEnvironmentVariable(ConfigConstants.saConnectStr);
                var shareName = Environment.GetEnvironmentVariable(ConfigConstants.saCubeShareName);
                ShareClient shareClient = new(connectionString, shareName);

                var outputDirectoryClient = shareClient.GetDirectoryClient("output");
                await outputDirectoryClient.CreateIfNotExistsAsync();
                var outputFileClient = outputDirectoryClient.GetFileClient(myQueueItem.AsString);

                var sourceDirName = Environment.GetEnvironmentVariable(ConfigConstants.sourceDirName);
                var sourceDirectoryClient = shareClient.GetDirectoryClient(sourceDirName);
                var sourceFileClient = sourceDirectoryClient.GetFileClient(myQueueItem.AsString);

                if (await outputFileClient.ExistsAsync())
                {
                    log.LogError("File alreay exists in the output directory");
                    await sourceFileClient.DeleteAsync();
                    return;
                }

                using var stream = await sourceFileClient.OpenReadAsync().ConfigureAwait(false);
                using StreamReader reader = new(stream);

                var isFileMoved = false;

                //Storing each line in a string builder
                StringBuilder fileContent = new();

                while (!reader.EndOfStream || !isFileMoved)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(line) && IsRegexMatched(line))
                    {
                        var file = await outputFileClient.CreateAsync(int.MaxValue);

                        var outputFile = await outputFileClient.StartCopyAsync(sourceFileClient.Uri);

                        isFileMoved = true;
                    }

                    fileContent.Append(line);
                }

                //Eitherway the file needs to be deleted
                await sourceFileClient.DeleteAsync();
            }
            catch (Exception e)
            {
                log.LogError($"Failed to process file: {myQueueItem.AsString}");
                log.LogError(e.StackTrace);
                throw;
            }
        }

        public static bool IsRegexMatched(string lineToMatch)
        {
            var regexVar = Environment.GetEnvironmentVariable("regexToMatch");
            var regex = string.IsNullOrWhiteSpace(regexVar) ? new Regex("[A-Z]*?") : new Regex(regexVar);

            return regex.Match(lineToMatch).Success;
        }
    }
}
