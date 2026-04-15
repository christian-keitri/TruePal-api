using TruePal.Api.Core.Interfaces;
using TruePal.Api.Data;
using TruePal.Api.Infrastructure.Repositories;

namespace TruePal.Api.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _userRepository;
    private IPostRepository? _postRepository;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IPostRepository Posts => _postRepository ??= new PostRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
