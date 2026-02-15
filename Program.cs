using EagleFlow.Data;
using EagleFlow.Models;
using EagleFlow.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDocumentNumberGenerator, DocumentNumberGenerator>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ISmsSender, SmsSender>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseInMemoryDatabase("EagleFlowDocuments");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    if (!await dbContext.AdminUsers.AnyAsync())
    {
        var seedEmail = builder.Configuration["AdminAuth:SeedAdmin:Email"] ?? "admin@eagleflow.local";
        var seedPassword = builder.Configuration["AdminAuth:SeedAdmin:Password"] ?? "Admin@123";
        var seedMobile = builder.Configuration["AdminAuth:SeedAdmin:MobileNumber"];

        var admin = new AdminUser
        {
            Email = seedEmail.Trim().ToLowerInvariant(),
            MobileNumber = seedMobile,
            IsActive = true
        };

        var hasher = new PasswordHasher<AdminUser>();
        admin.PasswordHash = hasher.HashPassword(admin, seedPassword);

        dbContext.AdminUsers.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
