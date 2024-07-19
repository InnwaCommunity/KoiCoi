

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using KoiCoi.Backend;
global using KoiCoi.Database.AppDbContextModels;


//Repository
global using KoiCoi.Modules.Repository.User;
global using KoiCoi.Modules.Repository.ChangePassword;


///Models
global using KoiCoi.Models.Login_Models;
global using KoiCoi.Models.User_Dto;
global using KoiCoi.Models.ForgetPassword;
global using KoiCoi.Models;


//Operational
global using KoiCoi.Operational.Encrypt;
global using KoiCoi.Operational;
global using KoiCoi.Operational.Extensions;