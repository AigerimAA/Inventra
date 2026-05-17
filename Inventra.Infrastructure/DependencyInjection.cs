using CloudinaryDotNet;
using Inventra.Application.Interfaces;
using Inventra.Domain.Interfaces;
using Inventra.Infrastructure.Options;
using Inventra.Infrastructure.Persistence;
using Inventra.Infrastructure.Repositories;
using Inventra.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventra.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null)));

            services.AddMemoryCache();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ICustomIdGenerator, CustomIdGeneratorService>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IInventoryPermissionService, InventoryPermissionService>();
            services.AddScoped<IAccessRepository, AccessRepository>();
            services.AddScoped<IInventoryStatsRepository, InventoryStatsRepository>();
            services.AddScoped<ICustomIdRepository, CustomIdRepository>();
            services.Configure<SearchOptions>(configuration.GetSection("Search"));

            var cloudName = configuration["Cloudinary:CloudName"];
            if (!string.IsNullOrEmpty(cloudName))
            {
                var cloudinaryAccount = new Account(
                    cloudName,
                    configuration["Cloudinary:ApiKey"],
                    configuration["Cloudinary:ApiSecret"]);
                services.AddSingleton(new Cloudinary(cloudinaryAccount));
                services.AddScoped<ICloudStorageService, CloudStorageService>();
            }
            else
            {
                services.AddScoped<ICloudStorageService, NullCloudStorageService>();
            }

            var brevoLogin = configuration["Brevo:Login"];
            if (!string.IsNullOrEmpty(brevoLogin))
            {
                services.AddScoped<IEmailService, BrevoEmailService>();
            }
            else
            {
                services.AddScoped<IEmailService, NullEmailService>();
            }

            return services;
        }
    }
}
