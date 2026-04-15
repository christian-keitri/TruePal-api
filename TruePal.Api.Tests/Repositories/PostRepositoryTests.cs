using FluentAssertions;
using TruePal.Api.Infrastructure.Repositories;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests.Repositories;

public class PostRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly PostRepository _repository;
    private readonly User _testUser;

    public PostRepositoryTests()
    {
        _testDb = new TestDbContext();
        _repository = new PostRepository(_testDb.Context);

        _testUser = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _testDb.Context.Users.Add(_testUser);
        _testDb.Context.SaveChanges();
    }

    [Fact]
    public async Task GetByUserIdAsync_ExistingPosts_ReturnsPosts()
    {
        var post1 = new Post
        {
            UserId = _testUser.Id,
            Content = "First post",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var post2 = new Post
        {
            UserId = _testUser.Id,
            Content = "Second post",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        await _testDb.Context.Posts.AddRangeAsync(post1, post2);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(_testUser.Id);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.UserId == _testUser.Id);
        result.First().Content.Should().Be("Second post");
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExistentUser_ReturnsEmpty()
    {
        var result = await _repository.GetByUserIdAsync(999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentPostsAsync_MultiplePosts_ReturnsLimitedOrderedPosts()
    {
        var posts = new[]
        {
            new Post { UserId = _testUser.Id, Content = "Post 1", CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new Post { UserId = _testUser.Id, Content = "Post 2", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new Post { UserId = _testUser.Id, Content = "Post 3", CreatedAt = DateTime.UtcNow.AddHours(-1) }
        };
        await _testDb.Context.Posts.AddRangeAsync(posts);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetRecentPostsAsync(2);

        result.Should().HaveCount(2);
        result.First().Content.Should().Be("Post 3");
        result.Last().Content.Should().Be("Post 2");
    }

    [Fact]
    public async Task GetTrendingPostsAsync_MultiplePosts_ReturnsByEngagement()
    {
        var lowEngagement = new Post
        {
            UserId = _testUser.Id,
            Content = "Low engagement",
            LikesCount = 1,
            CommentsCount = 0,
            ViewsCount = 10,
            CreatedAt = DateTime.UtcNow
        };
        var highEngagement = new Post
        {
            UserId = _testUser.Id,
            Content = "High engagement",
            LikesCount = 50,
            CommentsCount = 20,
            ViewsCount = 100,
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddRangeAsync(lowEngagement, highEngagement);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetTrendingPostsAsync(2);

        result.Should().HaveCount(2);
        result.First().Content.Should().Be("High engagement");
    }

    [Fact]
    public async Task GetByIdWithUserAsync_ExistingPost_ReturnsPostWithUser()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Test post",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetByIdWithUserAsync(post.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
        result.User.Should().NotBeNull();
        result.User.Username.Should().Be(_testUser.Username);
    }

    [Fact]
    public async Task GetByIdWithUserAsync_NonExistentPost_ReturnsNull()
    {
        var result = await _repository.GetByIdWithUserAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_NewPost_PersistsToDatabase()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "New post",
            Location = "Manila",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(post.Id);
        saved.Should().NotBeNull();
        saved!.Content.Should().Be("New post");
        saved.Location.Should().Be("Manila");
        saved.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Update_ExistingPost_UpdatesFields()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Original content",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        post.Content = "Updated content";
        post.Location = "Cebu";
        post.UpdatedAt = DateTime.UtcNow;
        _repository.Update(post);
        await _testDb.Context.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(post.Id);
        updated.Should().NotBeNull();
        updated!.Content.Should().Be("Updated content");
        updated.Location.Should().Be("Cebu");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ExistingPost_RemovesFromDatabase()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "To be deleted",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        _repository.Delete(post);
        await _testDb.Context.SaveChangesAsync();

        var deleted = await _repository.GetByIdAsync(post.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultiplePosts_ReturnsAll()
    {
        var posts = new[]
        {
            new Post { UserId = _testUser.Id, Content = "Post 1", CreatedAt = DateTime.UtcNow },
            new Post { UserId = _testUser.Id, Content = "Post 2", CreatedAt = DateTime.UtcNow }
        };
        await _testDb.Context.Posts.AddRangeAsync(posts);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
