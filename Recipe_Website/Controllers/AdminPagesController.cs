using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Data;

namespace Recipe_Website.Controllers
{
    // MVC controller for Razor admin pages.
    [Route("Admin")]
    public class AdminPagesController : Controller
    {
        private readonly AppDbContext _db;

        public AdminPagesController(AppDbContext db)
        {
            _db = db;
        }

        // GET /Admin
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _db.Users.CountAsync();
            var totalRecipes = await _db.Recipes.CountAsync();
            var pendingRecipes = await _db.Recipes.CountAsync(r => !r.IsApproved && !r.IsDisabled);
            var totalReports = await _db.Reports.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRecipes = totalRecipes;
            ViewBag.PendingRecipes = pendingRecipes;
            ViewBag.TotalReports = totalReports;

            // Explicit path to your view folder
            return View("~/Views/Admin/Index.cshtml");
        }

        // GET /Admin/PendingRecipes
        [HttpGet("PendingRecipes")]
        public async Task<IActionResult> PendingRecipes()
        {
            var pending = await _db.Recipes
                .Include(r => r.CreatedByUser)
                .Where(r => !r.IsApproved && !r.IsDisabled)
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            return View("~/Views/Admin/PendingRecipes.cshtml", pending);
        }

        // GET /Admin/Reports
        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            var reports = await _db.Reports
                .Include(r => r.Recipe)
                .Include(r => r.Comment)
                .Include(r => r.ReporterUser)
                .Where(r => !r.IsResolved)
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            return View("~/Views/Admin/Reports.cshtml", reports);
        }

        // GET /Admin/Tags
        [HttpGet("Tags")]
        public async Task<IActionResult> Tags()
        {
            var tags = await _db.Tags
                .Include(t => t.RecipeTags)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View("~/Views/Admin/Tags.cshtml", tags);
        }

        // GET /Admin/Users
        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _db.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View("~/Views/Admin/Users.cshtml", users);
        }

        // GET /Admin/Settings
        [HttpGet("Settings")]
        public async Task<IActionResult> Settings()
        {
            var settings = await _db.Settings
                .OrderBy(s => s.Key)
                .ToListAsync();

            return View("~/Views/Admin/Settings.cshtml", settings);
        }
    }
}
