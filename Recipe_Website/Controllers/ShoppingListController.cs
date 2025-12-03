using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Api.Data;
using RecipeApp.Api.Models;
using System.Security.Claims;

namespace Recipe_Website.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingListController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ShoppingListController(AppDbContext db)
        {
            _db = db;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        [HttpGet]
        public async Task<IActionResult> GenerateShoppingList([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Fetch meal plans within the date range
            var mealPlans = await _db.MealPlans
                .Include(mp => mp.Items)
                    .ThenInclude(i => i.Recipe)
                        .ThenInclude(r => r.Ingredients)
                .Where(mp => mp.UserId == userId && 
                             mp.StartDate <= endDate && 
                             mp.EndDate >= startDate)
                .ToListAsync();

            // Flatten to get all relevant items within the specific date range requested
            // Note: A meal plan might span a week, but the user might only ask for 3 days.
            // We should filter the specific items by date.
            var relevantItems = mealPlans
                .SelectMany(mp => mp.Items)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .ToList();

            var aggregatedIngredients = new Dictionary<string, ShoppingListItem>();

            foreach (var item in relevantItems)
            {
                if (item.Recipe == null) continue;

                foreach (var ing in item.Recipe.Ingredients)
                {
                    // Create a composite key based on Name and Unit to group them
                    // Normalize name to lower case for better matching
                    var key = $"{ing.Name.Trim().ToLower()}_{ing.Unit?.Trim().ToLower() ?? ""}";

                    if (!aggregatedIngredients.ContainsKey(key))
                    {
                        aggregatedIngredients[key] = new ShoppingListItem
                        {
                            Name = ing.Name.Trim(),
                            Unit = ing.Unit?.Trim(),
                            Quantity = 0,
                            OriginalString = "" // Can be used for display if parsing fails
                        };
                    }

                    // Try to parse quantity
                    if (double.TryParse(ing.Quantity, out double qty))
                    {
                        aggregatedIngredients[key].Quantity += qty;
                    }
                    else
                    {
                        // If we can't parse, we might just want to list it or handle it differently.
                        // For now, let's just keep the original string if it's the first one, 
                        // or maybe append? Simple approach: if any non-numeric, just mark as such?
                        // Let's stick to simple summing for now and assume data is mostly clean or "1/2" style needs better parsing.
                        // For this MVP, we'll just try simple parse.
                    }
                }
            }

            var result = aggregatedIngredients.Values
                .OrderBy(i => i.Name)
                .ToList();

            return Ok(result);
        }

        public class ShoppingListItem
        {
            public string Name { get; set; } = string.Empty;
            public double Quantity { get; set; }
            public string? Unit { get; set; }
            public string? OriginalString { get; set; }
        }
    }
}
