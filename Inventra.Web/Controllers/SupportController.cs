using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Inventra.Application.Interfaces;

namespace Inventra.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;
        private readonly ILogger<SupportController> _logger;

        public SupportController(ICurrentUserService currentUserService, IConfiguration configuration,IIdentityService identityService, ILogger<SupportController> logger)
        {
            _currentUserService = currentUserService;
            _configuration = configuration;
            _identityService = identityService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(string summary, string priority, string? inventoryTitle, string pageLink)
        {
            var userId = _currentUserService.UserId;
            string reportedBy = "Anonymous";

            if (userId != null)
            {
                var user = await _identityService.FindByIdAsync(userId);
                reportedBy = user?.UserName ?? user?.Email ?? "Unknown";
            }
            var allUsers = await _identityService.GetAllUsersWithRolesAsync();
            var adminEmails = allUsers
                .Where(u => u.IsAdmin)
                .Select(u => u.Email)
                .ToList();

            var ticket = new
            {
                reported_by = reportedBy,
                inventory = inventoryTitle ?? "",
                link = pageLink,
                priority = priority,
                summary = summary,
                admin_emails = adminEmails,
                created_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonSerializer.Serialize(ticket, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var fileName = $"ticket_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var uploaded = await UploadToDropbox(json, fileName);

            if (uploaded)
                TempData["Success"] = "Support ticket created successfully!";
            else
                TempData["Error"] = "Failed to upload ticket. Please try again";

            return Redirect(pageLink);
        }

        private async Task<bool> UploadToDropbox(string content, string fileName)
        {
            var token = _configuration["Dropbox:AccessToken"];

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Dropbox AccessToken is not configured!");
                return false;
            }

            try
            {
                var filePath = $"/tickets/{fileName}";
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var args = new { path = filePath, mode = "add", autorename = true };
                client.DefaultRequestHeaders.Add("Dropbox-API-Arg", JsonSerializer.Serialize(args));

                var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
                byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync("https://content.dropboxapi.com/2/files/upload", byteContent);

                if (!response.IsSuccessStatusCode)
                    _logger.LogError("Dropbox upload failed: {status}", response.StatusCode);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dropbox upload exception");
                return false;
            }
        }
    }
}
