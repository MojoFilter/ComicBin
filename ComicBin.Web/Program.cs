using ComicBin.Service;
using ComicBin.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .UseComicBinService()
                .UseComicBinWebConfiguration();

builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


var api = app.Services.GetRequiredService<IComicBinApiService>();

app.MapGet("/refresh", api.RefreshComicDatabaseAsync).WithName("RefreshComicDatabase");

app.MapGet("/allbooks", api.GetAllBooksAsync).WithName("AllBooks");

app.MapGet("/cover/{bookId}", api.GetCoverAsync).WithName("Cover");

app.MapPost("/markread", api.MarkReadAsync).WithName("MarkRead");

app.Run();