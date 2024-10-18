namespace KoiCoi.Backend.Controllers.PublishControllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        private readonly BL_User _bLUser;

        public PublishController(BL_User blUser)
        {
            _bLUser = blUser;
        }

        [HttpGet("Testing", Name = "Testing")]
        public IActionResult Testing()
        {
            return Ok(new { data = "Hello, this is a DB testing response!" });
        }

        [HttpPost("RegisterAccount", Name = "RegisterAccount")]
        public async Task<IActionResult> RegisterAccount(RequestUserDto requestUser)
        {
            var respo = await _bLUser.CreateAccount(requestUser);
            return Ok(respo);
        }

        [HttpGet("AccountsWithDeviceId/{deviceId}", Name = "AccountsWithDeviceId")]
        public async Task<Result<List<UserLoginAccounts>>> AccountsWithDeviceId(string deviceId)
        {
            return await _bLUser.AccountsWithDeviceId(deviceId);
        }

        [HttpPost("RemoveLoginAccount", Name = "RemoveLoginAccount")]
        public async Task<Result<string>> RemoveLoginAccount(RemoveLoginAccountPayload payload)
        {
            return await _bLUser.RemoveLoginAccount(payload);
        }

        [HttpPost("Signin", Name = "Signin")]
        public async Task<Result<ResponseUserDto>> Signin(LoginPayload paylod)
        {
            return await _bLUser.Signin(paylod);
        }
    }
}
