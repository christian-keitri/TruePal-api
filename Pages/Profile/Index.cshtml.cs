using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Pages.Base;
using TruePal.Api.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TruePal.Api.Pages.Profile;

public class ProfileIndexModel : BasePageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ProfileIndexModel(IUnitOfWork unitOfWork, ILogger<ProfileIndexModel> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToPageWithError("/Auth/Login", "Please login to access your profile");
        }

        // Get user ID from JWT token
        var token = Request.Cookies["AuthToken"];
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return RedirectToPageWithError("/Auth/Login", "Invalid authentication token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
        {
            return RedirectToPageWithError("/Auth/Login", "User not found");
        }

        Username = user.Username;
        Email = user.Email;
        CreatedAt = user.CreatedAt;

        return Page();
    }
}
