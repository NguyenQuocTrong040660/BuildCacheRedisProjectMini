using BuildCacheRedisProjectMini.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();

// Enable Serilog Request Logging
app.UseSerilogRequestLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
