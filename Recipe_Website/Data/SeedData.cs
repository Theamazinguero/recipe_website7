// File: Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recipe_Website.Models;     // Recipe, RecipeIngredient, Tag
using RecipeApp.Api.Models;      // ApplicationUser, RecipeStep, RecipeTag

namespace RecipeApp.Api.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services, string issuer)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<AppDbContext>();

            // Make sure DB exists / migrations applied
            await context.Database.MigrateAsync();

            // ---------- Seed roles ----------
            string[] roles = { "User", "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ---------- Seed default admin ----------
            const string adminEmail = "admin@recipeapp.local";
            const string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure existing admin user has Admin role
                var userRoles = await userManager.GetRolesAsync(adminUser);
                if (!userRoles.Contains("Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var adminUserId = adminUser?.Id ?? string.Empty;

            // ---------- Tags (reuse if they already exist) ----------
            async Task<Tag> GetOrCreateTagAsync(string name, string slug)
            {
                var tag = await context.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
                if (tag != null) return tag;

                tag = new Tag { Name = name, Slug = slug };
                context.Tags.Add(tag);
                return tag;
            }

            var breakfastTag = await GetOrCreateTagAsync("Breakfast", "breakfast");
            var quickTag = await GetOrCreateTagAsync("Quick", "quick");
            var dinnerTag = await GetOrCreateTagAsync("Dinner", "dinner");
            var pastaTag = await GetOrCreateTagAsync("Pasta", "pasta");
            var healthyTag = await GetOrCreateTagAsync("Healthy", "healthy");
            var brunchTag = await GetOrCreateTagAsync("Brunch", "brunch");

            // ---------- 1) Seeded Pancakes ----------
            if (!await context.Recipes.AnyAsync(r => r.Title == "Seeded Pancakes"))
            {
                var recipe1 = new Recipe
                {
                    Title = "Seeded Pancakes",
                    ShortDescription = "Fluffy pancakes seeded from SeedData so you can test the Details page.",
                    InstructionsSummary = "Mix ingredients in a bowl and cook on a lightly oiled pan.",
                    ImageUrl = null,
                    IsApproved = true,
                    IsDisabled = false,
                    CreatedByUserId = adminUserId,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new RecipeIngredient { Name = "Flour", Unit = "cup" },
                        new RecipeIngredient { Name = "Milk",  Unit = "cup" },
                        new RecipeIngredient { Name = "Egg",   Unit = "pc"  }
                    },
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep { StepNumber = 1, Description = "Whisk flour, milk, and egg together until smooth." },
                        new RecipeStep { StepNumber = 2, Description = "Heat a non-stick pan over medium heat and lightly oil it." },
                        new RecipeStep { StepNumber = 3, Description = "Pour batter, cook until bubbles form, flip and cook until golden." }
                    },
                    RecipeTags = new List<RecipeTag>
                    {
                        new RecipeTag { Tag = breakfastTag },
                        new RecipeTag { Tag = quickTag     }
                    }
                };

                context.Recipes.Add(recipe1);
            }

            // ---------- 2) Creamy Chicken Alfredo Pasta ----------
            if (!await context.Recipes.AnyAsync(r => r.Title == "Creamy Chicken Alfredo Pasta"))
            {
                var recipe2 = new Recipe
                {
                    Title = "Creamy Chicken Alfredo Pasta",
                    ShortDescription = "A rich and creamy Alfredo pasta tossed with tender sliced chicken.",
                    InstructionsSummary = "Cook pasta, sear chicken, and combine with homemade Alfredo sauce.",
                    ImageUrl = null,
                    IsApproved = true,
                    IsDisabled = false,
                    CreatedByUserId = adminUserId,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new RecipeIngredient { Name = "Fettuccine",      Unit = "g"   },
                        new RecipeIngredient { Name = "Chicken Breast",  Unit = "pc"  },
                        new RecipeIngredient { Name = "Heavy Cream",     Unit = "cup" },
                        new RecipeIngredient { Name = "Parmesan Cheese", Unit = "cup" },
                        new RecipeIngredient { Name = "Butter",          Unit = "tbsp"}
                    },
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep { StepNumber = 1, Description = "Boil fettuccine until al dente. Reserve some pasta water." },
                        new RecipeStep { StepNumber = 2, Description = "Season and pan-sear chicken until golden, then slice." },
                        new RecipeStep { StepNumber = 3, Description = "Melt butter, add cream, and simmer until slightly thick." },
                        new RecipeStep { StepNumber = 4, Description = "Whisk in Parmesan cheese until smooth." },
                        new RecipeStep { StepNumber = 5, Description = "Toss pasta and sliced chicken in the sauce, adding pasta water if needed." }
                    },
                    RecipeTags = new List<RecipeTag>
                    {
                        new RecipeTag { Tag = dinnerTag },
                        new RecipeTag { Tag = pastaTag  }
                    }
                };

                context.Recipes.Add(recipe2);
            }

            // ---------- 3) Avocado Toast with Eggs ----------
            if (!await context.Recipes.AnyAsync(r => r.Title == "Avocado Toast with Eggs"))
            {
                var recipe3 = new Recipe
                {
                    Title = "Avocado Toast with Eggs",
                    ShortDescription = "Crispy sourdough topped with creamy avocado and soft-boiled eggs.",
                    InstructionsSummary = "Toast bread, mash avocado, and top with soft-boiled eggs.",
                    ImageUrl = null,
                    IsApproved = true,
                    IsDisabled = false,
                    CreatedByUserId = adminUserId,
                    Ingredients = new List<RecipeIngredient>
                    {
                        new RecipeIngredient { Name = "Sourdough Bread", Unit = "slice" },
                        new RecipeIngredient { Name = "Avocado",         Unit = "pc"    },
                        new RecipeIngredient { Name = "Egg",             Unit = "pc"    },
                        new RecipeIngredient { Name = "Lemon Juice",     Unit = "tsp"   },
                        new RecipeIngredient { Name = "Salt & Pepper",   Unit = ""      }
                    },
                    Steps = new List<RecipeStep>
                    {
                        new RecipeStep { StepNumber = 1, Description = "Toast the sourdough slices until golden and crispy." },
                        new RecipeStep { StepNumber = 2, Description = "Mash avocado with lemon juice, salt, and pepper." },
                        new RecipeStep { StepNumber = 3, Description = "Soft-boil eggs for about 6 minutes, peel, and slice." },
                        new RecipeStep { StepNumber = 4, Description = "Spread avocado on toast and top with egg slices." }
                    },
                    RecipeTags = new List<RecipeTag>
                    {
                        new RecipeTag { Tag = breakfastTag },
                        new RecipeTag { Tag = healthyTag   },
                        new RecipeTag { Tag = brunchTag    }
                    }
                };

                context.Recipes.Add(recipe3);
            }

            await context.SaveChangesAsync();
        }
    }
}
