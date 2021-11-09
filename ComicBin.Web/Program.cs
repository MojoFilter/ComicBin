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


app.MapGet("/refresh", async (IComicBinRepo repo, CancellationToken ct) =>
{
    await repo.RefreshAsync(ct).ConfigureAwait(false);
    return "Ok, cool";
})
    .WithName("RefreshComicDatabase");

app.MapGet("/allbooks", 
          (IComicBinRepo repo, CancellationToken ct) => repo.GetAllBooksAsync(ct))
   .WithName("AllBooks");

app.MapGet("/cover/{bookId}",
    async (HttpContext context, string bookId, IComicBinRepo repo, CancellationToken ct) =>
    {
        context.Response.ContentType = "image/jpg";
        var image = await repo.GetCoverAsync(bookId).ConfigureAwait(false);
        await image.CopyToAsync(context.Response.Body, ct).ConfigureAwait(false);        
    })
   .WithName("Cover");

app.MapPost("/markread", 
    async (MarkReadRequest req, IComicBinRepo repo, CancellationToken ct) =>
    {
        await repo.MarkReadAsync(req.Read, req.BookIds, ct).ConfigureAwait(false);
    })
   .WithName("MarkRead");

app.Run();