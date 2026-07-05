using NameDemo.Web.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
