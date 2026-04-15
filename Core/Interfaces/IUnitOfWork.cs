namespace TruePal.Api.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    Task<int> CompleteAsync();
}
