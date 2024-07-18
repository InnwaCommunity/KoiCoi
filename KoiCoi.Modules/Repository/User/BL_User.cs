

using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

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

    public async Task<ResponseData> CreateAccount(RequestUserDto requestUserDto)
    {
        try
        {
            requestUserDto.Email = requestUserDto.Email ?? "";
            requestUserDto.Phone = requestUserDto.Phone ?? "";
            requestUserDto.DeviceId = requestUserDto.DeviceId ?? "";
            // Generate a new 12-character password with at least 1 non-alphanumeric character.
            RandomPassword passwordGenerator = new RandomPassword();
            if (requestUserDto.Name == null && requestUserDto.Password == null)
            {
                //string password = CreatePassword(11);
                string password = passwordGenerator.CreatePassword(11, 3);
                string temppassword = password;
                string name = Guid.NewGuid().ToString();
                string salt = SaltedHash.GenerateSalt();
                string userIdval =Encryption.EncryptID(name,salt) + passwordGenerator.CreatePassword(name.Length, name.Length/3);
                requestUserDto.UserIdval = userIdval;
                password = SaltedHash.ComputeHash(salt, password.ToString());

                requestUserDto.Name = name;
                requestUserDto.Password = password;
                requestUserDto.PasswordHash = salt;
                requestUserDto.DateCreated = DateTime.UtcNow;
                return await _daUser.CreateAccount(requestUserDto,temppassword);
            }
            else
            {
                string salt = SaltedHash.GenerateSalt();
                string tempPas = requestUserDto.Password!;
                requestUserDto.Password = SaltedHash.ComputeHash(salt, requestUserDto.Password!.ToString());
                requestUserDto.PasswordHash = salt;
                string userIdval = Encryption.EncryptID(requestUserDto.Name!, salt) + passwordGenerator.CreatePassword(requestUserDto.Name!.Length, requestUserDto.Name!.Length / 3);
                requestUserDto.UserIdval = userIdval;
                requestUserDto.DateCreated = DateTime.UtcNow;
                return await _daUser.CreateAccount(requestUserDto, tempPas);
            }
        }
        catch (Exception ex) {
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }
}
