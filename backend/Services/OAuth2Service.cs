using HexLabels.Api.Core.Data.Contexts;
using HexLabels.Api.Services.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HexLabels.Api.Services
{
    public class OAuth2Service
    {

        public OAuth2Service()
        {
        }

        public Task<IActionResult> VerifyAndGenerate(string client_id, string client_secret, string[] scope, IConfiguration configuration, DatabaseContext database)
        {

            var validator = GetValidator(scope, client_id, client_secret, database);

            try
            {
                validator.Validate();
            }
            catch
            {
                return Task.FromResult<IActionResult>(new UnauthorizedResult());
            }


            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, client_id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };


            validator.SetClaims(claims);

            var jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? "hexlables";
            var jwtAudience = configuration["Jwt:Audience"] ?? "hexlables-api";
            var jwtExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult<IActionResult>(new OkObjectResult(new
            {
                access_token = tokenString,
                token_type = "Bearer",
                expires_in = jwtExpirationMinutes * 60,
            }));
        }

        public IOAuth2Validator GetValidator(string[] scope, string client_id, string client_secret, DatabaseContext database)
        {
            //if (AdminScopes(scope))
            //{
            //    return new AdminValidator();
            //}
            //else if (OperatorScopes(scope))
            //{
            //    return new OperatorValidator();
            //}
            //else if (ApplicationScopes(scope))
            //{
            //    return new ApplicationValidator();
            //}
            //else
            //{
            return new ClientValidadtor(scope, client_id, client_secret, database);
            //}
        }

        public bool OperatorScopes(string[] scopes)
        {
            return scopes.Any(s => s.StartsWith("operator:"));
        }

        public bool AdminScopes(string[] scopes)
        {
            return scopes.Any(s => s.StartsWith("admin:"));
        }

        public bool ApplicationScopes(string[] scopes)
        {
            return scopes.Any(s => s.StartsWith("app:"));
        }

    }
}
