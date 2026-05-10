using Inventra.Application.DTOs;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Inventra.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;

        public IdentityService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();

            var userRoleIds = await _context.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Select(ur => new { ur.UserId, ur.RoleId })
                .ToListAsync();

            var roleIds = userRoleIds.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync();

            var roleDict = roles.ToDictionary(r => r.Id, r => r.Name);
            var rolesByUserId = userRoleIds
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key,
                    g => g.Select(ur => roleDict[ur.RoleId]).ToList());

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                IsBlocked = u.IsBlocked,
                IsAdmin = rolesByUserId.GetValueOrDefault(u.Id, []).Contains("Admin"),
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<ApplicationUser?> FindByIdAsync(string userId)
            => await _userManager.FindByIdAsync(userId);

        public async Task<ApplicationUser?> FindByEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email);

        public async Task<AuthResult> CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return ToAuthResult(result);
        }

        public async Task<AuthResult> UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return ToAuthResult(result);
        }

        public async Task<AuthResult> DeleteUserAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return ToAuthResult(result);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string role)
            => await _userManager.IsInRoleAsync(user, role);

        public async Task<AuthResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return ToAuthResult(result);
        }

        public async Task<AuthResult> RemoveFromRoleAsync(ApplicationUser user, string role)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return ToAuthResult(result);
        }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent)
            => await _signInManager.SignInAsync(user, isPersistent);

        public async Task<AuthResult> PasswordSignInAsync(string userName, string password,
            bool rememberMe, bool lockoutOnFailure)
        {
            var result = await _signInManager.PasswordSignInAsync(
                userName, password, rememberMe, lockoutOnFailure);
            return ToAuthResult(result);
        }

        public async Task SignOutAsync()
            => await _signInManager.SignOutAsync();

        public async Task<string> GetExternalLoginRedirectUrlAsync(string provider, string redirectUrl)
        {
            await Task.CompletedTask;
            return provider;
        }

        public async Task<(AuthResult Result, ApplicationUser? User)> ExternalLoginAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return (AuthResult.Failure(["External login info not found"]), null);

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (signInResult.Succeeded)
            {
                var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var existingUser = email != null
                    ? await _userManager.FindByEmailAsync(email)
                    : null;
                return (AuthResult.Success(), existingUser);
            }

            var userEmail = info.Principal.FindFirst(ClaimTypes.Email)?.Value
                ?? info.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value + "@oauth.com";

            if (userEmail == null)
                return (AuthResult.Failure(["Cannot retrieve email from external provider"]), null);

            var existingByEmail = await _userManager.FindByEmailAsync(userEmail);
            if (existingByEmail != null)
            {
                await _userManager.AddLoginAsync(existingByEmail, info);
                await _signInManager.SignInAsync(existingByEmail, isPersistent: false);
                return (AuthResult.Success(), existingByEmail);
            }

            var newUser = new ApplicationUser
            {
                UserName = userEmail.Split('@')[0],
                Email = userEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser,
                "OAuth1_" + Guid.NewGuid().ToString());

            if (!createResult.Succeeded)
                return (ToAuthResult(createResult), null);

            await _userManager.AddLoginAsync(newUser, info);
            await _signInManager.SignInAsync(newUser, isPersistent: false);
            return (AuthResult.Success(), newUser);
        }

        private static AuthResult ToAuthResult(IdentityResult result)
            => result.Succeeded
                ? AuthResult.Success()
                : AuthResult.Failure(result.Errors.Select(e => e.Description));

        private static AuthResult ToAuthResult(SignInResult result)
        {
            if (result.Succeeded) return AuthResult.Success();
            if (result.IsLockedOut) return AuthResult.LockedOut();
            return AuthResult.Failure(["Invalid login attempt"]);
        }
    }
}