using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.Data
{
    public class DbInitializer
    {

        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchServiceDB", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(item => item.Make, KeyType.Text)
                .Key(item => item.Model, KeyType.Text)
                .Key(item => item.Year, KeyType.Text)
                .Key(item => item.Color, KeyType.Text)
                .Key(item => item.Mileage, KeyType.Text)
                .Option(o => o.Name = "Item_Search_Index")
                .CreateAsync();

            var Count = await DB.CountAsync<Item>();

            using var scope = app.Services.CreateScope();   

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionService.Services.AuctionSrvcHttpClient>();

            if (Count == 0)
            {
                var items = await httpClient.GetItemsForSearchDb();
                foreach (var item in items)
                {
                    await item.SaveAsync();
                }
            }
        }
    }
}
