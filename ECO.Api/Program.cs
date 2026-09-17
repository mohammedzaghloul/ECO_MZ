using AutoMapper;
using ECO.Api.Middleware;
using ECO.BLL;
using ECO.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DAL services 
builder.Services.AddInfrastractionConfiguration(builder.Configuration);

// Add BLL services 
builder.Services.AddApplicationServices();
builder.Services.AddMemoryCache();

var app = builder.Build();
// Error Handling Middleware
app.UseStatusCodePagesWithReExecute("/error/{0}");
// Global Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();