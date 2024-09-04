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

    [HttpGet("Testing", Name = "Testing")]
    public IActionResult Testing()
    {
        return Ok(new { data = "Hello, this is a DB testing response!" });
    }

    [HttpPost("RegisterAccount", Name = "RegisterAccount")]
    public async Task<IActionResult> RegisterAccount(RequestUserDto requestUser)
    {
        var respo = await _bLUser.CreateAccount(requestUser);
        return Ok(respo);
    }

    [HttpGet("AccountsWithDeviceId/{deviceId}",Name = "AccountsWithDeviceId")]
    public async Task<Result<List<UserLoginAccounts>>> AccountsWithDeviceId(string deviceId)
    {
        return await _bLUser.AccountsWithDeviceId(deviceId);
    }

    [HttpPost("UpdateUserInfo", Name = "UpdateUserInfo")]
    public async Task<IActionResult> UpdateUserInfo(RequestUserDto requestUser)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        var respo = await _bLUser.UpdateUserInfo(requestUser,LoginEmpID);
        return Ok(respo);
    }

    [HttpGet("FindUserByName/{username}", Name = "FindUserByName")]
    public async Task<Result<List<UserInfoResponse>>> FindUserByName(string username)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.FindUserByName(username,LoginEmpID);
    }

    [HttpDelete("DeleteLoginUser",Name = "DeleteLoginUser")]
    public async Task<Result<string>> DeleteLoginUser()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.DeleteLoginUser(LoginEmpID);
    }

    [HttpPost("UploadUserProfile",Name = "UploadUserProfile")]
    public async Task<Result<string>> UploadUserProfile(UploadUserProfileReqeust payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.UploadUserProfile(payload,LoginEmpID);
    }

    [HttpGet("GetUserTypes",Name = "GetUserTypes")]
    public async Task<Result<List<UserTypeResponse>>> GetUserTypes()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.GetUserTypes(LoginEmpID);
    }

    [HttpPost("Signin", Name = "Signin")]
    public async Task<Result<ResponseUserDto>> Signin(LoginPayload paylod)
    {
        return await _bLUser.Signin(paylod);
    }
    [HttpGet("GetLoginUserInfo",Name = "GetLoginUserInfo")]
    public async Task<Result<LoginUserInfo>> GetLoginUserInfo()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.GetLoginUserInfo(LoginEmpID);
    }


    //[HttpGet("GetAccountByVertifiedEmail/{email}/{OTPPasscode}/{OTPPrefix}", Name = "GetAccountByVertifiedEmail")]
    //public async Task<Result<UserInfoResponse>> GetAccountByVertifiedEmail(string email,string OTPPasscode,string OTPPrefix)
    //{
    //int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
    //    return await _bLUser.GetAccountByVertifiedEmail(email, OTPPasscode, OTPPrefix, LoginEmpID);
    //}
}
