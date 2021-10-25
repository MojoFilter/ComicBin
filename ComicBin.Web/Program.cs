using ComicBin.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.UseComicBinService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/refresh", async (IComicBinRepo repo, CancellationToken ct) =>
{
    await repo.RefreshAsync(ct).ConfigureAwait(false);
    return "Ok, cool";
})
    .WithName("RefreshComicDatabase");

app.MapGet("/allbooks", (IComicBinRepo repo, CancellationToken ct) => repo.GetAllBooksAsync(ct));

app.Run();