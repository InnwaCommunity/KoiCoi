using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Login_Models;

public class TokenData
{
    public string Sub = "";  //Required Field, Used for core JWT
    public string Jti = ""; //Required Field, Used for core JWT
    public string Iat = ""; //Required Field, Used for core JWT
    public string UserID = "";
    public string UserName = "";
    public string LoginType = "";
    public string UserLevelID = "";
    public string IPAddress = "";

    public bool isAdmin = false;

    public DateTime TicketExpireDate = DateTime.UtcNow;
}
