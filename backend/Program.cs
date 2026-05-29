using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;

namespace HexLabels.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddControllers();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi("v1", options =>
            {
                options.AddOperationTransformer<Filters.SecurityRequirementsTransformer>();

                options.AddOperationTransformer((operation, context, cancellationToken) =>
                {
                    // Remove the version parameter from the operation
                    var versionParameter = operation.Parameters?.FirstOrDefault(p =>
                      p.Name.Equals("version", StringComparison.OrdinalIgnoreCase));

                    if (versionParameter != null)
                    {
                        operation.Parameters?.Remove(versionParameter);
                    }

                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                    };

                    document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            ClientCredentials = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri("/api/v1/oauth/token", UriKind.Relative),
                                Scopes = Services.OAuth2.Scopes.Descriptions
                            }
                        }
                    };

                    return Task.CompletedTask;
                });
            });

            builder.Services.AddAuthorization();
            // JWT Authentication
            var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "hexlables",
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "hexlables-api",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                    };
                });

            builder.Services.AddScoped<Services.OAuth2Service>();
            builder.Services.AddDbContextPool<Core.Data.Contexts.DatabaseContext>(opt =>
            {
                opt.UseNpgsql(builder.Configuration.GetConnectionString("LocalhostConnection"));
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
