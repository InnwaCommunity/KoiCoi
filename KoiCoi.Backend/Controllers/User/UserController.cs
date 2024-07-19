namespace KoiCoi.Backend.Controllers.User;

[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly BL_User _bLUser;

    public UserController(BL_User blUser)
    {
        _bLUser = blUser;
    }

    [HttpPost("RegisterAccount", Name = "RegisterAccount")]
    public async Task<IActionResult> RegisterAccount(RequestUserDto requestUser)
    {
        var respo = await _bLUser.CreateAccount(requestUser);
        return Ok(respo);
    }

    [HttpPost("UpdateUserInfo", Name = "UpdateUserInfo")]
    public async Task<IActionResult> UpdateUserInfo(RequestUserDto requestUser)
    {
        var respo = await _bLUser.UpdateUserInfo(requestUser);
        return Ok(respo);
    }

    [HttpGet()]
    public async Task<IActionResult> SearchUser()
    {
        var agentList = await _bLUser.GetStatusType();

        return Ok(agentList);
    }
}
