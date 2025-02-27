using GracelineCMS.Infrastructure.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GracelineCMS.Controllers
{
    [Route("organization")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = "GlobalAdmin")]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            await Task.CompletedTask;
            return Ok();
        }
    }
}
