using com.prodg.photobooth.domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.AddConsole();
builder.Services.AddPhotoBooth(builder.Configuration);
builder.Services.AddHostedService<PhotoBoothHost>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck("Photobooth", () => HealthCheckResult.Healthy("A healthy result."));
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

// app.UseAuthorization();

app.MapControllers();

app.Run();