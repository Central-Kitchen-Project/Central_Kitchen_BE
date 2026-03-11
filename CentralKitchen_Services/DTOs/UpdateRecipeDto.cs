using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.DTOs
{
    public class UpdateRecipeDto
    {
        public int FinishedItemId { get; set; } 
        public List<RecipeUpdateLineDto> Ingredients { get; set; } = new List<RecipeUpdateLineDto>();
    }
}
