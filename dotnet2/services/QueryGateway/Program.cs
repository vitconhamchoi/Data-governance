using QueryGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient for PolicyService
builder.Services.AddHttpClient("PolicyService", client =>
{
    var policyServiceUrl = builder.Configuration["PolicyServiceUrl"] ?? "http://policy-service:8080";
    client.BaseAddress = new Uri(policyServiceUrl);
});

// Add custom services
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<ITrinoService, TrinoService>();
builder.Services.AddScoped<IMaskingService, MaskingService>();

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
