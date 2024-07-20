
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.ChangePassword;

public class DA_ChangePassword
{
    private readonly AppDbContext _db;

    public DA_ChangePassword(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseData> RequestByEmail(string? email, int userid,string ipaddress, int maxRetryOTPCount, int maxOTPFailCount, int otpExpireMinute)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x => x.UserId == userid).FirstOrDefaultAsync();
            if (userData == null)
                throw new ValidationException("Login User  not found.");

            if (!string.IsNullOrEmpty(email))
            {
                responseData = await DoOTPValidationAsync("Vertify Email", userData.Name, email, userid, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
            }
            else
            {
                if (string.IsNullOrEmpty(userData.Email))
                    throw new ValidationException("Login User Email  not found.");

                responseData = await DoOTPValidationAsync("Reset Password", userData.Name, userData.Email, userid, ipaddress, maxRetryOTPCount, maxOTPFailCount, otpExpireMinute);
            }
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
    private async Task<ResponseData> DoOTPValidationAsync(string subject,string LoginName,string Email, int userId, string ipaddress, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
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
                                    Your OTP code to {subject} is {sRandomOTP}";

            ResponseData d=Globalfunction.SendEmailAsync(mailinfo.FromMail,mailinfo.AppPassword, Email, subject, messagebody);
            
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

    public async Task<ResponseData> ChangePasswordByOTP(ChangePasswordOTPPayload objPayload, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        ResponseData resdata = new ResponseData();
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
                    if (otpObject.SendDateTime.AddMinutes(_otpExpireMinute) < DateTime.Now)
                    {
                        throw new ValidationException("OTP code is expired, please request OTP again.");
                    }
                    string passcode = otpObject.Passcode;
                    // Check otp code
                    if (otpObject.FailCount >= _maxOTPFailCount)
                    {
                        throw new ValidationException("You have been reached a lot of wrong OTP code, please request OTP again");
                    }

                    if (otpcode == passcode)
                    {
                        otpObject.FailCount = 0;
                        otpObject.RetryCount = 0;
                        otpObject.LastModifiedDate = DateTime.Now;
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

                        resdata.StatusCode = 1;
                        resdata.Message = "Successfully change password";
                        return resdata;
                    }
                    else
                    {
                        otpObject.FailCount = otpObject.FailCount + 1;
                        otpObject.LastModifiedDate = DateTime.Now;
                         _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();
                        throw new ValidationException("Your OTP code is wrong, please try again.");
                    }
                }
                else
                {
                    throw new ValidationException("Invalid  Email");
                }
            }
            else
            {
               throw new ValidationException("Invalid Login Name");
            }
        }
        catch (ValidationException vex)
        {
            resdata.StatusCode = 0;
            resdata.Message = vex.ValidationResult.ErrorMessage;
            return resdata;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            resdata.StatusCode = 0;
            resdata.Message = ex.Message;
            return resdata;
        }
    }
    public async Task<ResponseData> SaveVertifyEmail(VertifyEmailPayload verEmPay, int _maxRetryOTPCount, int _maxOTPFailCount, int _otpExpireMinute)
    {
        ResponseData resdata = new ResponseData();
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
                    if (otpObject.SendDateTime.AddMinutes(_otpExpireMinute) < DateTime.Now)
                    {
                        throw new ValidationException("OTP code is expired, please request OTP again.");
                    }
                    string passcode = otpObject.Passcode;
                    // Check otp code
                    if (otpObject.FailCount >= _maxOTPFailCount)
                    {
                        throw new ValidationException("You have been reached a lot of wrong OTP code, please request OTP again");
                    }

                    if (otpcode == passcode)
                    {
                        otpObject.FailCount = 0;
                        otpObject.RetryCount = 0;
                        otpObject.LastModifiedDate = DateTime.Now;
                        _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();



                        useinfo.Email = verEmPay.Email;
                        _db.Users.Update(useinfo);
                        await _db.SaveChangesAsync();

                        resdata.StatusCode = 1;
                        resdata.Message = "Vertify Email Success";
                        return resdata;
                    }
                    else
                    {
                        otpObject.FailCount = otpObject.FailCount + 1;
                        otpObject.LastModifiedDate = DateTime.Now;
                         _db.Otps.Update(otpObject);
                        await _db.SaveChangesAsync();
                        throw new ValidationException("Your OTP code is wrong, please try again.");
                    }
                }
                else
                {
                    throw new ValidationException("Invalid  Email");
                }
            }
            else
            {
               throw new ValidationException("Invalid Login Name");
            }
        }
        catch (ValidationException vex)
        {
            resdata.StatusCode = 0;
            resdata.Message = vex.ValidationResult.ErrorMessage;
            return resdata;
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            resdata.StatusCode = 0;
            resdata.Message = ex.Message;
            return resdata;
        }
    }
}
