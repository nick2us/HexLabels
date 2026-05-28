
using HexLabels.Api.Core.Data.Models;

namespace HexLabels.Api.Services.OAuth2
{
    public static class Scopes
    {
        public const string CompanyWrite = "company:write";
        public const string CompanyRead = "company:read";

        public const string DocumentsSend = "document:send";
        public const string DocumentsRead = "document:read";


        internal static readonly List<string> All = [CompanyWrite, CompanyRead, DocumentsRead, DocumentsSend];

        private static readonly List<string> AdminRights = [CompanyWrite];

        private static readonly List<string> SupportedCompanyScopes = [CompanyWrite, CompanyRead, DocumentsRead, DocumentsSend];


        public static readonly Dictionary<string, string> Descriptions = new()
        {
            { CompanyRead, "Read company data" },
            { CompanyWrite, "Write company data" },
            { DocumentsRead, "Read documents" },
            { DocumentsSend, "Send documents" }
        };

        public static string[] ValidScopes(string[] scopes, UserRoleTypes userRole)
        {
            List<string> scope = [.. scopes];

            scope = [.. scope.Intersect(SupportedCompanyScopes)];

            if (userRole != UserRoleTypes.Admin)
            {
                scope = [.. scope.Except(AdminRights)];
            }

            if (!UserRoleTypes.AllowedToSend.HasFlag(userRole))
            {
                scope = [.. scope.Except([DocumentsSend])];
            }


            return [.. scope];
        }
    }
}
