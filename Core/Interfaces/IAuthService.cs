using TruePal.Api.Models;

namespace TruePal.Api.Core.Interfaces;

public interface IAuthService
{
    Task<Result<User>> RegisterAsync(string username, string email, string password);
    Task<Result<string>> LoginAsync(string email, string password);
}
