namespace HexLabels.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredScopesAttribute : Attribute
    {
        public string SecurityScheme { get; }
        public List<string> Scopes { get; }

        public RequiredScopesAttribute(string securityScheme, params string[] scopes)
        {
            SecurityScheme = securityScheme;
            Scopes = (scopes ?? []).ToList();
        }
    }
}
