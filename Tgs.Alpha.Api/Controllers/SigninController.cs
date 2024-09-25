using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Alpha.Api.Requests;

namespace Tgs.Alpha.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SigninController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<SsoModel> GetSsoUrl(
            [FromBody] SsoSignRequest request)
        {
            return await mediator.Send(request);
        }
    }
}
