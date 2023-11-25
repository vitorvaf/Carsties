using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    private readonly ILogger<AuctionCreatedConsumer> _logger;

    public AuctionCreatedConsumer(IMapper mapper, ILogger<AuctionCreatedConsumer> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        _logger.LogInformation($"--> Comsuming auction created. AuctionCreated: {context.Message.Id}");       
        
        var item = _mapper.Map<Item>(context.Message);

        await item.SaveAsync();        
    }
}
