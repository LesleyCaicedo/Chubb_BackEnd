using Chubb_Repository.Repository.Asegurado;
using Chubb_Repository.Repository.Seguro;
using Chubb_Service.Service.Asegurado;
using Chubb_Service.Service.Seguro;
using System.Runtime.CompilerServices;

namespace Chubb_BackEnd.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<IAseguradoRepository, AseguradoRepository>();
            services.AddScoped<IAseguradoService, AseguradoService>();
            services.AddScoped<ISeguroRepository, SeguroRepository>();
            services.AddScoped<ISeguroService, SeguroService>();

            return services;
        }
    }
}
