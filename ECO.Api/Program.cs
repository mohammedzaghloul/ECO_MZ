using AutoMapper;
using ECO.Api.Middleware;
using ECO.BLL;
using ECO.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DAL services (DbContext, Repositories)
builder.Services.AddInfrastractionConfiguration(builder.Configuration);

// Add BLL services (Business Logic)
builder.Services.AddApplicationServices();


var app = builder.Build();

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