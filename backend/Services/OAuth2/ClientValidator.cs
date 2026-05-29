using System.Security.Claims;
using HexLabels.Api.Core.Data.Contexts;
using HexLabels.Api.Core.Data.Exceptions;
using HexLabels.Api.Core.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace HexLabels.Api.Services.OAuth2
{

    public class UserHasNoAccessException() : NoAcessException("");
    public class ClientValidadtor(string[] scope, string client_id, string client_secret, DatabaseContext database) : IOAuth2Validator
    {

        private Guid ClientID { get; set; } = new Guid(client_id);
        private Guid ClientSecret { get; set; } = new Guid(client_secret);
        private DatabaseContext Database { get; set; } = database;
        private string[] Scope { get; set; } = scope;

        private Company? Company { get; set; }
        private Department? Department { get; set; }

        public int GetScopeSize() => Scope.Length;

        public void SetClaims(List<Claim> claims)
        {
            foreach (var scope in Scope)
            {
                claims.Add(new Claim("scope", scope));
            }
            claims.Add(new Claim("user_id", ClientID.ToString()));
            claims.Add(new Claim("company_id", Company?.ID.ToString() ?? ""));
            claims.Add(new Claim("department_id", Department?.ID.ToString() ?? ""));
        }

        public void Validate()
        {

            User user = Database.Users.Include(u => u.UserRole).Include(u => u.Departments).Where(u => u.ID == ClientID).FirstOrDefault() ?? throw new UserHasNoAccessException();

            ApiKey key = Database.APIKeys.Include(a => a.Company).Where(k => k.Key == ClientSecret).FirstOrDefault() ?? throw new UserHasNoAccessException();

            Company = key.Company ?? throw new CompanyNotFoundException(null);

            UserRoles ur = user.UserRole.Where(ur => ur.Company.ID == Company.ID).FirstOrDefault() ?? throw new UserHasNoAccessException();

            Department = ur.Department ?? throw new DepartmentNotFoundException(null);

            Scope = Scopes.ValidScopes(Scope, ur.Role!);

        }


    }
}
