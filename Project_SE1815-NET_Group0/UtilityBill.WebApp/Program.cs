using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Rewrite; // <-- USING MỚI CẦN THIẾT
using UtilityBill.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ cần thiết vào container.
// Đưa về dạng mặc định, không cần options
builder.Services.AddRazorPages();

// Cấu hình HttpClient để gọi API
builder.Services.AddHttpClient("ApiClient", client =>
{
    // Nhớ thay cổng 7240 cho khớp với API của bạn
    client.BaseAddress = new Uri("https://localhost:7240/api/");
});

// Cấu hình xác thực bằng Cookie cho WebApp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Các dịch vụ khác
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiClient, ApiClient>();

var app = builder.Build();

// Cấu hình pipeline xử lý HTTP request.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.MapGet("/", async context =>
{
    context.Response.Redirect("/MyHomepage");
});
app.UseHttpsRedirection();


// ==========================================================
// === THÊM ĐOẠN CODE CHUYỂN HƯỚNG VÀO ĐÂY ===
var rewriteOptions = new RewriteOptions()
    // Thêm quy tắc: Nếu URL là rỗng (trang chủ), chuyển hướng vĩnh viễn đến /MyHomepage
    .AddRedirect("^$", "MyHomepage", 301);

app.UseRewriter(rewriteOptions);
// ==========================================================


app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLower();
        if (path.EndsWith(".html") || path.EndsWith(".htm"))
        {
            ctx.Context.Response.StatusCode = 404;
        }
    }
});

app.UseRouting();

// Middleware tùy chỉnh để redirect root về MyHomepage
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "/index.html")
    {
        context.Response.Redirect("/MyHomepage", permanent: false);
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();