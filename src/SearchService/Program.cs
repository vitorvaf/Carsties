using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());

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

