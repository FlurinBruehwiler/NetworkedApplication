namespace Shared;

public struct Result
{
    public bool IsSuccess;
    public string? ErrorMessage;

    public static Result Success()
    {
        return new Result
        {
            IsSuccess = true
        };
    }

    public static Result Error(string? message)
    {
        return new Result
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}