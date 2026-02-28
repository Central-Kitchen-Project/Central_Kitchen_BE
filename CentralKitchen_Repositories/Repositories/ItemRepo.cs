using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class ItemRepo
    {
        private readonly CentralKitchenDBContext _context;

        public ItemRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all items with recipe ingredients. Supports type, category, and search filters.
        /// </summary>
        public async Task<List<Item>> GetAllItemsWithRecipesAsync(string? type = null, string? category = null, string? search = null)
        {
            var query = _context.Items
                .Include(i => i.RecipeFinishedItems)
                    .ThenInclude(r => r.IngredientItem)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(i => i.ItemType == type);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(i => i.Category == category);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.ItemName.Contains(search) || (i.Description != null && i.Description.Contains(search)));

            return await query.OrderBy(i => i.ItemType).ThenBy(i => i.ItemName).ToListAsync();
        }

        /// <summary>
        /// Get single item by ID with recipe ingredients
        /// </summary>
        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.RecipeFinishedItems)
                    .ThenInclude(r => r.IngredientItem)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// Get all distinct categories
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.Items
                .Where(i => i.Category != null)
                .Select(i => i.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        /// <summary>
        /// Get categories with item count (for tab badges on UI)
        /// </summary>
        public async Task<List<CategoryCountResult>> GetCategoriesWithCountAsync()
        {
            return await _context.Items
                .Where(i => i.Category != null)
                .GroupBy(i => i.Category)
                .Select(g => new CategoryCountResult
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderBy(c => c.Category)
                .ToListAsync();
        }
    }

    public class CategoryCountResult
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }
}
