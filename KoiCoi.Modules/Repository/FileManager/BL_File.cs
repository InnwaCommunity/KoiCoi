namespace KoiCoi.Modules.Repository.FileManager;

public class BL_File
{
    private readonly DA_File _daFile;

    public BL_File(DA_File daFile)
    {
        _daFile = daFile;
    }
    public async Task<Result<string>> GetFiles(GetFilePayload paylod,int LoginUserId)
    {
        return await _daFile.GetFiles(paylod, LoginUserId);
    }
}
