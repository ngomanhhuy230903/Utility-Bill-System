using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UtilityBill.Api.BackgroundServices;
using UtilityBill.Business.Interfaces;
using UtilityBill.Business.Services;
using UtilityBill.Data.Context;
using UtilityBill.Data.Repositories;
using QuestPDF.Infrastructure;
using UtilityBill.Api.Services.Momo;
using UtilityBill.Api.Services.VnPay;
using UtilityBill.Data.Models.Momo;

using UtilityBill.Business.Settings;
using Microsoft.AspNetCore.Identity;
using UtilityBill.Data.Models;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// Đăng ký các dịch vụ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UtilityBillDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
builder.Services.AddScoped<ITenantHistoryRepository, TenantHistoryRepository>();
builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
//builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IBillingConfigService, BillingConfigService>(); // Thêm lại dòng này nếu bạn đã xóa
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// Thay thế đăng ký Email Service cũ bằng cái mới
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
// Đăng ký service nghiệp vụ
builder.Services.AddScoped<IBillingService, BillingService>();

// Đăng ký background job
builder.Services.AddHostedService<MonthlyBillingJob>();

// Cấu hình xác thực JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7240")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS to allow WebApp to access API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:7082", "http://localhost:5164")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//Connect MOMO API
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
//Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Thêm Hangfire vào DI container
builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage(); // Hoặc UseSqlServerStorage(...) nếu dùng SQL Server
});
builder.Services.AddHangfireServer(); // Cho phép chạy background jobs
builder.Services.AddScoped<MaintenanceReminderJob>(); // Đăng ký job nếu dùng DI

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseCors("AllowFrontend");

// Enable CORS
app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

// Kích hoạt Hangfire Dashboard (tùy chọn, hữu ích để debug)
app.UseHangfireDashboard("/hangfire"); // => https://localhost:7240/hangfire

app.MapControllers();

RecurringJob.AddOrUpdate<MaintenanceReminderJob>(
    "maintenance-reminder",
    job => job.Execute(),
    Cron.Daily,
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

app.Run();