// FieldWork.Application/Exceptions/ConflictException.cs
namespace FieldWork.Application.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}