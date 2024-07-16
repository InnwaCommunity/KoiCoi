

namespace KoiCoi.Backend.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly BL_User _bLUser;

    public UserController(BL_User _blUser)
    {
        _bLUser = _blUser;
    }

    [HttpGet()]
    public async Task<IActionResult> SearchUser()
    {
        var agentList = await _bLUser.GetStatusType();

        return Ok(agentList);
    }
}
