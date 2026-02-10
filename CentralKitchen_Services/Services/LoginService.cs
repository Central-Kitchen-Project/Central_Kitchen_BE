using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repository;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
    public class LoginService : ILoginService
    {

        private readonly LoginRepo _repository;

        public LoginService(LoginRepo repository)
        {
            _repository = repository;
        }
        public User Login(string email, string password)
        {
            
            return _repository.GetAccount(email, password);
        }
    }
}
