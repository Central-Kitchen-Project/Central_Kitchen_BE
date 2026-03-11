using CentralKitchen_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repositories
{
    public class RecipeRepo
    {
        private readonly CentralKitchenDBContext _context;
        public RecipeRepo(CentralKitchenDBContext context) => _context = context;

        public async Task UpdateRecipeAsync(int finishedItemId, List<Recipe> newRecipes)
        {
            // Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tìm và xóa tất cả nguyên liệu cũ của thành phẩm này trong bảng recipes
                var oldRecipes = _context.Recipes.Where(r => r.FinishedItemId == finishedItemId);
                _context.Recipes.RemoveRange(oldRecipes);

                // 2. Thêm danh sách nguyên liệu mới
                if (newRecipes != null && newRecipes.Any())
                {
                    await _context.Recipes.AddRangeAsync(newRecipes);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
