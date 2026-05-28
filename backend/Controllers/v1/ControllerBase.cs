namespace HexLabels.Api.Controllers.v1
{
  public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
  {
    protected internal Identity GetIdentity()
    {
      var user = HttpContext.User;

      var UserId = Guid.Parse(user.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value ?? throw new Exception("Could not get identity"));

      var companyClaim = user.Claims.FirstOrDefault(c => c.Type == "company_id")?.Value;

      var CompanyId = Guid.Parse(companyClaim ?? throw new Exception("Could not get identity"));

      return new Identity()
      {
        UserId = UserId,
        CompanyId = CompanyId,
        Scopes = [.. user.Claims.Where(c => c.Type == "scope").Select(c => c.Value)]
      };
    }
  }
}
