using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Login_Models;

public class TokenData
{
    public string Sub = "";  //Required Field, Used for core JWT, The subject of the token 
    public string Jti = ""; //Required Field, Used for core JWT, Unique identifier for the JWT. Can be used to prevent the JWT from being replayed. This is helpful for a one time use token.
    public string Iat = ""; //Required Field, Used for core JWT, The time the JWT was issued. Can be used to determine the age of the JWT
    public string UserID = "";
    public string LoginType = "";
    public string UserLevelID = "";
    public string CustomerID = "";
    public string Browser = "";
    public string IPAddress = "";
    public string LoginUserId = "";
    public string Url = "";

    public string isMobile = "";

    public string customDateFormat = "";
    public DateTime TicketExpireDate = DateTime.UtcNow;

    public string Email = "";
}
