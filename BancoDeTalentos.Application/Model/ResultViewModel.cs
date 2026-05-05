using System.Net;

namespace BancoDeTalentos.Application.Model;

public class ResultViewModel
{
    public ResultViewModel(string message = "", bool isSuccess = true, HttpStatusCode? errorCode = null)
    {
        Message = message;
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
    }

    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public HttpStatusCode? ErrorCode { get; set; }

    public static ResultViewModel Sucess() => new ResultViewModel();
    public static ResultViewModel Error(string message, HttpStatusCode errorCode)
        => new ResultViewModel(message, false, errorCode);
}

public class ResultViewModel<T> : ResultViewModel
{
    public ResultViewModel(T? data, string message = "", bool isSuccess = true, HttpStatusCode? errorCode = null)
        : base(message, isSuccess, errorCode) => Data = data;

    public T? Data { get; set; }

    public static ResultViewModel<T> Success(T data) => new ResultViewModel<T>(data);
    public static ResultViewModel<T> Error(string message, HttpStatusCode errorCode, T? data)
        => new ResultViewModel<T>(data, message, false, errorCode);
}