using CentralKitchen_Services.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IItemService
    {
        Task<List<ProductCatalogItemDTO>> GetAllItemsAsync(string? type = null, string? category = null, string? search = null);
        Task<ProductCatalogItemDTO?> GetItemByIdAsync(int id);
        Task<List<string>> GetCategoriesAsync();
        Task<List<CategoryDTO>> GetCategoriesWithCountAsync();
        Task<ProductCatalogItemDTO?> CreateItemAsync(CreateItemDTO dto);
        Task<bool> CreateRecipeAsync(CreateFinishedProductDto dto);
        Task<bool> UpdateItemAsync(int id, ItemUpdateDto dto);
    }
}

