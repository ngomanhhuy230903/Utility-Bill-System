using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.Services;
using UtilityBill.Data.Context;
using UtilityBill.Data.Models;
using UtilityBill.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UtilityBillDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // <-- Dịch vụ mới của chúng ta
builder.Services.AddScoped<ITenantHistoryRepository, TenantHistoryRepository>();
// Cấu hình xác thực JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... cấu hình JWT như cũ ... */ });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. Kích hoạt Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Thêm class Role trống để tương thích với Identity
public partial class Role : IdentityRole<string> { }