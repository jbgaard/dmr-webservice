using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace DMRWebScrapper_service.Authentication
{
    /// <summary>
    /// API Key authorization attribute that can be applied to controllers or actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var apiKey = configuration.GetValue<string>("AdminApiKey") ?? Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "MyVerySecretApiKey";

            if (!apiKey.Equals(potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

    /// <summary>
    /// Configures Swagger to use the API Key authentication
    /// </summary>
    public class ApiKeyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiKeyAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<ApiKeyAttribute>();

            if (apiKeyAttributes.Any())
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "ApiKey"
                                }
                            },
                            new List<string>()
                        }
                    }
                };
            }
        }
    }
}
