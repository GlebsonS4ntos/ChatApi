using ChatCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Extensions
{
    public static class ServiceCollectionExtension
    { 
        public static IServiceCollection AddChat(this IServiceCollection service)
        {
            service.AddSignalRCore();
            service.AddSingleton<ChatUserConnectionManager>();

            return service;
        }
    }
}
