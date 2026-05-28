using HexLabels.Api.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HexLabels.Api.Filters
{
    public class SecurityRequirementsTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any();

            var securityAttributes = context.Description.ActionDescriptor.EndpointMetadata
                .OfType<RequiredScopesAttribute>()
                .ToList();

            if (!hasAuthorize && securityAttributes.Count == 0)
            {
                return Task.CompletedTask;
            }

            operation.Security ??= [];

            if (securityAttributes.Count > 0)
            {
                foreach (var attr in securityAttributes)
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(attr.SecurityScheme, context.Document)] = attr.Scopes
                    });
                }
            }
            else if (hasAuthorize)
            {
                // Default to Bearer if [Authorize] is present but no OpenApiSecurity attribute
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                });
            }

            return Task.CompletedTask;
        }
    }
}
