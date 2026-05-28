namespace HexLabels.Api.Controllers
{
  public class Identity
  {
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }

    public string[] Scopes { get; set; } = [];
  }
}
