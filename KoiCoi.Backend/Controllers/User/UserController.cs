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
    public async Task<Result<string>> UploadUserProfile([FromForm] IFormFile files)
    {
        try
        {
            if (files != null && files.Length > 0)
            {

                int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
                return await _bLUser.UploadUserProfile(files, LoginEmpID);
            }
            else
            {
                return Result<string>.Error("No file uploaded.");
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
    }

    [HttpGet("GetUserTypes",Name = "GetUserTypes")]
    public async Task<Result<List<UserTypeResponse>>> GetUserTypes()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.GetUserTypes(LoginEmpID);
    }
    [HttpGet("GetLoginUserInfo",Name = "GetLoginUserInfo")]
    public async Task<Result<LoginUserInfo>> GetLoginUserInfo()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.GetLoginUserInfo(LoginEmpID);
    }

    [HttpPost("ChangeLoginPassword", Name = "ChangeLoginPassword")]
    public async Task<Result<string>> ChangeLoginPassword(ChangePasswordPayload paylod)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.ChangeLoginPassword(paylod, LoginEmpID);
    }

    [HttpPost("GetUserProfile",Name ="GetUserProfile")]
    public async Task<Result<string>> GetUserProfile(GetUserData payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _bLUser.GetUserProfile(payload,LoginEmpID);
    }


    //[HttpGet("GetAccountByVertifiedEmail/{email}/{OTPPasscode}/{OTPPrefix}", Name = "GetAccountByVertifiedEmail")]
    //public async Task<Result<UserInfoResponse>> GetAccountByVertifiedEmail(string email,string OTPPasscode,string OTPPrefix)
    //{
    //int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
    //    return await _bLUser.GetAccountByVertifiedEmail(email, OTPPasscode, OTPPrefix, LoginEmpID);
    //}
}
