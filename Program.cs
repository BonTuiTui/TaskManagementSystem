using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Areas.Identity.Data;
using TaskManagementSystem.Data;
using TaskManagementSystem.Services;
using TaskManagementSystem.Services.Proxies;
using TaskManagementSystem.Middleware;
using TaskManagementSystem.Services.ProjectFactory;
using TaskManagementSystem.Hubs;
using TaskManagementSystem.Services.Observer;
using TaskManagementSystem.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); // Thêm SignalR
builder.Services.AddTransient<ISenderEmail, EmailSender>();// Forgot Password

builder.Services.AddScoped<IProjectFactory, ProjectFactory>();
builder.Services.AddHttpClient(); // Thêm HttpClient

builder.Services.AddSingleton<TaskNotifier>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Google Authentication
builder.Services.AddAuthentication()
.AddGoogle(options =>
{
    options.ClientId = "271910572021-f32fpqaasav094d48ere0f99f10av66o.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 6;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<UserManagementProxy>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomExceptionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Seed roles and admin user
    await SeedRolesAndAdminUser(roleManager, userManager);
}

app.Run();

async Task SeedRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    string[] roleNames = { "admin", "manager", "employee" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminUser = new ApplicationUser
    {
        UserName = "adminUser",
        Email = "admin@example.com",
        FullName = "admin",
        CreateAt = DateTime.Now
    };

    string adminPassword = "Admin@123";
    var user = await userManager.FindByEmailAsync(adminUser.Email);

    if (user == null)
    {
        var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
        if (createPowerUser.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}