using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repository;
using CentralKitchen_Services.DTOs;
using CentralKitchen_Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CentralKitchen_Services.Services
{
	public class LoginService : ILoginService
	{

		private readonly LoginRepo _repository;
		private readonly IEmailService _emailService;
		private readonly CentralKitchen_Repositories.Models.CentralKitchenDBContext _context;

		public LoginService(LoginRepo repository, IEmailService emailService, CentralKitchen_Repositories.Models.CentralKitchenDBContext context)
		{
			_repository = repository;
			_emailService = emailService;
			_context = context;
		}

		public async Task<bool> ResetPassword(ResetPasswordDTO resetDto)
		{
			// Validate input
			if (resetDto == null || string.IsNullOrEmpty(resetDto.Email) || string.IsNullOrEmpty(resetDto.Token) || string.IsNullOrEmpty(resetDto.NewPassword))
			{
				return false;
			}

			var key = $"forgot_{resetDto.Email}";
			var existing = _context.SystemParameters.FirstOrDefault(p => p.ParamKey == key);
			if (existing == null)
			{
				return false;
			}

			if (existing.ParamValue != resetDto.Token)
			{
				return false;
			}

			// Token valid - update password
			var newHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
			var updated = await _repository.UpdatePassword(resetDto.Email, newHash);
			if (!updated)
			{
				return false;
			}

			// Remove the token entry
				_context.SystemParameters.Remove(existing);
				await _context.SaveChangesAsync();

			return true;
		}
		public User? Login(string email, string password)
		{
			var user = _repository.GetAccountByEmail(email);
			if (user == null) return null;

			bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
			return isPasswordValid ? user : null;


			// Stored password is not a valid bcrypt hash (legacy/plaintext). Fallback to direct comparison.
			if (user.PasswordHash == password)
			{
				// Upgrade stored password to bcrypt hash

				var newHash = BCrypt.Net.BCrypt.HashPassword(password);
				// Update synchronously (repository method is async)
				_repository.UpdatePassword(user.Email, newHash).GetAwaiter().GetResult();

				return user;
			}

			return null;

		}

		public async Task<bool> Register(RegisterRequestDTO registerDto)
		{
			if (await _repository.UserExists(registerDto.Username))
			{
				return false;
			}

			string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

			var newUser = new User
			{
				Username = registerDto.Username,
				Email = registerDto.Email,
				PasswordHash = passwordHash,
				RoleId = registerDto.RoleId,
				CreatedAt = DateTime.Now
			};

			var result = await _repository.RegisterUser(newUser);

			// Auto-create inventory for Franchise Store Staff and Supply Coordinator
			if (result)
			{
				var roleName = _context.Roles
					.Where(r => r.Id == registerDto.RoleId)
					.Select(r => r.RoleName)
					.FirstOrDefault();

				if (roleName == "FranchiseStore" || roleName == "SupplyCoordinator")
				{
					var activeItems = _context.Items.Where(i => i.IsActive).ToList();
					var defaultLocationId = _context.Locations.Select(l => l.Id).FirstOrDefault();

					foreach (var item in activeItems)
					{
						_context.Inventories.Add(new Inventory
						{
							ItemId = item.Id,
							LocationId = defaultLocationId,
							Quantity = 0,
							ManagedBy = newUser.Id
						});
					}

					await _context.SaveChangesAsync();
				}
			}

			return result;
		}

		public async Task<bool> ChangePassword(ChangePasswordDTO changePasswordDto)
		{
			// 1. Tìm user theo Email
			var user = _repository.GetAccountByEmail(changePasswordDto.Email);
			if (user == null) return false;

			// 2. Kiểm tra mật khẩu cũ có đúng không
			bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash);
			if (!isOldPasswordValid) return false;

			// 3. Hash mật khẩu mới
			string newHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

			// 4. Cập nhật vào DB
			return await _repository.UpdatePassword(changePasswordDto.Email, newHash);
		}

		public async Task<bool> ForgotPassword(string email)
		{
			var user = _repository.GetAccountByEmail(email);
			if (user == null)
			{
				return false;
			}

			// Generate a simple token (in production use a secure token and expiry)
			var token = System.Guid.NewGuid().ToString();

			// Store token in system_parameters table with key 'forgot_{email}' and value token
			var key = $"forgot_{email}";
			var param = await _context.SystemParameters.FindAsync(0);
			var existing = _context.SystemParameters.FirstOrDefault(p => p.ParamKey == key);
			if (existing == null)
			{
				_context.SystemParameters.Add(new CentralKitchen_Repositories.Models.SystemParameter { ParamKey = key, ParamValue = token });
			}
			else
			{
				existing.ParamValue = token;
				_context.SystemParameters.Update(existing);
			}
			await _context.SaveChangesAsync();

			var resetLink = $"http://localhost:5173/reset-password?email={email}&token={token}";
			var subject = "CentralKitchen - Reset your password";
			var body = $"<p>Hello {user.Username},</p><p>Click the link to reset your password:</p><p><a href=\"{resetLink}\">Reset Password</a></p>";

			var sendResult = await _emailService.SendEmailAsync(email, subject, body);

			return sendResult;
		}
	}
}

