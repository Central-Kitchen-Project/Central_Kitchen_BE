using CentralKitchen_Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IRecipeService
    {
        Task<bool> UpdateFinishedProductRecipeAsync(UpdateRecipeDto dto);
    }
}
