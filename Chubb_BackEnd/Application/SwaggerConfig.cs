using Microsoft.OpenApi.Models;

namespace Chubb_BackEnd.Application
{
    public static class SwaggerConfig
    {
        public static void AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Chubb API",
                    Version = "v1",
                    Description = "Backend aplicativo chubb prueba",
                    Contact = new OpenApiContact
                    {
                        Name = "Chubb",
                        Email = "application@chubb.com",
                        Url = new Uri("https://chubb.com")
                    }
                });

                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Ingrese el token en el campo correspondiente",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                };

                c.AddSecurityRequirement(securityRequirement);
            });
        }
    }
}
