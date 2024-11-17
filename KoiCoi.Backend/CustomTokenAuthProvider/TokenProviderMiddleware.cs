using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace KoiCoi.Backend.CustomTokenAuthProvider;

public class TokenProviderMiddleware : IMiddleware
{
    //private readonly IRepositoryWrapper _repository;
    private readonly AppDbContext _repository;
    private readonly TokenProviderOptions _options;
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly double _tokenExpireMinute;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SymmetricSecurityKey _tokenencKey;
    private readonly ILogger<TokenProviderMiddleware> Logger;


    public TokenProviderMiddleware(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext repository,
        IConfiguration configuration,
        ILogger<TokenProviderMiddleware> logger
        )
    {
        Logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
        _configuration = configuration;

        _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        double expiretimespan = Convert.ToDouble(_configuration.GetSection("TokenAuthentication:TokenExpire").Value);
        TimeSpan expiration = TimeSpan.FromMinutes(expiretimespan);

        _tokenExpireMinute = _configuration.GetSection("TokenAuthentication:TokenExpire").Get<int>();
        _tokenencKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:TokenEncKey").Get<string>()!));
        _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("TokenAuthentication:SecretKey").Get<string>()!));
        _options = new TokenProviderOptions
        {
            Path = _configuration.GetSection("TokenAuthentication:TokenPath").Get<string>()!,
            Audience = _configuration.GetSection("TokenAuthentication:Audience").Get<string>(),
            Issuer = _configuration.GetSection("TokenAuthentication:Issuer").Get<string>(),
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256),
            Expiration = expiration
        };
    }
    public async Task ResponseMessage(dynamic data, HttpContext context, int code = 400)
    {
        var response = new
        {
            data.status,
            data.message
        };
        context.Response.StatusCode = code;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        context.Response.Headers.Add("server", "");  //added to hide server info for security

        //If public url no need to have token and authorization, add in below list
        if (
            context.Request.Path.ToString().ToLower().Contains("testapi/getinfo") ||
            context.Request.Path.ToString().ToLower().Contains("forgotpassword/requestbyemail") ||
            context.Request.Path.ToString().ToLower().Contains("forgotpassword/changepasswordbyotp") ||
            context.Request.Path.ToString().ToLower().Contains("swagger/") ||//api/v1/User/Testing
            context.Request.Path.ToString().ToLower().Contains("/api/v1/publish/testing") ||
            context.Request.Path.ToString().ToLower().Contains("/api/v1/publish/registeraccount") ||
            context.Request.Path.ToString().ToLower().Contains("/api/v1/publish/accountswithdeviceid") ||
            context.Request.Path.ToString().ToLower().Contains("api/v1/publish/signin") ||
            context.Request.Path.ToString().ToLower().Contains("api/v1/publish/createsocialaccount") ||
            context.Request.Path.ToString().ToLower().Contains("api/v1/publish/checksocialaccount")

        )
        {
            await next(context);
            return;
        }

        if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
        {
            string newToken = await ReGenerateToken(context);
            if (newToken == "-1")
            {
                await ResponseMessage(new { status = "fail", message = "Access Denied" }, context, 401);
            }
            else if (newToken == "-2")
            {
                await ResponseMessage(new { status = "fail", message = "The Token has expired" }, context, 401);
            }
            else if (newToken == "-3")
            {
                await ResponseMessage(new { status = "fail", message = "Access Token Invalid" }, context, 401);
            }
            else if (newToken == "-4")
            {
                await ResponseMessage(new { status = "fail", message = "Force Logout" }, context, 401);
            }
            else if (newToken == "-5")
            {
                await ResponseMessage(new { status = "fail", message = "Password Expire" }, context, 401);
            }
            else if (newToken == "-6")
            {
                await ResponseMessage(new { status = "fail", message = "API Access Denied" }, context, 401);
            }
            else if (newToken != "")
            {
                context.Response.Headers.Add("Access-Control-Expose-Headers", "newToken");
                context.Response.Headers.Add("newToken", newToken);
                await next(context);
            }
            else   // return blank
            {
                await ResponseMessage(new { status = "fail", message = "Token not found" }, context, 401);
            }
        }
        else if (context.Request.Path.ToString().Contains(_options.Path, StringComparison.Ordinal) && context.Request.Method == HttpMethods.Post)
        {
            // Employee Login Validation & Generate Token when valid.
            await GenerateToken(context);
        }
        else
        {
            await ResponseMessage(new { status = "fail", message = "Method Not Allowed." }, context, 405);
        }
    }


    private async Task GenerateToken(HttpContext context)
    {
        LoginDataModel? loginData = new();
        string username = "";
        string password = "";
        string userIdval = "";
        string adminemail = "";
        string _loginType = "";

        try
        {
            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;

            using (var reader = new System.IO.StreamReader(context.Request.Body))
            {
                var request_body = reader.ReadToEnd();
                loginData = JsonConvert.DeserializeObject<LoginDataModel>(request_body, _serializerSettings);
                if (loginData == null)
                    throw new Exception("Invalid login data");

                if (loginData.UserName == null) loginData.UserName = "";
                if (loginData.Password == null) loginData.Password = "";
                if (loginData.UserIdval == null) loginData.UserIdval = "";
                if (loginData.LoginType == null) loginData.LoginType = "";
                if (loginData.Email == null) loginData.Email = "";
                username = loginData.UserName;
                password = AesEncryption.Decrypt(loginData.Password,aseKey,aseIv);
                userIdval = loginData.UserIdval;
                adminemail = loginData.Email;
                _loginType = loginData.LoginType;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Generate Token Invalid Login data");
            await ResponseMessage(new { status = "fail", message = "The user name or password is incorrect." }, context, 400);
            return;
        }

        try
        {
            dynamic loginresult;
            int AdminID;
            string AdminName;
            string AdminEmail;
            int AdminLevelID;

            if (_loginType == "1")
            {
                loginresult = await DoAdminTypeloginValidation(username, userIdval, password);
                if (loginresult.error == 0)
                {
                    loginresult = loginresult.data;
                    AdminID = loginresult.UserId;
                    AdminName = loginresult.Name;
                    AdminEmail = loginresult.Email ?? "";
                    AdminLevelID =  1;
                }
                else
                {
                    await ResponseMessage(new { status = "fail", message = loginresult.message.ToString() }, context, 400);
                    return;
                }
            }
            else
            {
                await ResponseMessage(new { status = "fail", message = "Invalid login type" }, context, 400);
                return;
            }
            string deviceID_InHeader = Convert.ToString(context.Request.Headers["DeviceID"]); // To Check DeviceID for mobile request  
            string firebaseToken = Convert.ToString(context.Request.Headers["FirebaseToken"]); // Get Device Token
            string appVersion = Convert.ToString(context.Request.Headers["AppVersion"]); // Get Device App Versoin
            string osVersion = Convert.ToString(context.Request.Headers["OsVersion"]); // Get Device OS Versoin
            string phoneModel = Convert.ToString(context.Request.Headers["PhoneModel"]); // Get Device Phone Model
            bool isRooted = Convert.ToBoolean(context.Request.Headers["IsRooted"]); // Get IsRooted

            if (deviceID_InHeader is not null && appVersion is not null)
            {
                await SaveFirebaseToken(AdminID, deviceID_InHeader, firebaseToken, appVersion, osVersion, phoneModel, isRooted);
            }
            var now = DateTime.UtcNow;
            var _tokenData = new TokenData
            {
                Sub = AdminName,
                Jti = await _options.NonceGenerator(),
                Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(),
                UserID = AdminID.ToString(),
                UserLevelID = AdminLevelID.ToString(),
                LoginType = _loginType.ToString(),
                TicketExpireDate = now.Add(_options.Expiration)
            };
            var claims = Globalfunction.GetClaims(_tokenData);

            var appIdentity = new ClaimsIdentity(claims);
            context.User.AddIdentity(appIdentity); //add custom identity because default identity has delay to get data in EventLogRepository

            string encodedJwt = CreateEncryptedJWTToken(claims);
            //int LoginUserID = Convert.ToInt32(_tokenData.UserID);

            var response = new
            {
                AccessToken = encodedJwt,
                CurrentVersion = "1.0.0"
                //ExpiresIn = (int)_options.Expiration.TotalSeconds,
                // UserID = AdminID.ToString(),
                //UserIDval = Encryption.EncryptID(AdminID.ToString(), LoginUserID.ToString()),
                //UserID = AdminID.ToString(),
                //LoginType = _loginType,
                //UserLevelID = AdminLevelID,
                //DisplayName = AdminName
            };
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            Logger.LogInformation("Successful login for this account {AdminName}", AdminName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Generate Token Fail");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Generate Token Fail");
        }
    }



    async Task<dynamic> DoAdminTypeloginValidation(string username, string userIdval, string password)
    {
        try
        {
            //var objAdmin = await _repository.Admin.GetAdminByLoginName(username);
            var resultAdmin = await _repository.Users.Where(adm => adm.Name == username && adm.UserIdval == userIdval).ToListAsync();
            if (resultAdmin == null || !resultAdmin.Any())
                throw new ValidationException("Login User " + username + " not found.");

            var objAdmin = resultAdmin.FirstOrDefault();
            if (objAdmin == null)
            {
                throw new ValidationException("Invalid Login User " + username);
            }

            string oldsalt = objAdmin.PasswordHash!;
            string oldhash = objAdmin.Password!;
            bool flag = SaltedHash.Verify(oldsalt, oldhash, password);

            if (flag == false)  //incorrect pwd
            {
                throw new Exception("Incorrect Login info for user account : " + username);
            }
            else
            {
                return new { error = 0, data = objAdmin };
            }
        }
        catch (ValidationException vex)
        {
            Logger.LogWarning(vex, "Invalid Login");
            return new { error = 1, message = vex.Message };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login Fail");
            return new { error = 1, message = ex.ToString() };
        }
    }


    public async Task<string> ReGenerateToken(HttpContext context)
    {
        try
        {
            string access_token = "";
            TokenData _tokenData;

            var hdtoken = context.Request.Headers["Authorization"];
            if (hdtoken.Count > 0)
            {
                access_token = hdtoken[0] ?? "";
                access_token = access_token.Replace("Bearer ", "");
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(access_token,   //this will throw exception if token change or fake token.
                        new TokenValidationParameters  //this is necessary in both startup and here.
                        {
                            // The signing key must match!
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = _signingKey,
                            RequireSignedTokens = true,
                            // Validate the JWT Issuer (iss) claim
                            ValidateIssuer = true,
                            ValidIssuer = _options.Issuer,
                            // Validate the JWT Audience (aud) claim
                            ValidateAudience = false,
                            ValidAudience = _options.Audience,
                            // Validate the token expiry
                            ValidateLifetime = true,
                            // If you want to allow a certain amount of clock drift, set that here:
                            ClockSkew = TimeSpan.Zero,
                            TokenDecryptionKey = _tokenencKey
                        }, out SecurityToken tokenS);

                    var tokenJS = (JwtSecurityToken)tokenS;
                    if (tokenJS.SignatureAlgorithm != "A256KW")   //only allow new encryption alg A256KW
                        throw new Exception("Invalid Algorithm " + tokenJS.SignatureAlgorithm);

                    _tokenData = Globalfunction.GetTokenData(tokenJS);
                }
                catch (SecurityTokenExpiredException)
                {
                    return "-2";  // token expired
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Invalid Access token");
                    return "-3";  // invalid access token
                }
            }
            else
            {
                return "";  //Token not found
            }

            //_tokenData.IPAddress = HttpContextExtensions.GetRemoteIPAddress(context).ToString();
            bool allow = false;

            var pathstr = context.Request.Path.ToString();

            string[] patharr = pathstr.Split('/');

            // add url which need to allow after login success without menu permission
            if (pathstr.ToLower().Contains("fileservice/profilephoto") ||
                pathstr.ToLower().Contains("menu/getadminlevelmenudata") ||
                pathstr.ToLower().Contains("admin/passchange")
            )
            {
                allow = true; //Other File Functions default allow = false
            }
            else
            {
                //allow = true;
                allow = await CheckURLPermission(_tokenData, pathstr);  //check url with regular expression
            }


            if (allow)
            {
                var userObj = await _repository.Users.FindAsync(Convert.ToInt32(_tokenData.UserID));
                if (userObj != null && _tokenData.UserLevelID.Trim() != "")
                {
                    var exp = DateTime.UtcNow;
                    var expires_in = exp.AddMinutes(_tokenExpireMinute).ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'");
                    var now = DateTime.UtcNow;
                    var _newtokenData = new TokenData();
                    _newtokenData.Sub = userObj.Name;
                    _newtokenData.Jti = await _options.NonceGenerator();
                    _newtokenData.Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString();
                    _newtokenData.UserID = userObj.UserId.ToString();
                    //_newtokenData.UserName = userObj.Name;
                    //_newtokenData.UserLevelID = userObj.AdminLevelId.ToString();
                    _newtokenData.TicketExpireDate = now.Add(_options.Expiration);
                    _newtokenData.LoginType = _tokenData.LoginType;

                    //global variable 
                    _tokenData.Sub = userObj.Name;
                    _tokenData.Jti = await _options.NonceGenerator();
                    _tokenData.Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString();
                    _tokenData.UserID = userObj.UserId.ToString();
                    //_tokenData.UserName = userObj.Name;
                    // var _newtokenData = new TokenData()
                    // {
                    //   Sub = userObj.AdminName,
                    //   Jti = await _options.NonceGenerator(),
                    //   Iat = new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(),
                    //   UserID = userObj.AdminId.ToString(),
                    //   UserLevelID = userObj.AdminLevelId.ToString(),
                    //   TicketExpireDate = now.Add(_options.Expiration),
                    //   LoginType = _tokenData.LoginType
                    // };
                    var claims = Globalfunction.GetClaims(_newtokenData);
                    var appIdentity = new ClaimsIdentity(claims);
                    context.User.AddIdentity(appIdentity); //add custom identity because default identity has delay to get data in EventLogRepository

                    string newToken = CreateEncryptedJWTToken(claims);
                    return newToken;
                }
                else
                {
                    return "-1";
                }
            }
            else
            {
                return "-6";  //API Access Denied
            }
        }
        catch (Exception)
        {
            return "-3";  //Invalid Access token
        }
    }

    async Task<bool> CheckURLPermission(TokenData obj, string ServiceURL)
    {
        try
        {
            var userlevelid = int.Parse(obj.UserLevelID);

            if (userlevelid != 0)
            {
                //   var objAdminLevel = await _repository.AdminLevel.FindByIDAsync(userlevelid);

                //   var isadmin = false;
                //   if (objAdminLevel != null)
                //   {
                //     isadmin = objAdminLevel.IsAdministrator;
                //   }
                return true;
                // if (isadmin)
                //     return true;
                // else
                // {
                //     var checkMenuID = await _repository.AdminLevel.CheckAdminLevelAccessURL(userlevelid, ServiceURL);
                //     return checkMenuID;
                // }
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "CheckURLPermission Fail.");
        }
        return false;
    }

    private async Task<string> SaveFirebaseToken(int userId,string deviceID,string firebaseToken,string appVersion,string osVersion,string phoneModel,bool isRooted)
    {
        var tokendata = await _repository.NotificationTokens
                            .Where(x=> x.UserId == userId).FirstOrDefaultAsync();
        if (tokendata is not null)
        {
            tokendata.Token = firebaseToken ?? "";
            tokendata.UserId = userId;
            tokendata.LastActivities = DateTime.UtcNow;
            tokendata.AppVersion = appVersion ?? "";
            tokendata.OsVersion = osVersion ?? "";
            tokendata.PhModel = phoneModel ?? "";
            tokendata.IsRooted = isRooted;

            await _repository.SaveChangesAsync();
        }
        else
        {
            tokendata = new NotificationToken {
                Token = firebaseToken ?? "",
                UserId = userId,
                LastActivities = DateTime.UtcNow,
                AppVersion = appVersion ?? "",
                OsVersion = osVersion ?? "",
                PhModel = phoneModel ?? "",
                IsRooted = isRooted
            };
            await _repository.NotificationTokens.AddAsync(tokendata);
            await _repository.SaveChangesAsync();
        }
        var loginHistory = await _repository.AccountLoginHistories
            .Where(x=> x.UserId == userId && x.DeviceId == deviceID).FirstOrDefaultAsync();
        if(loginHistory is not null)
        {
            loginHistory.AppVersion = appVersion;
            loginHistory.OsVersion = osVersion;
            loginHistory.PhoneModel = phoneModel;
            loginHistory.ModifiedData = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
        }
        else
        {
            AccountLoginHistory newAccount = new AccountLoginHistory
            {
                UserId = userId,
                DeviceId = deviceID,
                AppVersion = appVersion,
                OsVersion = osVersion,
                PhoneModel = phoneModel,
                CreatedData = DateTime.UtcNow,
                ModifiedData = DateTime.UtcNow
            };
            await _repository.AccountLoginHistories.AddAsync(newAccount);
            await _repository.SaveChangesAsync();
        }
        return "success";
}

    private string CreateEncryptedJWTToken(Claim[] claims)
    {
        string encodedJwt = "";
        try
        {
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _options.Audience,
                Issuer = _options.Issuer,
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                IssuedAt = Globalfunction.UnixTimeStampToDateTime(Int32.Parse(claims.First(claim => claim.Type == "iat").Value)),
                Expires = now.Add(_options.Expiration),
                SigningCredentials = _options.SigningCredentials,
                EncryptingCredentials = new EncryptingCredentials(_tokenencKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512)
            };
            var handler = new JwtSecurityTokenHandler();
            encodedJwt = handler.CreateEncodedJwt(tokenDescriptor);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "CreateEncryptedJWTToken Fail");
        }
        return encodedJwt;
    }

}
