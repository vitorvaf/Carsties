using Contracts;
using MassTransit;

namespace AuctionService;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("--> Consuming fault: AuctionCreatedFaultConsumer");

        /// TODO: Implementar lógica para tratar a mensagem de erro
        
        var message = context.Message;
        var exception = message.Exceptions[0];
        Console.WriteLine($"AuctionCreatedFaultConsumer: {exception.Message}");
        return Task.CompletedTask;
    }
}
