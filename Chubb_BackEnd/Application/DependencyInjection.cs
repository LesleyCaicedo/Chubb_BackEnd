using Chubb_Repository.Repository.Asegurado;
using Chubb_Repository.Repository.Cuenta;
using Chubb_Repository.Repository.Seguro;
using Chubb_Repository.Repository.Usuario;
using Chubb_Service.Service.Asegurado;
using Chubb_Service.Service.Cuenta;
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
            services.AddScoped<ICuentaService, CuentaService>();
            services.AddScoped<ICuentaRepository, CuentaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            return services;
        }
    }
}
