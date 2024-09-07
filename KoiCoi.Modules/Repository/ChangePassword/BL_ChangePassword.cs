

namespace KoiCoi.Modules.Repository.ChangePassword;

public class BL_ChangePassword
{
    private readonly DA_ChangePassword _daChangePassword;

    public BL_ChangePassword(DA_ChangePassword daChangePassword)
    {
        _daChangePassword = daChangePassword;
    }

    public async Task<Result<OtpPrefixChar>> RequestByEmail(string? Email,int userId,string ipaddress,int maxRetryOTPCount,int maxOTPFailCount,int otpExpireMinute)
    {
        return await _daChangePassword.RequestByEmail(Email,userId, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
    }
    public async Task<Result<string>> ChangePasswordByOTP(ChangePasswordOTPPayload ObjPayload,int maxRetryOTPCount, int maxOTPFailCount, int otpExpireMinute)
    {
        return await _daChangePassword.ChangePasswordByOTP(ObjPayload, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
    }
    public async Task<Result<string>> SaveVertifyEmail(VertifyEmailPayload varEmPay,int maxRetryOTPCount, int maxOTPFailCount, int otpExpireMinute)
    {
        return await _daChangePassword.SaveVertifyEmail(varEmPay, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
    }
}
