using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inventra.Infrastructure.Services
{
    public class SalesforceService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesforceService> _logger;
        private readonly HttpClient _httpClient;

        public SalesforceService(IConfiguration configuration, ILogger<SalesforceService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var instanceUrl = _configuration["Salesforce:InstanceUrl"];
            var clientId = _configuration["Salesforce:ClientId"];
            var clientSecret = _configuration["Salesforce:ClientSecret"];

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!)
            });

            var response = await _httpClient.PostAsync($"{instanceUrl}/services/oauth2/token", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Salesforce auth failed: {error}", error);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<bool> CreateAccountAndContactAsync(string accessToken, string firstName, string lastName, string email, string phone, string company)
        {
            var instanceUrl = _configuration["Salesforce:InstanceUrl"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var account = new { Name = company };
            var accountJson = JsonSerializer.Serialize(account);
            var accountResponse = await _httpClient.PostAsync($"{instanceUrl}/services/data/v59.0/sobjects/Account", new StringContent(accountJson, Encoding.UTF8, "application/json"));

            if (!accountResponse.IsSuccessStatusCode)
            {
                var error = await accountResponse.Content.ReadAsStringAsync();
                _logger.LogError("Salesforce Account creation failed: {error}", error);
                return false;
            }

            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var accountDoc = JsonDocument.Parse(accountResult);
            var accountId = accountDoc.RootElement.GetProperty("id").GetString();

            var contact = new
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                AccountId = accountId
            };

            var contactJson = JsonSerializer.Serialize(contact);
            var contactResponse = await _httpClient.PostAsync($"{instanceUrl}/services/data/v59.0/sobjects/Contact", new StringContent(contactJson, Encoding.UTF8, "application/json"));

            if (!contactResponse.IsSuccessStatusCode)
            {
                var error = await contactResponse.Content.ReadAsStringAsync();
                _logger.LogError("Salesforce Contact creation failed: {error}", error);
                return false;
            }

            return true;
        }
    }
}
