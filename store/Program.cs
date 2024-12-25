using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using store.Areas.Identity.Data;
using store.Services;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllersWithViews();

        // Register storeContext with the connection string for Identity management
        builder.Services.AddDbContext<storeContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        // Register ApplicationDbContext for the business data like products and quotes
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });

        // Add Identity services using storeUser and storeContext
        builder.Services.AddDefaultIdentity<storeUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>() // Enable role management
            .AddEntityFrameworkStores<storeContext>(); // Identity uses storeContext

        var app = builder.Build();

        // Seed roles and initial users after the app starts
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<storeUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedRolesAndUsersAsync(userManager, roleManager);
        }

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
        app.Run();
    }

    // Method for seeding roles and creating an initial admin
    private static async Task SeedRolesAndUsersAsync(UserManager<storeUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string adminEmail = "admin@admin.com";
        string adminPassword = "Admin@123";

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Client"))
        {
            await roleManager.CreateAsync(new IdentityRole("Client"));
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new storeUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, adminPassword);
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

}
