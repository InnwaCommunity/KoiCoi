﻿
using KoiCoi.Models.Login_Models;
using KoiCoi.Models.User_Dto.Payload;
using KoiCoi.Models.Via;
using Microsoft.AspNetCore.Http;

namespace KoiCoi.Modules.Repository.UserFeature;

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

    public async Task<Result<ResponseUserDto>> CreateAccount(RequestUserDto requestUser)
    {
        try
        {
            return await _daUser.CreateAccount(requestUser);
            /*
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
                string name = "Guest";
                string salt = SaltedHash.GenerateSalt();
                string userIdval =Encryption.EncryptID(name,salt);// + passwordGenerator.CreatePassword(name.Length, name.Length/3)
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
            }*/
        }
        catch (Exception ex) {
            return Result<ResponseUserDto>.Error(ex);
        }
    }

    public async Task<Result<List<UserLoginAccounts>>> AccountsWithDeviceId(string deviceId)
    {
        return await _daUser.AccountsWithDeviceId(deviceId);
    }

    public async Task<Result<string>> UpdateUserInfo(RequestUserDto requestUserDto,int LoginUserId)
    {
        return await _daUser.UpdateUserInfo(requestUserDto, LoginUserId);
    }

    public async Task<Result<List<UserInfoResponse>>> FindUserByName(string name, int LoginUserId)
    {
        return await _daUser.FindUserByName(name, LoginUserId);
    }

    public async Task<Result<string>> DeleteLoginUser(int LoginUserId)
    {
        return await _daUser.DeleteLoginUser(LoginUserId);
    }

    public async Task<Result<string>> UploadUserProfile(IFormFile file,int LoginUserId)
    {
        return await _daUser.UploadUserProfile(file, LoginUserId);
    }

    public async Task<Result<List<UserTypeResponse>>> GetUserTypes(int LoginUserId)
    {
        return await _daUser.GetUserTypes(LoginUserId);
    }
    public async Task<Result<ResponseUserDto>> Signin(LoginPayload paylod)
    {
        return await _daUser.Signin(paylod);
    }
    public async Task<Result<LoginUserInfo>> GetLoginUserInfo(int LoginEmpID)
    {
        return await _daUser.GetLoginUserInfo(LoginEmpID);
    }

    public async Task<Result<string>> ChangeLoginPassword(ChangePasswordPayload paylod,int LoginUserId)
    {
        return await _daUser.ChangeLoginPassword(paylod, LoginUserId);
    }
    public async Task<Result<string>> RemoveLoginAccount(RemoveLoginAccountPayload payload)
    {
        return await _daUser.RemoveLoginAccount(payload);
    }
    public async Task<Result<string>> GetUserProfile(GetUserData payload,int LoginEmpID)
    {
        return await _daUser.GetUserProfile(payload, LoginEmpID);
    }
    public async Task<Result<UserLoginAccounts>> CheckSocialAccount(CheckSocialAccountPayload payload)
    {
        return await _daUser.CheckSocialAccount(payload);
    }
    public async Task<Result<ResponseUserDto>> CreateSocialAccount(SocialSignInPayload payload)
    {
        return await _daUser.CreateSocialAccount(payload);
    }
}
