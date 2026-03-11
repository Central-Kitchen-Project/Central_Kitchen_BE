using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
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

        public async Task<ProductCatalogItemDTO?> CreateItemAsync(CreateItemDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemName))
                return null;

            var item = new Item
            {
                ItemName = dto.ItemName,
                Unit = dto.Unit,
                ItemType = dto.ItemType,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category
            };

            var created = await _itemRepo.CreateItemWithInventoryAsync(item);
            if (created == null) return null;
            return MapToDTO(created);
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

        public async Task<bool> CreateRecipeAsync(CreateFinishedProductDto dto)
        {
            if (!await _itemRepo.ItemExistsAsync(dto.FinishedItemId))
                return false;

            var recipeEntities = dto.Ingredients.Select(ing => new Recipe
            {
                FinishedItemId = dto.FinishedItemId,
                IngredientItemId = ing.IngredientItemId,
                Quantity = ing.Quantity
            }).ToList();

            await _itemRepo.AddRecipesAsync(recipeEntities);
            return true;
        }
        public async Task<bool> UpdateItemAsync(int id, ItemUpdateDto dto)
        {
            var existingItem = await _itemRepo.GetItemByIdAsync(id);
            if (existingItem == null) return false;

            // Cập nhật các trường thông tin cơ bản
            existingItem.ItemName = dto.ItemName;
            existingItem.Unit = dto.Unit;
            existingItem.ItemType = dto.ItemType;
            existingItem.Description = dto.Description;
            existingItem.Price = dto.Price;
            existingItem.Category = dto.Category;
            // created_at thường giữ nguyên không cập nhật

            await _itemRepo.UpdateAsync(existingItem);
            return true;
        }

        public async Task<bool> SoftDeleteItemAsync(int id)
        {
            var item = await _itemRepo.GetItemByIdAsync(id);

        
            if (item == null ) return false;

            // Cập nhật trạng thái
            item.IsActive = false;

            await _itemRepo.UpdateAsync(item);
            return true;
        }
    }
}
