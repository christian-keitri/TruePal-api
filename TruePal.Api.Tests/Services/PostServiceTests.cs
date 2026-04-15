using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TruePal.Api.Application.Services;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Infrastructure;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests.Services;

public class PostServiceTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly UnitOfWork _unitOfWork;
    private readonly PostService _postService;
    private readonly User _testUser;

    public PostServiceTests()
    {
        _testDb = new TestDbContext();
        _unitOfWork = new UnitOfWork(_testDb.Context);

        var logger = NullLogger<PostService>.Instance;
        _postService = new PostService(_unitOfWork, logger);

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

    // --- Create Post Tests ---

    [Fact]
    public async Task CreatePostAsync_ValidInput_ReturnsSuccess()
    {
        var result = await _postService.CreatePostAsync(
            _testUser.Id,
            "This is a test post",
            "Manila",
            "https://example.com/image.jpg"
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Content.Should().Be("This is a test post");
        result.Data.Location.Should().Be("Manila");
        result.Data.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.Data.Id.Should().BeGreaterThan(0);
        result.Data.User.Should().NotBeNull();
        result.Data.User.Username.Should().Be(_testUser.Username);
    }

    [Fact]
    public async Task CreatePostAsync_EmptyContent_ReturnsFailure()
    {
        var result = await _postService.CreatePostAsync(_testUser.Id, "", null, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Post content is required");
        result.ErrorCode.Should().Be(ErrorCodes.Validation);
    }

    [Fact]
    public async Task CreatePostAsync_ContentTooLong_ReturnsFailure()
    {
        var longContent = new string('x', 501);
        var result = await _postService.CreatePostAsync(_testUser.Id, longContent, null, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Post content must be 500 characters or less");
    }

    [Fact]
    public async Task CreatePostAsync_NonExistentUser_ReturnsFailure()
    {
        var result = await _postService.CreatePostAsync(999, "Test post", null, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task CreatePostAsync_WithoutLocationAndImage_ReturnsSuccess()
    {
        var result = await _postService.CreatePostAsync(_testUser.Id, "Simple post", null, null);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Location.Should().BeNull();
        result.Data.ImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreatePostAsync_LocationTooLong_ReturnsFailure()
    {
        var longLocation = new string('x', 201);
        var result = await _postService.CreatePostAsync(_testUser.Id, "Test post", longLocation, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Location must be 200 characters or less");
    }

    [Fact]
    public async Task CreatePostAsync_ImageUrlTooLong_ReturnsFailure()
    {
        var longUrl = new string('x', 501);
        var result = await _postService.CreatePostAsync(_testUser.Id, "Test post", null, longUrl);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Image URL must be 500 characters or less");
    }

    // --- Get Post Tests ---

    [Fact]
    public async Task GetPostByIdAsync_ExistingPost_ReturnsPost()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Test post",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.GetPostByIdAsync(post.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(post.Id);
        result.Data.Content.Should().Be("Test post");
    }

    [Fact]
    public async Task GetPostByIdAsync_NonExistentPost_ReturnsFailure()
    {
        var result = await _postService.GetPostByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    // --- Get User Posts Tests ---

    [Fact]
    public async Task GetUserPostsAsync_ExistingPosts_ReturnsPosts()
    {
        await _testDb.Context.Posts.AddRangeAsync(
            new Post { UserId = _testUser.Id, Content = "Post 1", CreatedAt = DateTime.UtcNow },
            new Post { UserId = _testUser.Id, Content = "Post 2", CreatedAt = DateTime.UtcNow }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.GetUserPostsAsync(_testUser.Id);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserPostsAsync_NonExistentUser_ReturnsEmpty()
    {
        var result = await _postService.GetUserPostsAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().BeEmpty();
    }

    // --- Get Recent Posts Tests ---

    [Fact]
    public async Task GetRecentPostsAsync_ValidCount_ReturnsPosts()
    {
        await _testDb.Context.Posts.AddRangeAsync(
            new Post { UserId = _testUser.Id, Content = "Post 1", CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new Post { UserId = _testUser.Id, Content = "Post 2", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new Post { UserId = _testUser.Id, Content = "Post 3", CreatedAt = DateTime.UtcNow.AddHours(-1) }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.GetRecentPostsAsync(2);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
        result.Data!.First().Content.Should().Be("Post 3");
    }

    [Fact]
    public async Task GetRecentPostsAsync_InvalidCount_ReturnsFailure()
    {
        var result = await _postService.GetRecentPostsAsync(0);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Count must be between 1 and 100");
        result.ErrorCode.Should().Be(ErrorCodes.Validation);
    }

    [Fact]
    public async Task GetRecentPostsAsync_CountTooLarge_ReturnsFailure()
    {
        var result = await _postService.GetRecentPostsAsync(101);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Count must be between 1 and 100");
    }

    // --- Get Trending Posts Tests ---

    [Fact]
    public async Task GetTrendingPostsAsync_ValidCount_ReturnsPosts()
    {
        await _testDb.Context.Posts.AddRangeAsync(
            new Post
            {
                UserId = _testUser.Id,
                Content = "Low engagement",
                LikesCount = 1,
                CommentsCount = 0,
                CreatedAt = DateTime.UtcNow
            },
            new Post
            {
                UserId = _testUser.Id,
                Content = "High engagement",
                LikesCount = 100,
                CommentsCount = 50,
                CreatedAt = DateTime.UtcNow
            }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.GetTrendingPostsAsync(2);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
        result.Data!.First().Content.Should().Be("High engagement");
    }

    // --- Update Post Tests ---

    [Fact]
    public async Task UpdatePostAsync_ValidInput_ReturnsSuccess()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Original content",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.UpdatePostAsync(
            post.Id,
            _testUser.Id,
            "Updated content",
            "Cebu",
            "https://example.com/new.jpg"
        );

        result.IsSuccess.Should().BeTrue();
        result.Data!.Content.Should().Be("Updated content");
        result.Data.Location.Should().Be("Cebu");
        result.Data.ImageUrl.Should().Be("https://example.com/new.jpg");
        result.Data.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdatePostAsync_NonExistentPost_ReturnsFailure()
    {
        var result = await _postService.UpdatePostAsync(999, _testUser.Id, "Content", null, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task UpdatePostAsync_WrongUser_ReturnsFailure()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Original content",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.UpdatePostAsync(post.Id, 999, "Updated", null, null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You are not authorized to update this post");
        result.ErrorCode.Should().Be(ErrorCodes.Forbidden);
    }

    [Fact]
    public async Task UpdatePostAsync_EmptyContent_ReturnsFailure()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Original",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.UpdatePostAsync(post.Id, _testUser.Id, "", null, null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Post content is required");
    }

    // --- Delete Post Tests ---

    [Fact]
    public async Task DeletePostAsync_ValidInput_ReturnsSuccess()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "To be deleted",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.DeletePostAsync(post.Id, _testUser.Id);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _unitOfWork.Posts.GetByIdAsync(post.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeletePostAsync_NonExistentPost_ReturnsFailure()
    {
        var result = await _postService.DeletePostAsync(999, _testUser.Id);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public async Task DeletePostAsync_WrongUser_ReturnsFailure()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "To be deleted",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.DeletePostAsync(post.Id, 999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You are not authorized to delete this post");
        result.ErrorCode.Should().Be(ErrorCodes.Forbidden);
    }

    // --- Increment Views Tests ---

    [Fact]
    public async Task IncrementViewsAsync_ExistingPost_IncrementsCount()
    {
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Test",
            ViewsCount = 5,
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var result = await _postService.IncrementViewsAsync(post.Id);

        result.IsSuccess.Should().BeTrue();

        var updated = await _unitOfWork.Posts.GetByIdAsync(post.Id);
        updated!.ViewsCount.Should().Be(6);
    }

    [Fact]
    public async Task IncrementViewsAsync_NonExistentPost_ReturnsFailure()
    {
        var result = await _postService.IncrementViewsAsync(999);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _testDb.Dispose();
    }
}
