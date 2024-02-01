using MongoDB.Entities;

namespace SearchService;

public static class SearchExtensions
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        app.MapGet("/api/search", async ([AsParameters] SearchParams searchParams) =>
        {
            var query = DB.PagedSearch<Item, Item>();

            query.Sort(x => x.Ascending(a => a.Make));
            

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();


            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Ascending(a => a.CreatedAt)),
                "year" => query.Sort(x => x.Ascending(a => a.Year)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                    && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
                query.Match(x => x.Seller == searchParams.Seller);

            if (!string.IsNullOrEmpty(searchParams.Winner))
                query.Match(x => x.Winner == searchParams.Winner);

            searchParams.PageSize ??= 4;
            searchParams.PageNumber ??= 1;

            query.PageNumber(searchParams.PageNumber.Value).PageSize(searchParams.PageSize.Value);

            var result = await query.ExecuteAsync();

            return Results.Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });

        });
    }
}
