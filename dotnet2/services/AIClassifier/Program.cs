using AIClassifier.Data;
using AIClassifier.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AI Classifier Service", Version = "v1" });
});

// PostgreSQL with EF Core
builder.Services.AddDbContext<ClassifierDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Named HttpClients
builder.Services.AddHttpClient("llm");
builder.Services.AddHttpClient("datahub");

// Application services
builder.Services.AddScoped<ILlmService, LlmService>();
builder.Services.AddScoped<IPiiDetectorService, PiiDetectorService>();
builder.Services.AddScoped<IDataHubService, DataHubService>();

var app = builder.Build();

// Ensure tables exist (create from EF model if not present)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClassifierDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
