using malAnimeUpdater;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var malConfiguration = builder.Configuration.GetSection(MalOptions.Credentials).Get<MalOptions>();
var tmdbConfiguration = builder.Configuration.GetSection(TmdbConfiguration.Credentials).Get<TmdbConfiguration>();
builder.Services.AddScoped(_ => new MALService(malConfiguration.Malhlogsessid, malConfiguration.Malsessionid, tmdbConfiguration.ApiKey));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

public class TmdbConfiguration
{
    public const string Credentials = "tmdbCredentials";
    
    public string ApiKey { get; set; }
}