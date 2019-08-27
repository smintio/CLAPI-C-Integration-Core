using Microsoft.Extensions.Configuration;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator;
using SmintIo.CLAPI.Consumer.Integration.Core.Authenticator.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs;
using SmintIo.CLAPI.Consumer.Integration.Core.Jobs.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers;
using SmintIo.CLAPI.Consumer.Integration.Core.Providers.Impl;
using SmintIo.CLAPI.Consumer.Integration.Core.Services;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IntegrationCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddSmintIoClapicIntegrationCore(this IServiceCollection services)
        {            
            Console.WriteLine("Initializing CLAPI-C Integration Core...");

            services.AddSingleton<ISyncJob, SyncJobImpl>();

            services.AddSingleton<ISmintIoApiClientProvider, SmintIoApiClientProviderImpl>();            

            services.AddHostedService<TimedSynchronizerService>();
            services.AddHostedService<PusherService>();

            Console.WriteLine("CLAPI-C Integration Core initialized successfully");

            return services;
        }
    }
}
