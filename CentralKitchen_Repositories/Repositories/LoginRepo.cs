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

        public User? GetAccountByEmail(string email)
        {
            return _context.Users
                 .Include(u => u.Role) 
                 .FirstOrDefault(a => a.Email == email);
        }

        public async Task<bool> RegisterUser(User user)
        {
            _context.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UserExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }
    }
}
