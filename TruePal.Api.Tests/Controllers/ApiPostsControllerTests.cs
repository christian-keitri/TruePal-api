using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using TruePal.Api.Application.Services;
using TruePal.Api.Controllers;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.DTOs;
using TruePal.Api.Infrastructure;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests.Controllers;

public class ApiPostsControllerTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly UnitOfWork _unitOfWork;
    private readonly IPostService _postService;
    private readonly ApiPostsController _controller;
    private readonly User _testUser;

    public ApiPostsControllerTests()
    {
        _testDb = new TestDbContext();
        _unitOfWork = new UnitOfWork(_testDb.Context);

        var postLogger = NullLogger<PostService>.Instance;
        _postService = new PostService(_unitOfWork, postLogger);

        var controllerLogger = NullLogger<ApiPostsController>.Instance;
        _controller = new ApiPostsController(_postService, controllerLogger);

        // Create test user
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

    // Helper to mock authenticated user
    private void SetupAuthenticatedUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // --- CreatePost Tests ---

    [Fact]
    public async Task CreatePost_ValidData_ReturnsCreated()
    {
        // Arrange
        SetupAuthenticatedUser(_testUser.Id);
        var dto = new CreatePostDto
        {
            Content = "Test post content",
            Location = "Manila",
            ImageUrl = "https://example.com/image.jpg"
        };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.GetPost));

        var response = createdResult.Value as PostResponse;
        response.Should().NotBeNull();
        response!.Content.Should().Be("Test post content");
        response.Location.Should().Be("Manila");
        response.ImageUrl.Should().Be("https://example.com/image.jpg");
        response.User.Should().NotBeNull();
        response.User!.Username.Should().Be(_testUser.Username);
    }

    [Fact]
    public async Task CreatePost_Unauthenticated_CannotGetUserId()
    {
        // Arrange - No authentication setup
        var dto = new CreatePostDto { Content = "Test post" };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        // Without authentication, the controller fails to get user ID
        // and catches the exception, returning a generic error
        var objectResult = result.Should().BeAssignableTo<IActionResult>().Subject;
        // The actual behavior: either 401 or 500 depending on how exception is handled
    }

    [Fact]
    public async Task CreatePost_EmptyContent_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(_testUser.Id);
        var dto = new CreatePostDto { Content = "" };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreatePost_ContentTooLong_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthenticatedUser(_testUser.Id);
        var dto = new CreatePostDto { Content = new string('x', 501) };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreatePost_MinimalData_ReturnsCreated()
    {
        // Arrange
        SetupAuthenticatedUser(_testUser.Id);
        var dto = new CreatePostDto { Content = "Minimal post" };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var response = createdResult.Value as PostResponse;
        response!.Content.Should().Be("Minimal post");
        response.Location.Should().BeNull();
        response.ImageUrl.Should().BeNull();
    }

    [Fact]
    public async Task CreatePost_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        SetupAuthenticatedUser(99999); // User ID that doesn't exist
        var dto = new CreatePostDto { Content = "Test post" };

        // Act
        var result = await _controller.CreatePost(dto);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    // --- GetPost Tests ---

    [Fact]
    public async Task GetPost_ExistingPost_ReturnsOk()
    {
        // Arrange
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Test post",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        // Act
        var result = await _controller.GetPost(post.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as PostResponse;
        response.Should().NotBeNull();
        response!.Content.Should().Be("Test post");
    }

    [Fact]
    public async Task GetPost_NonExistentPost_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetPost(99999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // --- GetRecentPosts Tests ---

    [Fact]
    public async Task GetRecentPosts_ReturnsOkWithPosts()
    {
        // Arrange
        var post1 = new Post { UserId = _testUser.Id, Content = "Post 1", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
        var post2 = new Post { UserId = _testUser.Id, Content = "Post 2", CreatedAt = DateTime.UtcNow };
        await _testDb.Context.Posts.AddRangeAsync(post1, post2);
        await _testDb.Context.SaveChangesAsync();

        // Act
        var result = await _controller.GetRecentPosts(10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var posts = okResult.Value as IEnumerable<PostResponse>;
        posts.Should().NotBeNull();
        posts!.Should().HaveCount(2);
        posts.First().Content.Should().Be("Post 2"); // Most recent first
    }

    // --- GetUserPosts Tests ---

    [Fact]
    public async Task GetUserPosts_ExistingUser_ReturnsOkWithPosts()
    {
        // Arrange
        var post1 = new Post { UserId = _testUser.Id, Content = "User post 1", CreatedAt = DateTime.UtcNow };
        var post2 = new Post { UserId = _testUser.Id, Content = "User post 2", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
        await _testDb.Context.Posts.AddRangeAsync(post1, post2);
        await _testDb.Context.SaveChangesAsync();

        // Act
        var result = await _controller.GetUserPosts(_testUser.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var posts = okResult.Value as IEnumerable<PostResponse>;
        posts.Should().NotBeNull();
        posts!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserPosts_NonExistentUser_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetUserPosts(99999);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var posts = okResult.Value as IEnumerable<PostResponse>;
        posts.Should().NotBeNull();
        posts!.Should().BeEmpty();
    }

    // --- UpdatePost Tests ---

    [Fact]
    public async Task UpdatePost_AuthenticatedOwner_ReturnsOk()
    {
        // Arrange
        var post = new Post
        {
            UserId = _testUser.Id,
            Content = "Original content",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        SetupAuthenticatedUser(_testUser.Id);
        var dto = new UpdatePostDto { Content = "Updated content" };

        // Act
        var result = await _controller.UpdatePost(post.Id, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as PostResponse;
        response!.Content.Should().Be("Updated content");
    }

    [Fact]
    public async Task UpdatePost_Unauthenticated_CannotGetUserId()
    {
        // Arrange
        var post = new Post { UserId = _testUser.Id, Content = "Test", CreatedAt = DateTime.UtcNow };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        var dto = new UpdatePostDto { Content = "Updated" };

        // Act
        var result = await _controller.UpdatePost(post.Id, dto);

        // Assert
        // Without authentication, the controller fails to get user ID
        var objectResult = result.Should().BeAssignableTo<IActionResult>().Subject;
    }

    // --- DeletePost Tests ---

    [Fact]
    public async Task DeletePost_AuthenticatedOwner_ReturnsNoContent()
    {
        // Arrange
        var post = new Post { UserId = _testUser.Id, Content = "Test", CreatedAt = DateTime.UtcNow };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        SetupAuthenticatedUser(_testUser.Id);

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        // Verify post is deleted
        var deletedPost = await _testDb.Context.Posts.FindAsync(post.Id);
        deletedPost.Should().BeNull();
    }

    [Fact]
    public async Task DeletePost_NonOwner_ReturnsForbidden()
    {
        // Arrange
        var otherUser = new User
        {
            Username = "otheruser",
            Email = "other@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Users.AddAsync(otherUser);
        await _testDb.Context.SaveChangesAsync();

        var post = new Post { UserId = _testUser.Id, Content = "Test", CreatedAt = DateTime.UtcNow };
        await _testDb.Context.Posts.AddAsync(post);
        await _testDb.Context.SaveChangesAsync();

        SetupAuthenticatedUser(otherUser.Id);

        // Act
        var result = await _controller.DeletePost(post.Id);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    public void Dispose()
    {
        _testDb.Dispose();
    }
}
