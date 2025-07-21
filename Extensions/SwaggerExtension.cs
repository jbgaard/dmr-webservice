using DMRWebScrapper_service.Authentication;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DMRWebScrapper_service.Extensions
{
    public static class SwaggerExtension
    {
        public static void AddApiKeySecurityDefinition(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key authentication using the X-API-KEY header",
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            options.OperationFilter<ApiKeyOperationFilter>();
        }
    }
}
