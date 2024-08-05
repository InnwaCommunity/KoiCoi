

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using KoiCoi.Backend;
global using KoiCoi.Database.AppDbContextModels;


//Repository
global using KoiCoi.Modules.Repository.UserFeature;
global using KoiCoi.Modules.Repository.ChangePassword;
global using KoiCoi.Modules.Repository.ChannelFeature;
global using KoiCoi.Modules.Repository.EventFreture;
global using KoiCoi.Modules.Repository.PostFeature;
global using KoiCoi.Modules.Repository.NotificationManager;

///Models
global using KoiCoi.Models.Login_Models;
global using KoiCoi.Models.User_Dto;
global using KoiCoi.Models.Otp_Dtos;
global using KoiCoi.Models.ChannelDtos;
global using KoiCoi.Models;
global using KoiCoi.Models.Via;
global using KoiCoi.Models.Currency;
global using KoiCoi.Models.EventDto;
global using KoiCoi.Models.EventDto.Response;
global using KoiCoi.Models.PostDtos.Payload;
global using KoiCoi.Models.PostDtos.Response;


//Operational
global using KoiCoi.Operational.Encrypt;
global using KoiCoi.Operational;
global using KoiCoi.Operational.Extensions;