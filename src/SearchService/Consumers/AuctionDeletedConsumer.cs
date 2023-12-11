using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper;
    private readonly ILogger<AuctionDeletedConsumer> _logger;

    public AuctionDeletedConsumer(IMapper mapper, ILogger<AuctionDeletedConsumer> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        _logger.LogInformation($"--> Comsuming auction deleted. AuctionDeleted: {context.Message.Id}");       
        
        var item = _mapper.Map<Item>(context.Message);       

        await item.DeleteAsync();  
        
    }
}
