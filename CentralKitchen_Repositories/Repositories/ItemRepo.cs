using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        /// <summary>
        /// Create a new item and automatically generate inventory records
        /// (quantity 0) for every user who manages inventory.
        /// </summary>
        public async Task<Item?> CreateItemWithInventoryAsync(Item item)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                item.CreatedAt = DateTime.Now;
                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Get distinct (ManagedBy, LocationId) pairs from existing inventory
                var inventoryOwners = await _context.Inventories
                    .Where(inv => inv.ManagedBy != null)
                    .Select(inv => new { inv.ManagedBy, inv.LocationId })
                    .Distinct()
                    .ToListAsync();

                if (inventoryOwners.Count == 0)
                {
                    // Fallback: create one record per user with SupplyCoordinator or FranchiseStore role
                    var defaultLocation = await _context.Locations.FirstOrDefaultAsync();
                    var locationId = defaultLocation?.Id ?? 1;

                    var users = await _context.Users
                        .Include(u => u.Role)
                        .Where(u => u.Role.RoleName == "SupplyCoordinator"
                            || u.Role.RoleName == "FranchiseStore")
                        .Select(u => u.Id)
                        .ToListAsync();

                    foreach (var userId in users)
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            ItemId = item.Id,
                            LocationId = locationId,
                            Quantity = 0,
                            ManagedBy = userId
                        });
                    }
                }
                else
                {
                    foreach (var owner in inventoryOwners)
                    {
                        _context.Inventories.Add(new Inventory
                        {
                            ItemId = item.Id,
                            LocationId = owner.LocationId,
                            Quantity = 0,
                            ManagedBy = owner.ManagedBy
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetItemByIdAsync(item.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }
    }

    public class CategoryCountResult
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }
}
