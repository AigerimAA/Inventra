using Inventra.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    [Authorize]
    public class OdooController : Controller
    {
        private readonly OdooService _odooService;

        public OdooController(OdooService odooService)
        {
            _odooService = odooService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _odooService.GetProductsAsync();
            return View(products);
        }
    }
}
