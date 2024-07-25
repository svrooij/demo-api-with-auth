using Microsoft.OpenApi.Models;

namespace Svrooij.Demo.Api;

/// <summary>
/// Extension methods for <see cref="OpenApiOperation"/>
/// </summary>
internal static class OpenApiOperationExtensions
{
    /// <summary>
    /// Add security requirement to the operation
    /// </summary>
    /// <param name="operation">Operation you want to add the security requirement</param>
    /// <param name="schemeName">The `Name` of the <see cref="OpenApiSecurityScheme"/> in AddSwaggerGen</param>
    /// <param name="scope"></param>
    /// <returns></returns>
    internal static OpenApiOperation AddSecurityRequirement(this OpenApiOperation operation, string schemeName, string scope, params string[] extraScopes)
    {
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme{
                    Reference = new OpenApiReference{
                        Id = schemeName, //The name of the previously defined security scheme.
                        Type = ReferenceType.SecurityScheme
                    }
                }, [scope, ..extraScopes]
            }
        });
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

        return operation;
    }
}
