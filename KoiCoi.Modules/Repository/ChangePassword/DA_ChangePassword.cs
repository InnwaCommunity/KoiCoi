
namespace KoiCoi.Modules.Repository.ChangePassword;

public class DA_ChangePassword
{
    private readonly AppDbContext _db;

    public DA_ChangePassword(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<OtpPrefixChar>> RequestByEmail(string? email, int userid,string ipaddress, int maxRetryOTPCount, int maxOTPFailCount, int otpExpireMinute)
    {
        Result<OtpPrefixChar> model = null;
        try
        {
            var userData = await _db.Users.Where(x => x.UserId == userid).FirstOrDefaultAsync();
            if (userData == null)
                return Result<OtpPrefixChar>.Error("User Not Found");

            if (!string.IsNullOrEmpty(email))
            {
                model = await DoOTPValidationAsync("Vertify Email", userData.Name, email, userid, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
            }
            else
            {
                if (string.IsNullOrEmpty(userData.Email))
                    return Result<OtpPrefixChar>.Error("Login User Email  not found.");

                model = await DoOTPValidationAsync("Reset Password", userData.Name, userData.Email, userid, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
            }

        }
        catch (Exception ex)
        {
            model = Result<OtpPrefixChar>.Error(ex);
        }

        return model;
    }
    private async Task<Result<OtpPrefixChar>> DoOTPValidationAsync(string subject,string LoginName,string Email, int userId, string ipaddress, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        Result<OtpPrefixChar> model = null;
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
                SendDateTime = DateTime.UtcNow,
                FailCount = 0,
                RetryCount = 0,
                CreatedDate = DateTime.UtcNow
            };
        }
        else
        {
            DateTime value_plus_hrs = otpObject.SendDateTime.AddHours(24);
            if (DateTime.UtcNow > value_plus_hrs)
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
            otpObject.SendDateTime = DateTime.UtcNow;
            otpObject.Ipaddress = ipaddress;
            otpObject.FailCount = 0;
            otpObject.RetryCount = otpObject.RetryCount + 1;
            otpObject.LastModifiedDate = DateTime.UtcNow;
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
                model = Result<OtpPrefixChar>.Error("Inform Email Not Found");
            }
            mailinfo!.UseCount = 1 + mailinfo.UseCount;
            await _db.SaveChangesAsync();
            string messagebody = $@"
                                    Dear {LoginName},<br>
                                    Your OTP code to {subject} is {sRandomOTP}";

            Result<string> d=Globalfunction.SendEmailAsync(mailinfo.FromMail,mailinfo.AppPassword, Email, subject, messagebody);
            
            if(d.IsError)
            {
                return Result<OtpPrefixChar>.Error(d.Message);
            }
            model = Result<OtpPrefixChar>.Success( new OtpPrefixChar{
                PrefixChar = sRandomChar
            });
        }
        else
        {
            model = Result<OtpPrefixChar>.Error("You have been reached max OTP code, please try again after 24 hrs");
        }

        return model;
    }

    public async Task<Result<string>> ChangePasswordByOTP(ChangePasswordOTPPayload objPayload, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        Result<string> model = null;
        try
        {
            Globalfunction.CheckPassword(objPayload.ConfirmPassword);
            string otpcode = objPayload.OTPPrefix + "-" + objPayload.OTPPasscode;
            var useinfo = await _db.Users.Where(x=> x.UserId == objPayload.LoginId).FirstOrDefaultAsync();
            if ( useinfo is not null)
            {
                var otpObject = await _db.Otps
                    .Where(x => x.UserId == objPayload.LoginId 
                    && x.EmailPhone == objPayload.Email).FirstOrDefaultAsync();
                if (otpObject is not null)
                {
                    if (otpObject.SendDateTime.AddMinutes(_otpExpireMinute) < DateTime.UtcNow)
                    {
                        return Result<string>.Error("OTP code is expired, please request OTP again.");
                    }
                    string passcode = otpObject.Passcode;
                    // Check otp code
                    if (otpObject.FailCount >= _maxOTPFailCount)
                    {
                        return Result<string>.Error("You have been reached a lot of wrong OTP code, please request OTP again");
                    }

                    if (otpcode == passcode)
                    {
                        otpObject.FailCount = 0;
                        otpObject.RetryCount = 0;
                        otpObject.LastModifiedDate = DateTime.UtcNow;
                        _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();

                        string salt = "";
                        string password = "";
                        salt = SaltedHash.GenerateSalt();
                        password = SaltedHash.ComputeHash(salt, objPayload.Password);

                        useinfo.PasswordHash = salt;
                        useinfo.Password = password;
                        _db.Users.Update(useinfo);
                        await _db.SaveChangesAsync();

                        model = Result<string>.Success("Successfully change password");
                    }
                    else
                    {
                        otpObject.FailCount = otpObject.FailCount + 1;
                        otpObject.LastModifiedDate = DateTime.UtcNow;
                         _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();
                        model = Result<string>.Error("Your OTP code is wrong, please try again.");
                    }
                }
                else
                {
                    model = Result<string>.Error("Invalid  Email");
                }
            }
            else
            {
                model = Result<string>.Error("Invalid Login Name");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.UtcNow + ex.Message);
            model = Result<string>.Error(ex);
        }
        return model;
    }
    public async Task<Result<string>> SaveVertifyEmail(VertifyEmailPayload verEmPay, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        Result<string> model = null;
        try
        {
            string otpcode = verEmPay.OTPPrefix + "-" + verEmPay.OTPPasscode;
            var useinfo = await _db.Users.Where(x=> x.UserId == verEmPay.LoginId).FirstOrDefaultAsync();
            if ( useinfo is not null)
            {
                var otpObject = await _db.Otps
                    .Where(x => x.UserId == verEmPay.LoginId 
                    && x.EmailPhone == verEmPay.Email).FirstOrDefaultAsync();
                if (otpObject is not null)
                {
                    if (otpObject.SendDateTime.AddMinutes(_otpExpireMinute) < DateTime.UtcNow)
                    {
                        return Result<string>.Error("OTP code is expired, please request OTP again.");
                    }
                    string passcode = otpObject.Passcode;
                    // Check otp code
                    if (otpObject.FailCount >= _maxOTPFailCount)
                    {
                        return Result<string>.Error("You have been reached a lot of wrong OTP code, please request OTP again");
                    }

                    if (otpcode == passcode)
                    {
                        otpObject.FailCount = 0;
                        otpObject.RetryCount = 0;
                        otpObject.LastModifiedDate = DateTime.UtcNow;
                        _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();



                        useinfo.Email = verEmPay.Email;
                        _db.Users.Update(useinfo);
                        await _db.SaveChangesAsync();

                        model = Result<string>.Success("Vertify Email Success");
                    }
                    else
                    {
                        otpObject.FailCount = otpObject.FailCount + 1;
                        otpObject.LastModifiedDate = DateTime.UtcNow;
                         _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();
                        model = Result<string>.Error("Your OTP code is wrong, please try again.");
                    }
                }
                else
                {
                    model = Result<string>.Error("Invalid  Email");
                }
            }
            else
            {
                model = Result<string>.Error("Invalid Login Name");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.UtcNow + ex.Message);
            model = Result<string>.Error(ex);
        }

        return model;
    }
}
