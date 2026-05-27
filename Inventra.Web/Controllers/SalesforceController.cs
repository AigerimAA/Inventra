using Inventra.Application.Interfaces;
using Inventra.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    [Authorize]
    public class SalesforceController : Controller
    {
        private readonly SalesforceService _salesforceService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public SalesforceController(SalesforceService salesforceService, ICurrentUserService currentUserService, IIdentityService identityService)
        {
            _salesforceService = salesforceService;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateContact(string firstName, string lastName, string phone, string company)
        {
            var userId = _currentUserService.UserId;
            if (userId == null) return Forbid();

            var user = await _identityService.FindByIdAsync(userId);
            if (user == null) return Forbid();

            var token = await _salesforceService.GetAccessTokenAsync();
            if (token == null)
            {
                TempData["Error"] = "Failed to connect to Salesforce.";
                return RedirectToAction("Index", "Profile");
            }
            var success = await _salesforceService.CreateAccountAndContactAsync(token, firstName, lastName, user.Email!, phone, company);

            TempData[success ? "SalesforceSuccess" : "SalesforceError"] = success
                ? "Your contact was successfully created in Salesforce!"
                : "Failed to create contact in Salesforce.";

            return RedirectToAction("Index", "Profile");
        }
    }
}
