using Microsoft.Extensions.DependencyInjection;

namespace Chat.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddChat(this IServiceCollection service)
        {
            service.AddSignalR(); 
            service.AddSingleton<ChatUserConnectionManager>();

            return service;
        }
    }
}
