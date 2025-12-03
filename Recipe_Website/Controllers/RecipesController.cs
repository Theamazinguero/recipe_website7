// File: Controllers/RecipesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Website.Models;
using RecipeApp.Api.Data;
using RecipeApp.Api.DTOs;
using RecipeApp.Api.Models;
using System.Security.Claims;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RecipesController(AppDbContext db)
        {
            _db = db;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        // Public browse/search (only approved & not disabled)
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RecipeResponse>>> GetRecipes([FromQuery] string? search = null)
        {
            var query = _db.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
                .Where(r => r.IsApproved && !r.IsDisabled);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(s) ||
                    (r.ShortDescription ?? "").ToLower().Contains(s));
            }

            var recipes = await query.ToListAsync();

            var result = recipes.Select(r => new RecipeResponse
            {
                Id = r.Id,
                Title = r.Title,
                ShortDescription = r.ShortDescription,
                ImageUrl = r.ImageUrl,
                IsApproved = r.IsApproved,
                IsDisabled = r.IsDisabled,
                CreatedByDisplayName = r.CreatedByUser?.DisplayName ?? "",
                CreatedAtUtc = r.CreatedAtUtc,
                Tags = r.RecipeTags.Select(rt => rt.Tag!.Name),
                Ingredients = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }),
                Steps = r.Steps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new RecipeStepDto
                    { StepNumber = s.StepNumber, Description = s.Description })
            });

            return Ok(result);
        }

        // ?? list recipes created by the current logged-in user
        // GET /api/recipes/mine
        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RecipeResponse>>> GetMyRecipes()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipes = await _db.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
                .Where(r => r.CreatedByUserId == userId && !r.IsDisabled)
                .ToListAsync();

            var result = recipes.Select(r => new RecipeResponse
            {
                Id = r.Id,
                Title = r.Title,
                ShortDescription = r.ShortDescription,
                ImageUrl = r.ImageUrl,
                IsApproved = r.IsApproved,
                IsDisabled = r.IsDisabled,
                CreatedByDisplayName = r.CreatedByUser?.DisplayName ?? "",
                CreatedAtUtc = r.CreatedAtUtc,
                Tags = r.RecipeTags.Select(rt => rt.Tag!.Name),
                Ingredients = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }),
                Steps = r.Steps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new RecipeStepDto
                    { StepNumber = s.StepNumber, Description = s.Description })
            });

            return Ok(result);
        }

        // ?? NEW: list recipes favorited by the current logged-in user
        // GET /api/recipes/favorites
        [HttpGet("favorites")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RecipeResponse>>> GetMyFavorites([FromQuery] string? search = null)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // base query: approved, not disabled, favorited by this user
            var query = _db.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
                .Where(r => r.IsApproved && !r.IsDisabled)
                .Where(r => _db.Favorites.Any(f => f.UserId == userId && f.RecipeId == r.Id));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(s) ||
                    (r.ShortDescription ?? "").ToLower().Contains(s));
            }

            var recipes = await query.ToListAsync();

            var result = recipes.Select(r => new RecipeResponse
            {
                Id = r.Id,
                Title = r.Title,
                ShortDescription = r.ShortDescription,
                ImageUrl = r.ImageUrl,
                IsApproved = r.IsApproved,
                IsDisabled = r.IsDisabled,
                CreatedByDisplayName = r.CreatedByUser?.DisplayName ?? "",
                CreatedAtUtc = r.CreatedAtUtc,
                Tags = r.RecipeTags.Select(rt => rt.Tag!.Name),
                Ingredients = r.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }),
                Steps = r.Steps
                    .OrderBy(s => s.StepNumber)
                    .Select(s => new RecipeStepDto
                    { StepNumber = s.StepNumber, Description = s.Description })
            });

            return Ok(result);
        }

        // Create recipe (user) -> goes to approval queue
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RecipeResponse>> CreateRecipe(RecipeCreateRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipe = new Recipe
            {
                Title = request.Title,
                ShortDescription = request.ShortDescription ?? "",
                InstructionsSummary = request.InstructionsSummary,
                ImageUrl = request.ImageUrl,
                CreatedByUserId = userId,
                IsApproved = false, // approval required
                IsDisabled = false
            };

            foreach (var ing in request.Ingredients)
            {
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Name = ing.Name,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit
                });
            }

            foreach (var step in request.Steps.OrderBy(s => s.StepNumber))
            {
                recipe.Steps.Add(new RecipeStep
                {
                    StepNumber = step.StepNumber,
                    Description = step.Description
                });
            }

            // Tags: create or reuse
            foreach (var tagName in request.Tags.Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)))
            {
                var existingTag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (existingTag == null)
                {
                    existingTag = new Tag
                    {
                        Name = tagName,
                        Slug = tagName.ToLower().Replace(" ", "-")
                    };
                    _db.Tags.Add(existingTag);
                }

                recipe.RecipeTags.Add(new RecipeTag
                {
                    Tag = existingTag
                });
            }

            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, new { recipe.Id });
        }

        // View single recipe (only approved or own)
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RecipeResponse>> GetRecipeById(int id)
        {
            var userId = GetUserId();

            var recipe = await _db.Recipes
                .Include(r => r.CreatedByUser)
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            if ((!recipe.IsApproved || recipe.IsDisabled) &&
                recipe.CreatedByUserId != userId &&
                !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var response = new RecipeResponse
            {
                Id = recipe.Id,
                Title = recipe.Title,
                ShortDescription = recipe.ShortDescription,
                ImageUrl = recipe.ImageUrl,
                IsApproved = recipe.IsApproved,
                IsDisabled = recipe.IsDisabled,
                CreatedByDisplayName = recipe.CreatedByUser?.DisplayName ?? "",
                CreatedAtUtc = recipe.CreatedAtUtc,
                Tags = recipe.RecipeTags.Select(rt => rt.Tag!.Name),
                Ingredients = recipe.Ingredients.Select(i => new RecipeIngredientDto
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }),
                Steps = recipe.Steps.OrderBy(s => s.StepNumber).Select(s => new RecipeStepDto
                {
                    StepNumber = s.StepNumber,
                    Description = s.Description
                })
            };

            return Ok(response);
        }

        // Update own recipe (only if not approved yet or allow editing rules)
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateRecipe(int id, RecipeCreateRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipe = await _db.Recipes
                .Include(r => r.Ingredients)
                .Include(r => r.Steps)
                .Include(r => r.RecipeTags)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();
            if (recipe.CreatedByUserId != userId && !User.IsInRole("Admin")) return Forbid();

            recipe.Title = request.Title;
            recipe.ShortDescription = request.ShortDescription ?? "";
            recipe.InstructionsSummary = request.InstructionsSummary;
            recipe.ImageUrl = request.ImageUrl;
            recipe.UpdatedAtUtc = DateTime.UtcNow;
            recipe.IsApproved = false; // require re-approval on edit

            // Replace ingredients
            _db.RecipeIngredients.RemoveRange(recipe.Ingredients);
            recipe.Ingredients.Clear();
            foreach (var ing in request.Ingredients)
            {
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Name = ing.Name,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit
                });
            }

            // Replace steps
            _db.RecipeSteps.RemoveRange(recipe.Steps);
            recipe.Steps.Clear();
            foreach (var step in request.Steps.OrderBy(s => s.StepNumber))
            {
                recipe.Steps.Add(new RecipeStep
                {
                    StepNumber = step.StepNumber,
                    Description = step.Description
                });
            }

            // Replace tags
            _db.RecipeTags.RemoveRange(recipe.RecipeTags);
            recipe.RecipeTags.Clear();

            foreach (var tagName in request.Tags.Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)))
            {
                var existingTag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (existingTag == null)
                {
                    existingTag = new Tag
                    {
                        Name = tagName,
                        Slug = tagName.ToLower().Replace(" ", "-")
                    };
                    _db.Tags.Add(existingTag);
                }

                recipe.RecipeTags.Add(new RecipeTag
                {
                    Tag = existingTag
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Delete own recipe
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            if (recipe.CreatedByUserId != userId && !User.IsInRole("Admin")) return Forbid();

            _db.Recipes.Remove(recipe);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Favorite / Unfavorite
        [HttpPost("{id:int}/favorite")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipe = await _db.Recipes
                .FirstOrDefaultAsync(r => r.Id == id && r.IsApproved && !r.IsDisabled);
            if (recipe == null) return NotFound();

            var existing = await _db.Favorites
                .FirstOrDefaultAsync(f => f.RecipeId == id && f.UserId == userId);
            if (existing != null)
            {
                _db.Favorites.Remove(existing);
            }
            else
            {
                _db.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    RecipeId = id
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Report recipe
        [HttpPost("{id:int}/report")]
        [Authorize]
        public async Task<IActionResult> ReportRecipe(int id, [FromBody] string reason)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var recipe = await _db.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            var report = new Report
            {
                ReporterUserId = userId,
                RecipeId = id,
                Reason = reason
            };

            _db.Reports.Add(report);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
