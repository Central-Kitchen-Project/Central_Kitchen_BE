using CentralKitchen_Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Repositories.Repository
{
    public class LoginRepo 
    {
        private readonly CentralKitchenDBContext _context;

        public LoginRepo(CentralKitchenDBContext context)
        {
            _context = context;
        }

        public User? GetAccount(string email, string password)
        {
           
            return _context.Users
                 .Include(u => u.Role)
                .FirstOrDefault(
                   a => a.Email == email && a.PasswordHash == password
            );
        }
    }
}
