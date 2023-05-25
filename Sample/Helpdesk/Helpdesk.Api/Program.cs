using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();


var app = builder.Build();

app.Run();
