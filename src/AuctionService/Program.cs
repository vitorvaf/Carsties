using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AuctionService.Extensions;
using MassTransit;
using AuctionService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AuctionDbContext>(options => 
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});    

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
    {
        options.QueryDelay = TimeSpan.FromSeconds(10);

        options.UsePostgres();
        options.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();    
    
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => {
            h.Username("admin");
            h.Password("admin");
        });

        cfg.ConfigureEndpoints(context);

    });
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapAuctionEndpoints();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.Run();
