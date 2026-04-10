namespace TruePal.Api.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    Task<int> CompleteAsync();
}
