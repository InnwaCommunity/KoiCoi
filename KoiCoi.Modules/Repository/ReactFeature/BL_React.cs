
using KoiCoi.Models.Login_Models;
using KoiCoi.Modules.Repository.PostFeature;

namespace KoiCoi.Modules.Repository.ReactFeature;
public class BL_React
{
    private readonly DA_React _daReact;

    public BL_React(DA_React daReact)
    {
        _daReact = daReact;
    }

    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType(int LoginUserId)
    {
        return await _daReact.GetAllReactType(LoginUserId);
    }

    public async Task<Result<string>> ReactPost(ReactPostPayload payload,int LoginUserID)
    {
        return await _daReact.ReactPost(payload, LoginUserID);
    }
}
