using Asp.Versioning;
using HexLabels.Api.Core.Data.Contexts;
using HexLabels.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HexLabels.Api.Controllers.v1.OAuth2
{

    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiVersion("1.0")]
    public class OAuth2Controller(DatabaseContext databaseContext, IConfiguration configuration, OAuth2Service oAuth2Service) : Controller
    {
        private readonly DatabaseContext databaseContext = databaseContext;
        private readonly IConfiguration configuration = configuration;
        private readonly OAuth2Service oAuth2Service = oAuth2Service;

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromForm][Required] string grant_type, [FromForm][Required] string client_id, [FromForm][Required] string client_secret, [FromForm][Required] string scope)
        {
            if (grant_type != "client_credentials")
            {
                return BadRequest(new { error = "unsupported_grant_type", error_description = "Only client_credentials grant type is supported" });
            }

            return await oAuth2Service.VerifyAndGenerate(client_id, client_secret, scope.Split(',').Select(p => p.Trim()).ToArray() ?? [], configuration, databaseContext);
        }
    }
}
