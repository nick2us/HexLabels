using System.Security.Claims;

namespace HexLabels.Api.Services.OAuth2
{
  public interface IOAuth2Validator
  {

    public void Validate();

    public void SetClaims(List<Claim> claims);

  }
}
