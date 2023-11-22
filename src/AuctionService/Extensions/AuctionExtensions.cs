using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Extensions;

public static class AuctionExtensions
{
    public static void MapAuctionEndpoints(this WebApplication app)
    {

        #region GetAuctions

        app.MapGet("/api/auctions", async (AuctionDbContext _context, IMapper _mapper, string? date) =>
        {
            var query =  _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if(!string.IsNullOrEmpty(date))
            {                
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();           
        });


        app.MapGet("/api/auctions/{id}", async (HttpContext context) =>
        {
            var id = Guid.Parse(context.Request.RouteValues["id"]?.ToString() ?? string.Empty);

            using (var scope = app.Services.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
                var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                var auction = await _context.Auctions
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (auction == null)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var auctionDto = _mapper.Map<AuctionDto>(auction);
                await context.Response.WriteAsJsonAsync(auctionDto);
            }
        });


        #endregion

        #region CreateAuction

        app.MapPost("/api/auctions", async (
            AuctionDbContext _context,
            IMapper _mapper,
            CreateAuctionDto createAuctionDtoauctionDto) =>
        {
            var auction = _mapper.Map<Auction>(createAuctionDtoauctionDto);
            auction.Id = Guid.NewGuid();

            //TODO: add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
                return Results.BadRequest();

            return Results.Created(
                $"/api/auctions/{auction.Id}", _mapper.Map<AuctionDto>(auction));
        });

        #endregion

        #region UpdateAuction

        app.MapPut("/api/auctions/{id}", async (
            AuctionDbContext _context,            
            UpdateAuctionDto updateAuctionDto,
            Guid id) =>
        {

            var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return Results.NotFound();
            }

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
                return Results.BadRequest();

            return Results.Ok();
        });


        #endregion

        #region DeleteAuction

        app.MapDelete("/api/auctions/{id}", async (
            AuctionDbContext _context,
            Guid id) =>
        {

            var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return Results.BadRequest();
            }

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
                return Results.BadRequest();

            return Results.NoContent();
        });

        #endregion

    }

}
