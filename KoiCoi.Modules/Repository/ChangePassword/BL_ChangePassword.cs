
namespace KoiCoi.Modules.Repository.ChangePassword;

public class BL_ChangePassword
{
    private readonly DA_ChangePassword _daChangePassword;

    public BL_ChangePassword(DA_ChangePassword daChangePassword)
    {
        _daChangePassword = daChangePassword;
    }

    public async Task<ResponseData> RequestByEmail(string email,int userId,string ipaddress,int maxRetryOTPCount,int maxOTPFailCount,int otpExpireMinute)
    {
        return await _daChangePassword.RequestByEmail(email, userId, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
    }

    
}
