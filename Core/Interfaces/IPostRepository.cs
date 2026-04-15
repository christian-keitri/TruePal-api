using TruePal.Api.Models;

namespace TruePal.Api.Core.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count);
    Task<IEnumerable<Post>> GetTrendingPostsAsync(int count);
    Task<Post?> GetByIdWithUserAsync(int id);
}
