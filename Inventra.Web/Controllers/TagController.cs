using Inventra.Application.Interfaces;
using Inventra.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagRepository _tagRepository;

        public TagController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<string>());

            var tags = await _tagRepository.GetByPrefixAsync(query);
            return Json(tags.Select(t => t.Name).ToList());
        }
    }
}
