using CentralKitchen_Repositories.Models;
using CentralKitchen_Services.DTOs;
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
        Task<bool> Register(RegisterRequestDTO registerDto);
        Task<bool> ChangePassword(ChangePasswordDTO changePasswordDto);
    }
}
