using MongoDB.Entities;

namespace SearchService;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    } 


    public async Task<IEnumerable<Item>> GetItemsForSearchDbAsync()
    {
        var lastUpdate = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        Console.WriteLine($"Last update was {lastUpdate}");
        
        return await _httpClient
            .GetFromJsonAsync<IEnumerable<Item>>($"{_configuration["AuctionServiceUrl"]}/api/auctions?date={lastUpdate}");
        
    }

}
