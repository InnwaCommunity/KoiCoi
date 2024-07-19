
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.ChangePassword;

public class DA_ChangePassword
{
    private readonly AppDbContext _db;

    public DA_ChangePassword(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseData> RequestByEmail(string email,int userid,string ipaddress, int maxRetryOTPCount, int maxOTPFailCount, int otpExpireMinute)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x => x.UserId == userid).FirstOrDefaultAsync();
            if (userData == null)
                throw new ValidationException("Login User  not found.");

            if(userData.Email != email)
                throw new ValidationException("Login User Email  not found.");

            responseData= await DoOTPValidationAsync(userData.Name, email, userid, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
            return responseData;

        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }
    private async Task<ResponseData> DoOTPValidationAsync(string LoginName,string Email, int userId, string ipaddress, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        ResponseData responseData = new ResponseData();
        string[] OTPAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        string[] PrefixAllowedCharacters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        var otpObject = _db.Otps.Where(x=> x.UserId == userId && x.EmailPhone == Email).SingleOrDefault();
        if (otpObject == null)
        {
            otpObject = new()
            {
                EmailPhone = Email,
                UserId = userId,
                Otptoken = "otptoken",
                SendDateTime = DateTime.Now,
                FailCount = 0,
                RetryCount = 0,
                CreatedDate = DateTime.Now
            };
        }
        else
        {
            DateTime value_plus_hrs = otpObject.SendDateTime.AddHours(24);
            if (DateTime.Now > value_plus_hrs)
            {
                otpObject.RetryCount = 0;
                otpObject.FailCount = 0;
            }
        }
        // string passcode = otpObject.passcode;
        // string prefix_char = passcode.Substring(0, 1);

        string RandomCharString = Globalfunction.GenerateRandomOTP(6, OTPAllowedCharacters);
        string sRandomChar = Globalfunction.GenerateRandomChar(PrefixAllowedCharacters);
        string sRandomOTP = sRandomChar + "-" + RandomCharString;

        if (otpObject.RetryCount < _maxRetryOTPCount)
        {
            otpObject.Passcode = sRandomOTP;
            otpObject.SendDateTime = DateTime.Now;
            otpObject.Ipaddress = ipaddress;
            otpObject.FailCount = 0;
            otpObject.RetryCount = otpObject.RetryCount + 1;
            otpObject.LastModifiedDate = DateTime.Now;
            if (otpObject.Otpid > 0)
            {
                //_repositoryWrapper.OTP.Update(otpObject);
                _db.Otps.Update(otpObject);
            }
            else
            {
                await _db.Otps.AddAsync(otpObject);
            }
            await _db.SaveChangesAsync();
            var mailinfo = await _db.InformMails.FirstOrDefaultAsync();
            if(mailinfo == null)
            {
                responseData.StatusCode = 0;
                responseData.Message = "Inform Email Not Found";
            }
            mailinfo!.UseCount = 1 + mailinfo.UseCount;
            await _db.SaveChangesAsync();

            string messagebody = $@"
                                    Dear {LoginName},<br>
                                    Your OTP code to reset your password is {sRandomOTP}";

            ResponseData d=Globalfunction.SendEmailAsync(mailinfo.FromMail,mailinfo.AppPassword, Email, "Forgot Password", messagebody);
            //Console.WriteLine(messagebody);
            if(d.StatusCode == 0)
            {
                return d;
            }
            responseData.StatusCode = 1;
            responseData.Message = "Successful generate OTP code";
            responseData.Data = new { prefix_char = sRandomChar };
            return responseData;
        }
        else
        {
            responseData.StatusCode = 0;
            responseData.Message = "You have been reached max OTP code, please try again after 24 hrs";
            return responseData;
        }
    }
    

}
