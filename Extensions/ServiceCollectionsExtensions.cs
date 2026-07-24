using IpGroups.Services.Abstract;
using IpGroups.Services.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace IpGroups.Extensions
{
    public static class ServiceCollectionsExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IBinaService, BinaService>();
            services.AddScoped<IBirimService, BirimService>();
            services.AddScoped<IIpService, IpService>();
            services.AddScoped<IIpAddressService, IpAddressService>();
            services.AddScoped<IPersonService, PersonService>();

            return services;
        }
    }
}
