

using KoiCoi.Models.User_Dto;
using KoiCoi.Models.Via;

namespace KoiCoi.Modules.Repository.User;

public class BL_User
{
    private readonly DA_User _daUser;

    public BL_User(DA_User daUser)
    {
        _daUser = daUser;
    }
    public async Task<dynamic> GetStatusType()
    {
        return await _daUser.GetStatusType();
    }

    public async Task<ResponseData> CreateAccount(RequestUserDto requestUser)
    {
        try
        {
            ViaUser viaUser = new ViaUser();
            viaUser.Email = requestUser.Email ?? "";
            viaUser.Phone = requestUser.Phone ?? "";
            viaUser.DeviceId = requestUser.DeviceId ?? "";
            // Generate a new 12-character password with at least 1 non-alphanumeric character.
            RandomPassword passwordGenerator = new RandomPassword();
            if (requestUser.Name == null && requestUser.Password == null)
            {
                string password = passwordGenerator.CreatePassword(11, 3);
                string temppassword = password;
                string name = Guid.NewGuid().ToString();
                string salt = SaltedHash.GenerateSalt();
                string userIdval =Encryption.EncryptID(name,salt) + passwordGenerator.CreatePassword(name.Length, name.Length/3);
                viaUser.UserIdval = userIdval;
                viaUser.Name = name;
                password = SaltedHash.ComputeHash(salt, password.ToString());

                viaUser.Name = name;
                viaUser.Password = password;
                viaUser.PasswordHash = salt;
                return await _daUser.CreateAccount(viaUser,temppassword);
            }
            else
            {
                string salt = SaltedHash.GenerateSalt();
                string tempPas = requestUser.Password!;
                viaUser.Password = SaltedHash.ComputeHash(salt, requestUser.Password!.ToString());
                viaUser.PasswordHash = salt;
                viaUser.Name = requestUser.Name ?? "";
                string userIdval = Encryption.EncryptID(requestUser.Name!, salt) + passwordGenerator.CreatePassword(requestUser.Name!.Length, requestUser.Name!.Length / 3);
                viaUser.UserIdval = userIdval;
                return await _daUser.CreateAccount(viaUser, tempPas);
            }
        }
        catch (Exception ex) {
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }

    public async Task<ResponseData> UpdateUserInfo(RequestUserDto requestUserDto,int LoginUserId)
    {
        return await _daUser.UpdateUserInfo(requestUserDto, LoginUserId);
    }

    public async Task<ResponseData> FindUserByName(string name, int LoginUserId)
    {
        return await _daUser.FindUserByName(name, LoginUserId);
    }

    public async Task<ResponseData> DeleteLoginUser(int LoginUserId)
    {
        return await _daUser.DeleteLoginUser(LoginUserId);
    }
}
