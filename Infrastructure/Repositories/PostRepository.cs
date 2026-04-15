using Microsoft.EntityFrameworkCore;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Data;
using TruePal.Api.Models;

namespace TruePal.Api.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetTrendingPostsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.User)
            .OrderByDescending(p => p.LikesCount * 2 + p.CommentsCount * 3 + p.ViewsCount * 0.5)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Post?> GetByIdWithUserAsync(int id)
    {
        return await _dbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
