using Inventra.Application.Search.Queries;
using Inventra.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventra.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IMediator _mediator;
        public SearchController(IMediator mediator) => _mediator = mediator;

        public async Task<IActionResult> Index(string q, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new SearchResultViewModel());

            ViewData["SearchQuery"] = q;
            var result = await _mediator.Send(new SearchQuery(q), cancellationToken);

            var viewModel = new SearchResultViewModel
            {
                Query = q,
                Inventories = result.Inventories.Select(i => new SearchInventoryResult
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    CategoryName = i.CategoryName,
                    OwnerName = i.OwnerName,
                    ItemsCount = i.ItemsCount
                }).ToList(),

                Items = result.Items.Select(i => new SearchItemResult
                {
                    Id = i.Id,
                    CustomId = i.CustomId,
                    InventoryId = i.InventoryId,
                    InventoryTitle = i.InventoryTitle,
                    CreatedByName = i.CreatedByName
                }).ToList()
            };
            return View(viewModel);
        }
    }
}
