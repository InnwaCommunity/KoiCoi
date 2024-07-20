

using KoiCoi.Backend.Controllers;
using KoiCoi.Models.Otp_Dtos;
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
    public async Task<ResponseData> RequestByEmail(ForgotPasswordEmailPayload payload)
    {
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            string Email = payload.Email;
            string ipaddress = Convert.ToString(Globalfunction.GetClientIP(HttpContext));
            ResponseData res = await _blChnagePassword.RequestByEmail(Email,LoginEmpID, ipaddress, _maxRetryOTPCount, _maxOTPFailCount, _otpExpireMinute);
            return res;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }

    [HttpPost("ChangePasswordByOTP", Name = "ChangePasswordByOTP")]
    public async Task<ResponseData> ChangePasswordByOTP(ChangePasswordOTPPayload ObjPayload)
    {
        try
        {
            if (ObjPayload.Password != ObjPayload.ConfirmPassword)
                throw new ValidationException("Password and confirm password not match.");

            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            ObjPayload.LoginId = LoginEmpID;
            return await _blChnagePassword.ChangePasswordByOTP(ObjPayload, _maxRetryOTPCount, _maxOTPFailCount, _otpExpireMinute);

        }
        catch (ValidationException vex)
        {
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = vex.ValidationResult.ErrorMessage;
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }


    [HttpPost("SaveVertifyEmail",Name = "SaveVertifyEmail")]
    public async Task<ResponseData> SaveVertifyEmail(VertifyEmailPayload verEmPay)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        verEmPay.LoginId = LoginEmpID;
        return await _blChnagePassword.SaveVertifyEmail(verEmPay, _maxRetryOTPCount, _maxOTPFailCount, _otpExpireMinute);
    }
}
