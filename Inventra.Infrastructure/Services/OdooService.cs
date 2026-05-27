using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventra.Infrastructure.Services
{
    public class OdooService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OdooService> _logger;
        private readonly HttpClient _httpClient;

        public OdooService(IConfiguration configuration, ILogger<OdooService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<List<OdooProduct>> GetProductsAsync()
        {
            var url = _configuration["Odoo:Url"];
            var apiKey = _configuration["Odoo:ApiKey"];
            var username = _configuration["Odoo:Username"];
            var database = _configuration["Odoo:Database"];
            var password = _configuration["Odoo:Password"];

            var authBody = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "call",
                @params = new
                {
                    db = database,
                    login = username,
                    password = password
                }
            };

            var authJson = JsonSerializer.Serialize(authBody);
            var authContent = new StringContent(authJson, Encoding.UTF8, "application/json");
            var authResponse = await _httpClient.PostAsync($"{url}/web/session/authenticate", authContent);
            var authResponseJson = await authResponse.Content.ReadAsStringAsync();
            var authDoc = JsonDocument.Parse(authResponseJson);

            if (!authDoc.RootElement.TryGetProperty("result", out var authResult) ||
                authResult.ValueKind == JsonValueKind.Null)
            {
                _logger.LogError("Odoo auth failed: {response}", authResponseJson);
                return new List<OdooProduct>();
            }

            var requestBody = new
            {
                jsonrpc = "2.0",
                id = 2,
                method = "call",
                @params = new
                {
                    model = "product.product",
                    method = "search_read",
                    args = new object[] { new object[] { } },
                    kwargs = new
                    {
                        fields = new[] { "id", "name", "qty_available", "list_price" },
                        limit = 100
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{url}/web/dataset/call_kw/product.product/search_read", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Odoo request failed: {status}", response.StatusCode);
                return new List<OdooProduct>();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            if (!doc.RootElement.TryGetProperty("result", out var result))
            {
                _logger.LogError("Odoo response has no result: {response}", responseJson);
                return new List<OdooProduct>();
            }

            var products = new List<OdooProduct>();
            foreach (var item in result.EnumerateArray())
            {
                products.Add(new OdooProduct
                {
                    Id = item.GetProperty("id").GetInt32(),
                    Name = item.GetProperty("name").GetString() ?? "",
                    QtyAvailable = item.TryGetProperty("qty_available", out var qty) ? qty.GetDouble() : 0,
                    Price = item.TryGetProperty("list_price", out var price) ? price.GetDouble() : 0
                });
            }

            return products;
        }
    }
    public class OdooProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double QtyAvailable { get; set; }
        public double Price { get; set; }
    }
}
