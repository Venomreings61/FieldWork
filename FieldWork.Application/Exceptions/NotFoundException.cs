// FieldWork.Application/Exceptions/NotFoundException.cs
namespace FieldWork.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}