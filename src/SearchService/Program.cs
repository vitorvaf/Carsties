using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddMassTransit(x =>
{
    //Ler documentação para entender como são criadas as filas a partir dessa declaração
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {       
        
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);        
        });

        cfg.ReceiveEndpoint("search-auction-updated", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));            
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);            
        });

        cfg.ReceiveEndpoint("search-auction-deleted", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));            
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);            
        });

        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:UserName", "admin"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password","admin"));
        });

        cfg.ConfigureEndpoints(context);

    });
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(async () =>
{    
     await app.InitDb();           
});

app.MapSearchEndpoints();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(3));
}

