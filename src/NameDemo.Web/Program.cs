using NameDemo.Web.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "http://0.0.0.0:8080";
builder.WebHost.UseUrls(urls);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient<NameApiClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl is not configured.");

    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Logger.LogInformation("Starting NameDemo.Web on {Urls}", urls);

app.Run();
