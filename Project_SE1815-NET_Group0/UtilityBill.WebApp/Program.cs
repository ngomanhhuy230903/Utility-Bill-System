using Microsoft.AspNetCore.Authentication.Cookies;
using UtilityBill.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ cần thiết vào container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Map the Index page to the root URL
        options.Conventions.AddPageRoute("/Index", "");
    });

// Cấu hình HttpClient để gọi API
// !!! QUAN TRỌNG: Hãy đảm bảo số cổng (ví dụ: 7240) khớp chính xác với cổng API của bạn.
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7240/api/");
});
// Cấu hình xác thực bằng Cookie cho WebApp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true; // Gia hạn thời gian nếu người dùng hoạt động
        options.Cookie.Name = ".AspNetCore.Cookies";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Dịch vụ để 'đọc' được HttpContext hiện tại (cần cho việc lấy Token từ Cookie)
builder.Services.AddHttpContextAccessor();

// Đăng ký dịch vụ ApiClient mà chúng ta đã tạo
builder.Services.AddScoped<IApiClient, ApiClient>();

var app = builder.Build();

// Cấu hình pipeline xử lý HTTP request.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();