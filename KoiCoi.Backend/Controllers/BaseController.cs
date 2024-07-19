using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace KoiCoi.Backend.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
//[ApiController]
public class BaseController : Controller
{
    public TokenData _tokenData = new TokenData();
    public string _ipaddress = "";
    public string _clienturl = "";
    public ActionExecutingContext _actionExecutionContext;

    [NonAction]
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        
        try
        {
            base.OnActionExecuting(context);
            _actionExecutionContext = context;
            string Source = context.HttpContext.Request.Path.ToString().Split(new char[] { '/' })[1].ToString();
            string ControllerAction = context.ActionDescriptor.DisplayName.ToString().Replace(" (ESS)", "");

            Request.HttpContext.Session.Set("ApiSource", System.Text.Encoding.ASCII.GetBytes(Source));
            Request.HttpContext.Session.Set("ControllerAction", System.Text.Encoding.ASCII.GetBytes(ControllerAction));

            _ipaddress = "127.0.0.1";
            _ipaddress = Convert.ToString(HttpContextExtensions.GetRemoteIPAddress(context));
            _clienturl = context.HttpContext.Request.Headers["Referer"].ToString() == "" ? context.HttpContext.Request.Headers["myOrigin"].ToString() : context.HttpContext.Request.Headers["Referer"].ToString();
            ClaimsIdentity objclaim = context.HttpContext.User.Identities.Last();
            //if (objclaim.Claims.Count() >= 1)
            //{
                _tokenData.LoginType = objclaim.FindFirst("LoginType").Value;
                _tokenData.UserLevelID = objclaim.FindFirst("UserLevelID").Value;
                _tokenData.Sub = objclaim.FindFirst(JwtRegisteredClaimNames.Sub).Value;
                _tokenData.LoginEmpID = objclaim.FindFirst("UserID").Value;
                //_tokenData.IPAddress = objclaim.FindFirst("IPAddress").Value;
                //_tokenData.Browser = objclaim.FindFirst("Browser").Value;
                //_tokenData.CustomerID = objclaim.FindFirst("CustomerID").Value;
                //_tokenData.Url = objclaim.FindFirst("Url").Value;
                _tokenData.TicketExpireDate = DateTime.Parse(objclaim.FindFirst("TicketExpireDate").Value);
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine("setDefaultDataFromToken Base Controller" + ex.Message);
        }
    }
}
