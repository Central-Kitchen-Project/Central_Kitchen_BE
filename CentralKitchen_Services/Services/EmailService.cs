using CentralKitchen_Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CentralKitchen_Services.Services
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
		{
			try
			{
				var smtpSection = _configuration.GetSection("Smtp");

				var host = smtpSection["Host"];
				var port = int.Parse(smtpSection["Port"] ?? "25");
				var enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "false");
				var deliveryMethod = smtpSection["DeliveryMethod"]; // "PickupDirectory" or "Network"
				var pickupDir = smtpSection["PickupDirectoryLocation"];
				var user = smtpSection["User"];
				var pass = smtpSection["Pass"];
				var from = smtpSection["From"] ?? user ?? "no-reply@localhost";

				using var client = new SmtpClient(host, port);

				// Configure delivery method
				if (!string.IsNullOrEmpty(deliveryMethod) &&
					deliveryMethod.Equals("PickupDirectory", StringComparison.OrdinalIgnoreCase))
				{
					client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
					client.PickupDirectoryLocation = pickupDir;
				}
				else
				{
					client.EnableSsl = enableSsl;

					if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
					{
						client.Credentials = new NetworkCredential(user, pass);
					}
				}

				// Validate email addresses
				var fromAddress = new MailAddress(from.Trim());
				var toAddress = new MailAddress((toEmail ?? string.Empty).Trim());

				using var mail = new MailMessage(fromAddress, toAddress)
				{
					Subject = subject,
					Body = htmlBody,
					IsBodyHtml = true
				};

				await client.SendMailAsync(mail);

				_logger.LogInformation("Email sent successfully to {Email}", toEmail);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to send email to {Email}", toEmail);
				return false;
			}
		}
	}
}
