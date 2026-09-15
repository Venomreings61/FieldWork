namespace FieldWork.Application.Exceptions;

public class FaceServiceUnavailableException : Exception
{
    public string Code { get; }

    public FaceServiceUnavailableException(string code, string message) : base(message)
    {
        Code = code;
    }
}