using System;
using CubeFileProsessor.Constants;
using CubeFileProsessor.Factories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(CubeFileProsessor.Startup))]
namespace CubeFileProsessor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IQueueClientFactory>((s) =>
            {
                return new QueueClientFactory(Environment.GetEnvironmentVariable(ConfigConstants.saConnectStr));
            });
        }
    }
}