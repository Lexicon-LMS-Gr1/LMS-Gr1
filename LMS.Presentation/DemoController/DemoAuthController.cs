using LMS.Shared.DTOs.Demo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.DemoController;

[Route("api/demoauth")]
[ApiController]
public class DemoAuthController : ControllerBase
{
    [HttpGet]
    [Authorize]
    [SwaggerOperation(
        Summary =     "Hämta autentiserade demoanvändare.",
        Description = "Returnerar en lista med demoanvändare. Kräver en giltig JWT-token.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista med demoanvändare", typeof(IEnumerable<DemoAuthDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Obehörig - JWT-token saknas eller är ogiltig.")]
    public IActionResult GetDemoAuth()
    {
        return Ok(new[]{new DemoAuthDto(1, "Kalle"),
                        new DemoAuthDto(2, "Anka" ),
                        new DemoAuthDto(3, "Nisse"),
                        new DemoAuthDto(4, "Pelle")}
        );
    }
}
