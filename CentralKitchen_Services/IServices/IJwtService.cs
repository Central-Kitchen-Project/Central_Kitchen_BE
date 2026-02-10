using CentralKitchen_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface IJwtService
    {
        
        string GenerateToken(User acc);
    }
}
