using CentralKitchen_Repositories.Models;
using CentralKitchen_Repositories.Repository;
using CentralKitchen_Repositories.Repositories;
using CentralKitchen_Services.IServices;
using CentralKitchen_Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll",
		policy =>
		{
			policy.AllowAnyOrigin()
				  .AllowAnyHeader()
				  .AllowAnyMethod();
		});
});
builder.Services.AddDbContext <CentralKitchenDBContext> (options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<LoginRepo>();
builder.Services.AddScoped<OrderRepo>();
builder.Services.AddScoped<MaterialRequestRepo>();
builder.Services.AddScoped<ItemRepo>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMaterialRequestService, MaterialRequestService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<CentralKitchen_Services.IServices.IEmailService, CentralKitchen_Services.Services.EmailService>();
builder.Services.AddScoped<InventoryRepo>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
var configuration = builder.Configuration;
// 1. Đăng ký IJwtService
builder.Services.AddScoped<IJwtService, JwtService>();

// 2. Cấu hình JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
    };
});

// Thêm Authorization Middleware
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
