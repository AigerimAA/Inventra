using Inventra.Application;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Infrastructure;
using Inventra.Infrastructure.Persistence;
using Inventra.Web.Hubs;
using Inventra.Web.Middleware;
using Inventra.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace Inventra.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Inventra API",
                    Version = "v1",
                    Description = "API for Inventra inventory management system"
                });
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddSignalR();
            builder.Services.AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var githubClientId = builder.Configuration["Authentication:GitHub:ClientId"];

            var authBuilder = builder.Services.AddAuthentication();

            if (!string.IsNullOrEmpty(googleClientId))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                });
            }

            if (!string.IsNullOrEmpty(githubClientId))
            {
                authBuilder.AddGitHub(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
                    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
                    options.Scope.Add("user:email");
                });
            }

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/Login";
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (db.Database.IsRelational())
                    db.Database.Migrate();
                else
                    db.Database.EnsureCreated();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new IdentityRole("Admin"));

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var adminEmail = configuration["AdminSettings:Email"]
                    ?? throw new InvalidOperationException("AdminSettings:Email is not configured");
                var adminUserName = configuration["AdminSettings:UserName"]
                    ?? throw new InvalidOperationException("AdminSettings:UserName is not configured");
                var adminPassword = configuration["AdminSettings:Password"]
                    ?? throw new InvalidOperationException("AdminSettings:Password is not configured");

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminUserName,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(adminUser, adminPassword);
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else if (!adminUser.EmailConfirmed)
                {
                    adminUser.EmailConfirmed = true;
                    await userManager.UpdateAsync(adminUser);
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventra API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            var supportedCultures = new[] { "en", "ru", "kk" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            app.UseRequestLocalization(localizationOptions);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
public partial class Program { }
