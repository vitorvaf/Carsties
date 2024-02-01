using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public static class DbInitializer
{
    public static async Task InitDb(this WebApplication app)
    {
        try
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
                .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            using var scope = app.Services.CreateScope();

            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

            var items = await httpClient.GetItemsForSearchDbAsync();

            Console.WriteLine($"Found {items.Count()} items from auction service");

            if (items.Count() > 0) await DB.SaveAsync(items);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
