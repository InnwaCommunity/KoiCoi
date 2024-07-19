

using KoiCoi.Backend.Controllers;
using Org.BouncyCastle.Utilities.Encoders;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

[Route("api/[controller]")]
[ApiController]
public class ChangePasswordController : BaseController
{
    private readonly BL_ChangePassword _blChnagePassword;
    private readonly IConfiguration _configuration;
    private readonly int _maxOTPFailCount;
    private readonly int _maxRetryOTPCount;
    private readonly int _otpExpireMinute;

    public ChangePasswordController(BL_ChangePassword blChnagePassword, IConfiguration configuration)
    {
        _blChnagePassword = blChnagePassword;
        _configuration = configuration;
        _maxRetryOTPCount = _configuration.GetSection("appSettings:MaxRetryOTPCount").Get<int>();
        _maxOTPFailCount = _configuration.GetSection("appSettings:OTPFailCount").Get<int>();
        _otpExpireMinute = _configuration.GetSection("appSettings:OTPExpireMinute").Get<int>();
    }
    [HttpPost("RequestByEmail", Name = "RequestByEmail")]
    public async Task<ResponseData> RequestByEmail(ForgotPasswordEmailPayload ObjPayload)
    {
        string Email = "";
        try
        {
            Email = ObjPayload.Email;
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            if (!string.IsNullOrEmpty(Email))
            {
                string ipaddress = Convert.ToString(Globalfunction.GetClientIP(HttpContext));
                ResponseData res = await _blChnagePassword.RequestByEmail(Email, LoginEmpID, ipaddress,_maxRetryOTPCount, _maxOTPFailCount, _otpExpireMinute);
                return res;
            }
            else
            {
                throw new ValidationException("Please enter  Email");
            }
        }
        catch (ValidationException vex)
        {
            ResponseData data= new ResponseData();
            data.StatusCode = 0;
            data.Message = vex.ValidationResult.ErrorMessage;
            // Logger.LogWarning(vex, "Forgot Password Fail {LoginName}, {Email}", LoginName, Email);
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            // _repositoryWrapper.EventLog.Error("Error in Delete Assign Comment", ex.Message);
            // Logger.LogError(ex, "Forgot Password Fail {LoginName}, {Email}", LoginName, Email);
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = "Forgot Password Fail";
            return data;
        }
    }
}
