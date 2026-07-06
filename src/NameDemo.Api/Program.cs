using Microsoft.EntityFrameworkCore;
using NameDemo.Api.Services;
using NameDemo.Data;

var builder = WebApplication.CreateBuilder(args);

var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "http://0.0.0.0:8080";
builder.WebHost.UseUrls(urls);

builder.Services.AddControllers();
builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));
builder.Services.AddHttpClient<IWebhookNotifier, N8nWebhookNotifier>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else
        {
            policy.AllowAnyOrigin();
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapControllers();

app.Logger.LogInformation("Starting NameDemo.Api on {Urls}", urls);

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database migration failed. Check ConnectionStrings__DefaultConnection.");
}

app.Run();
