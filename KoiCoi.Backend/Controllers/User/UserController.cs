namespace KoiCoi.Backend.Controllers.User;

[Route("api/v1/[controller]")]
[ApiController]
public class UserController : BaseController
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
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        var respo = await _bLUser.UpdateUserInfo(requestUser,LoginEmpID);
        return Ok(respo);
    }

    [HttpGet("FindUserByName/{username}", Name = "FindUserByName")]
    public async Task<Result<List<UserInfoResponse>>> FindUserByName(string username)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _bLUser.FindUserByName(username,LoginEmpID);
    }

    [HttpDelete("DeleteLoginUser",Name = "DeleteLoginUser")]
    public async Task<Result<string>> DeleteLoginUser()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _bLUser.DeleteLoginUser(LoginEmpID);
    }

    [HttpPost("UploadUserProfile",Name = "UploadUserProfile")]
    public async Task<Result<string>> UploadUserProfile(UploadUserProfileReqeust payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _bLUser.UploadUserProfile(payload,LoginEmpID);
    }

    [HttpGet("GetUserTypes",Name = "GetUserTypes")]
    public async Task<Result<List<UserTypeResponse>>> GetUserTypes()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _bLUser.GetUserTypes(LoginEmpID);
    }
}
