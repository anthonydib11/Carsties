using MongoDB.Entities;
using SearchService.Models;

namespace AuctionService.Services
{
    public class AuctionSrvcHttpClient
    {
        private IConfiguration _config;
        private HttpClient _httpClient;

        public AuctionSrvcHttpClient(HttpClient httpClient, IConfiguration config) {
            
            _config = config;
            _httpClient = httpClient;
        
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var lastItem = await DB.Find<Item>()
                .Sort(s => s.Descending(x => x.UpdatedAt))
                .ExecuteFirstAsync();

            var lastUpdated = lastItem?.UpdatedAt ?? DateTime.MinValue;

            string BaseUrl = _config["AuctionServiceUrl"];
            var dateParam = Uri.EscapeDataString(lastUpdated.ToString("o")); // ISO-8601

            return await _httpClient.GetFromJsonAsync<List<Item>>(
                $"{BaseUrl}/api/auctions?date={dateParam}"
            ) ?? new List<Item>();
        }

    }
}
