namespace FieldWork.Application.Authentication; 
public interface IAuthService 
{ 
    Task<LoginResponse?> LoginAsync(LoginRequest request); 
}