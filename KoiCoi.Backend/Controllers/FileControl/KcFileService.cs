namespace KoiCoi.Backend.Controllers.FileControl;

[Route("api/[controller]")]
[ApiController]
public class KcFileService : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_File _blFile;

    public KcFileService(BL_File blFile, IConfiguration configuration)
    {
        _blFile = blFile;
        _configuration = configuration;
    }

    [HttpPost("GetFiles", Name = "GetFiles")]
    public async Task<IActionResult> GetFiles(GetFilePayload paylod)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<string> response = await _blFile.GetFiles(paylod, LoginUserId);
        return Ok(response);
    }
    
}
