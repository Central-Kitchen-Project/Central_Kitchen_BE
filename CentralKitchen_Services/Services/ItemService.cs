using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class ItemService : IItemService
    {
        private readonly ItemRepo _itemRepo;

        public ItemService(ItemRepo itemRepo)
        {
            _itemRepo = itemRepo;
        }

        public async Task<List<ProductCatalogItemDTO>> GetAllItemsAsync(string? type = null, string? category = null, string? search = null)
        {
            var items = await _itemRepo.GetAllItemsWithRecipesAsync(type, category, search);
            return items.Select(MapToDTO).ToList();
        }

        public async Task<ProductCatalogItemDTO?> GetItemByIdAsync(int id)
        {
            var item = await _itemRepo.GetItemByIdAsync(id);
            if (item == null) return null;
            return MapToDTO(item);
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _itemRepo.GetCategoriesAsync();
        }

        public async Task<List<CategoryDTO>> GetCategoriesWithCountAsync()
        {
            var results = await _itemRepo.GetCategoriesWithCountAsync();
            return results.Select(r => new CategoryDTO
            {
                Category = r.Category,
                Count = r.Count
            }).ToList();
        }

        private ProductCatalogItemDTO MapToDTO(Item item)
        {
            return new ProductCatalogItemDTO
            {
                Id = item.Id,
                Name = item.ItemName ?? "",
                Type = item.ItemType ?? "",
                Category = item.Category ?? "",
                Description = item.Description ?? "",
                Unit = item.Unit ?? "",
                Price = item.Price,
                Ingredients = item.RecipeFinishedItems?.Select(r => new IngredientDTO
                {
                    Name = r.IngredientItem?.ItemName ?? "",
                    Qty = r.Quantity,
                    Unit = r.IngredientItem?.Unit ?? ""
                }).ToList() ?? new List<IngredientDTO>()
            };
        }
    }
}
