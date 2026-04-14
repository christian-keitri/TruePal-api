using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;
using TruePal.Api.Models;

namespace TruePal.Api.Controllers;

public class HomeController : BaseController
{
    public HomeController(ILogger<HomeController> logger) : base(logger)
    {
    }

    public IActionResult Index()
    {
        // Sample trending posts data (matches sample-posts.js)
        var trendingPosts = new List<TrendingPostViewModel>
        {
            new TrendingPostViewModel
            {
                Id = 1,
                Content = "Conquered the highest peak in the Philippines! 2,954 meters of pain, but the view from the top? Worth every step.",
                Location = "Mt. Apo, Davao",
                ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400&h=200&fit=crop",
                LikesCount = 189,
                CommentsCount = 34,
                ViewsCount = 1500,
                Username = "Joy Dela Cruz",
                TimeAgo = "1 day ago"
            },
            new TrendingPostViewModel
            {
                Id = 2,
                Content = "Station 1 at sunrise. No filter needed when the sand is THIS white and the water is THIS clear. Paradise is real.",
                Location = "White Beach, Boracay",
                ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=400&h=200&fit=crop",
                LikesCount = 156,
                CommentsCount = 31,
                ViewsCount = 1200,
                Username = "Carlos Mendoza",
                TimeAgo = "1 hr ago"
            },
            new TrendingPostViewModel
            {
                Id = 3,
                Content = "Rolling green hills, stone houses, and zero traffic. Batanes feels like a different country. The Honesty Coffee Shop is real!",
                Location = "Batan Island, Batanes",
                ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400&h=200&fit=crop",
                LikesCount = 142,
                CommentsCount = 28,
                ViewsCount = 1100,
                Username = "Marco Lim",
                TimeAgo = "1 day ago"
            },
            new TrendingPostViewModel
            {
                Id = 4,
                Content = "Kayaking through the Big Lagoon. Turquoise water, towering limestone cliffs. El Nido is the crown jewel of Palawan.",
                Location = "Big Lagoon, El Nido",
                ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=400&h=200&fit=crop",
                LikesCount = 134,
                CommentsCount = 27,
                ViewsCount = 890,
                Username = "Mia Santos",
                TimeAgo = "4 hrs ago"
            },
            new TrendingPostViewModel
            {
                Id = 5,
                Content = "Standing at the 8th Wonder of the World. 2,000-year-old rice terraces carved by our ancestors. Pure Filipino pride.",
                Location = "Rice Terraces, Banaue",
                ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400&h=200&fit=crop",
                LikesCount = 120,
                CommentsCount = 23,
                ViewsCount = 950,
                Username = "Lito Aguilar",
                TimeAgo = "4 hrs ago"
            },
            new TrendingPostViewModel
            {
                Id = 6,
                Content = "Caught my first barrel at Cloud 9! The surfing capital lives up to the hype. Stoked beyond words.",
                Location = "Cloud 9, Siargao",
                ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=400&h=200&fit=crop",
                LikesCount = 103,
                CommentsCount = 19,
                ViewsCount = 720,
                Username = "Sofia Diaz",
                TimeAgo = "2 hrs ago"
            },
            new TrendingPostViewModel
            {
                Id = 7,
                Content = "Dove into the clearest lake in Asia. The underwater visibility is unbelievable. Coron is a diver's paradise.",
                Location = "Kayangan Lake, Coron",
                ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=400&h=200&fit=crop",
                LikesCount = 97,
                CommentsCount = 20,
                ViewsCount = 680,
                Username = "Dan Villanueva",
                TimeAgo = "5 hrs ago"
            },
            new TrendingPostViewModel
            {
                Id = 8,
                Content = "Had the BEST lechon at Zubuchon. Crispy skin, juicy meat. Sorry Manila, Cebu wins this round.",
                Location = "Cebu City, Cebu",
                ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&h=200&fit=crop",
                LikesCount = 91,
                CommentsCount = 22,
                ViewsCount = 650,
                Username = "Ana Garcia",
                TimeAgo = "1 hr ago"
            }
        };

        var model = new HomeViewModel
        {
            IsAuthenticated = Request.Cookies.ContainsKey("AuthToken"),
            TrendingPosts = trendingPosts
        };
        return View(model);
    }

    public IActionResult Error()
    {
        return View();
    }
}

#region ViewModels

public class HomeViewModel
{
    public bool IsAuthenticated { get; set; }
    public List<TrendingPostViewModel> TrendingPosts { get; set; } = new();
}

public class TrendingPostViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int ViewsCount { get; set; }
    public string Username { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}

#endregion
