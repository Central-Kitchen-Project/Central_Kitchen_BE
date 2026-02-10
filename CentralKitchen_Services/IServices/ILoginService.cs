using CentralKitchen_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.IServices
{
    public interface ILoginService
    {
        User Login(string email, string password);
    }
}
