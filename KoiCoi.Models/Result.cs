﻿
namespace KoiCoi.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public bool IsError { get { return !IsSuccess; } }
    public bool IsWarning { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }

    public static Result<T> Success(T data, string message = "Success.")
    {
        return new Result<T>()
        {
            Data = data,
            Message = message,
            IsSuccess = true,
        };
    }

    public static Result<T> Error(string message = "Internal server error.")
    {
        return new Result<T>()
        {
            Message = message,
            IsSuccess = false,
        };
    }
    public static Result<T> Warning(string message)
    {
        return new Result<T>()
        {
            Message = message,
            IsSuccess = false,
            IsWarning = true
        };
    }


    public static Result<T> Error(Exception ex)
    {
        return new Result<T>()
        {
            Message = ex.ToString(),
            IsSuccess = false,
        };
    }
}
