// FieldWork.Application/Exceptions/NotFoundException.cs

// NotFoundException.cs
namespace FieldWork.Application.Exceptions;

public class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string code, string message) : base(message)
    {
        Code = code;
    }
}