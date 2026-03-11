using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly RecipeRepo _recipeRepo;
        public RecipeService(RecipeRepo recipeRepo) => _recipeRepo = recipeRepo;

        public async Task<bool> UpdateFinishedProductRecipeAsync(UpdateRecipeDto dto)
        {
            // Chuyển đổi từ DTO sang List<recipes> Entity
            var recipeEntities = dto.Ingredients.Select(i => new Recipe
            {
                FinishedItemId = dto.FinishedItemId,
                IngredientItemId = i.IngredientItemId,
                Quantity = i.Quantity
            }).ToList();

            try
            {
                await _recipeRepo.UpdateRecipeAsync(dto.FinishedItemId, recipeEntities);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    }
